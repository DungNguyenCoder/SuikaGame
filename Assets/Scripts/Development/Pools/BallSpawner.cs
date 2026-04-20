using System;
using System.Collections.Generic;
using Core;
using Core.Ball;
using Core.Skin;
using Cysharp.Threading.Tasks;
using Development.Controllers;
using Development.LoadSave.Data;
using Development.Managers;
using UnityEngine;

namespace Development.Pools
{
    public class BallSpawner : MonoBehaviour
    {
        [SerializeField] private Transform dynamicRoot;
        [SerializeField] private float spawnCooldown = 0.25f;
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

            Ball mergedBall = _ballPool.GetBall();
            mergedBall.transform.SetParent(null);
            mergedBall.transform.position = worldPosition;
            mergedBall.Setup(nextBallData, _skinDatabase, activeSkinSeriesID);
            mergedBall.Release(dynamicRoot);
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
            BallData ballData = _ballDatabase.GetBallData(savedBall.BallId);
            Ball restoredBall = _ballPool.GetBall();
            restoredBall.transform.SetParent(null);
            restoredBall.transform.position = savedBall.GetPosition();
            restoredBall.Setup(ballData, _skinDatabase, activeSkinSeriesID);
            restoredBall.Release(dynamicRoot);
            restoredBall.SetMotion(savedBall.GetVelocity(), savedBall.AngularVelocity);
        }
    }
}
