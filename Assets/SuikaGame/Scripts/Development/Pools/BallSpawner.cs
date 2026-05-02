using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Core.Ball;
using SuikaGame.Scripts.Core.Skin;
using SuikaGame.Scripts.Development.Controllers;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Pools
{
    public class BallSpawner : MonoBehaviour
    {
        [SerializeField] private Transform dynamicRoot;
        [SerializeField] private float spawnCooldown = 0.25f;
        [SerializeField] private float shuffleRespawnDelay = 0.2f;
        [SerializeField] private float shuffleSpawnInterval = 0.06f;
        [SerializeField] private float shuffleFallHeight = 4f;
        [SerializeField] private float shuffleMinWidth = 4f;
        [SerializeField] private float shuffleVerticalSpacing = 0.35f;
        [SerializeField] private float shuffleMergeUnlockDelay = 0.6f;
        [SerializeField] private float destroyAnimationDuration = 0.18f;
        [SerializeField] private float promotionAnimationDuration = 0.24f;
        [SerializeField] private float promotionPeakScale = 1.18f;
        [SerializeField] private int spawnBallID = 1;
        [SerializeField] private int randomMinBallID = 1;
        [SerializeField] private int randomMaxBallID = 3;
        [SerializeField] private int activeSkinSeriesID = 1;
        private bool _isCoolingDown;
        private BallDatabase _ballDatabase;
        private SkinDatabase _skinDatabase;
        private BallPool _ballPool;

        private void OnEnable()
        {
            EventManager.SameIdCollision += HandleSameIdCollision;
        }

        private void OnDisable()
        {
            EventManager.SameIdCollision -= HandleSameIdCollision;
        }

        public void Init(BallDatabase ballDatabase, SkinDatabase skinDatabase, BallPool ballPool)
        {
            _ballDatabase = ballDatabase;
            _skinDatabase = skinDatabase;
            _ballPool = ballPool;
        }

        public Ball SpawnAndAttach(Transform parent)
        {
            Ball ball = _ballPool.GetBall();
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;

            RandomizeSpawnBallID();
            ball.Setup(ResolveBallData(), _skinDatabase, activeSkinSeriesID);
            return ball;
        }

        public async UniTask<Ball> ReleaseAndRespawn(Ball oldBall, Transform parent)
        {
            // if (_isCoolingDown)
            //     throw new InvalidOperationException("BallSpawner is cooling down. ReleaseAndRespawn was called concurrently.");

            _isCoolingDown = true;
            try
            {
                oldBall.Release(dynamicRoot);
                await UniTask.Delay(TimeSpan.FromSeconds(spawnCooldown));

                return SpawnWithoutCooldown(parent);
            }
            finally
            {
                _isCoolingDown = false;
            }
        }

        private Ball SpawnWithoutCooldown(Transform parent)
        {
            Ball ball = _ballPool.GetBall();
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;

            RandomizeSpawnBallID();
            ball.Setup(ResolveBallData(), _skinDatabase, activeSkinSeriesID);
            return ball;
        }

        public void SetSpawnBallID(int ballID)
        {
            spawnBallID = ballID;
        }

        public void SetActiveSkinSeriesID(int seriesID)
        {
            activeSkinSeriesID = seriesID;
        }

        public void ReturnToPool(Ball ball)
        {
            _ballPool.ReturnPool(ball);
        }

        public void FillReleasedBalls(List<Ball> output)
        {
            _ballPool.FillReleasedBalls(output);
        }

        public void RestoreReleasedBalls(List<BallSaveData> savedBalls)
        {
            _ballPool.ReturnAllReleasedBalls();
            foreach (BallSaveData savedBall in savedBalls)
            {
                SpawnReleasedBall(savedBall);
            }
        }

        public async UniTask<bool> RemoveRandomReleasedBallsAsync(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            List<Ball> releasedBalls = GetReleasedBalls();
            if (releasedBalls.Count == 0)
            {
                return false;
            }

            int removeCount = Mathf.Min(amount, releasedBalls.Count);
            List<Ball> selectedBalls = new List<Ball>(removeCount);
            for (int i = 0; i < removeCount; i++)
            {
                int selectedIndex = UnityEngine.Random.Range(0, releasedBalls.Count);
                Ball selectedBall = releasedBalls[selectedIndex];
                releasedBalls.RemoveAt(selectedIndex);
                selectedBalls.Add(selectedBall);
            }

            await ReturnBallsToPoolWithDisappearAsync(selectedBalls);
            return true;
        }

        public async UniTask<bool> PromoteRandomReleasedBallsAsync(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            List<Ball> eligibleBalls = GetReleasedBalls();
            eligibleBalls.RemoveAll(ball => _ballDatabase.GetBallData(ball.ID + 1) == null);
            if (eligibleBalls.Count == 0)
            {
                return false;
            }

            int promoteCount = Mathf.Min(amount, eligibleBalls.Count);
            UniTask[] tasks = new UniTask[promoteCount];
            for (int i = 0; i < promoteCount; i++)
            {
                int selectedIndex = UnityEngine.Random.Range(0, eligibleBalls.Count);
                Ball selectedBall = eligibleBalls[selectedIndex];
                eligibleBalls.RemoveAt(selectedIndex);
                tasks[i] = PromoteBallAsync(selectedBall);
            }

            await UniTask.WhenAll(tasks);
            return true;
        }

        public async UniTask<bool> RemoveBiggestReleasedBallAsync()
        {
            List<Ball> releasedBalls = GetReleasedBalls();
            if (releasedBalls.Count == 0)
            {
                return false;
            }

            int biggestBallId = releasedBalls[0].ID;
            for (int i = 1; i < releasedBalls.Count; i++)
            {
                if (releasedBalls[i].ID > biggestBallId)
                {
                    biggestBallId = releasedBalls[i].ID;
                }
            }

            releasedBalls.RemoveAll(ball => ball.ID != biggestBallId);
            int selectedIndex = UnityEngine.Random.Range(0, releasedBalls.Count);
            await ReturnBallsToPoolWithDisappearAsync(new List<Ball> { releasedBalls[selectedIndex] });
            return true;
        }

        public async UniTask<bool> ShuffleReleasedBallsAsync()
        {
            List<Ball> releasedBalls = GetReleasedBalls();
            if (releasedBalls.Count == 0)
            {
                return false;
            }

            List<int> ballIds = new List<int>(releasedBalls.Count);
            Bounds shuffleBounds = BuildShuffleBounds(releasedBalls);
            for (int i = 0; i < releasedBalls.Count; i++)
            {
                ballIds.Add(releasedBalls[i].ID);
                _ballPool.ReturnPool(releasedBalls[i]);
            }

            Shuffle(ballIds);

            await UniTask.Delay(TimeSpan.FromSeconds(shuffleRespawnDelay), cancellationToken: this.GetCancellationTokenOnDestroy());

            for (int i = 0; i < ballIds.Count; i++)
            {
                Vector3 spawnPosition = CreateShuffleSpawnPosition(shuffleBounds, i);
                Ball shuffledBall = SpawnReleasedBallById(ballIds[i], spawnPosition, Vector2.zero, 0f);
                if (shuffledBall != null)
                {
                    releasedBalls[i] = shuffledBall;
                    shuffledBall.SetBoosterLocked(true);
                }

                if (shuffleSpawnInterval > 0f && i < ballIds.Count - 1)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(shuffleSpawnInterval), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(shuffleMergeUnlockDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
            foreach (Ball ball in releasedBalls)
            {
                if (ball != null)
                {
                    ball.SetBoosterLocked(false);
                }
            }

            return true;
        }

        public Ball SpawnAndAttachById(Transform parent, int ballId)
        {
            Ball ball = _ballPool.GetBall();
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;
            ball.Setup(_ballDatabase.GetBallData(ballId), _skinDatabase, activeSkinSeriesID);
            return ball;
        }

        private void HandleSameIdCollision(Ball firstBall, Ball secondBall)
        {
            int firstID = firstBall.ID;
            int secondID = secondBall.ID;
            if (firstID != secondID) return;

            Vector3 mergePosition = (firstBall.transform.position + secondBall.transform.position) * 0.5f;

            _ballPool.ReturnPool(firstBall);
            _ballPool.ReturnPool(secondBall);

            int nextBallID = firstID + 1;
            SpawnMergedBall(nextBallID, mergePosition);
        }

        private void SpawnMergedBall(int ballID, Vector3 worldPosition)
        {
            BallData nextBallData = _ballDatabase.GetBallData(ballID);
            if (nextBallData == null) return;

            SpawnReleasedBallById(ballID, worldPosition, Vector2.zero, 0f);
        }

        private void RandomizeSpawnBallID()
        {
            int minID = Mathf.Min(randomMinBallID, randomMaxBallID);
            int maxID = Mathf.Max(randomMinBallID, randomMaxBallID);
            spawnBallID = UnityEngine.Random.Range(minID, maxID + 1);
        }

        private BallData ResolveBallData()
        {
            BallData ballData = _ballDatabase.GetBallData(spawnBallID);
            // if (ballData == null)
            //     throw new InvalidOperationException($"BallData ID {spawnBallID} was not found.");

            return ballData;
        }

        private void SpawnReleasedBall(BallSaveData savedBall)
        {
            SpawnReleasedBallById(savedBall.BallId, savedBall.GetPosition(), savedBall.GetVelocity(), savedBall.AngularVelocity);
        }

        private Ball SpawnReleasedBallById(int ballId, Vector3 worldPosition, Vector2 velocity, float angularVelocity)
        {
            BallData ballData = _ballDatabase.GetBallData(ballId);
            if (ballData == null)
            {
                return null;
            }

            Ball releasedBall = _ballPool.GetBall();
            releasedBall.transform.SetParent(null);
            releasedBall.transform.position = worldPosition;
            releasedBall.Setup(ballData, _skinDatabase, activeSkinSeriesID);
            releasedBall.Release(dynamicRoot);
            releasedBall.SetMotion(velocity, angularVelocity);
            return releasedBall;
        }

        private async UniTask PromoteBallAsync(Ball ball)
        {
            int promotedBallId = ball.ID + 1;
            BallData promotedBallData = _ballDatabase.GetBallData(promotedBallId);
            if (promotedBallData == null)
            {
                return;
            }

            await ball.PlayPromotionAsync(promotedBallData, _skinDatabase, activeSkinSeriesID, promotionPeakScale, promotionAnimationDuration);
        }

        private async UniTask ReturnBallsToPoolWithDisappearAsync(List<Ball> balls)
        {
            UniTask[] tasks = new UniTask[balls.Count];
            for (int i = 0; i < balls.Count; i++)
            {
                tasks[i] = balls[i].PlayDisappearAsync(destroyAnimationDuration);
            }

            await UniTask.WhenAll(tasks);

            foreach (Ball ball in balls)
            {
                _ballPool.ReturnPool(ball);
            }
        }

        private List<Ball> GetReleasedBalls()
        {
            List<Ball> releasedBalls = new List<Ball>();
            FillReleasedBalls(releasedBalls);
            return releasedBalls;
        }

        private Bounds BuildShuffleBounds(List<Ball> releasedBalls)
        {
            Bounds bounds = new Bounds(releasedBalls[0].transform.position, Vector3.zero);
            for (int i = 1; i < releasedBalls.Count; i++)
            {
                bounds.Encapsulate(releasedBalls[i].transform.position);
            }

            if (bounds.size.x < shuffleMinWidth)
            {
                bounds.Expand(new Vector3(shuffleMinWidth - bounds.size.x, 0f, 0f));
            }

            return bounds;
        }

        private Vector3 CreateShuffleSpawnPosition(Bounds bounds, int index)
        {
            float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
            float y = bounds.max.y + shuffleFallHeight + index * shuffleVerticalSpacing;
            return new Vector3(x, y, 0f);
        }

        private static void Shuffle<T>(List<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int selectedIndex = UnityEngine.Random.Range(0, i + 1);
                (items[i], items[selectedIndex]) = (items[selectedIndex], items[i]);
            }
        }
    }
}
