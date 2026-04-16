using System;
using Controllers;
using Core;
using Cysharp.Threading.Tasks;
using Development.Managers;
using UnityEngine;

namespace Pools
{
    public class BallSpawner : MonoBehaviour
    {
        [SerializeField] private BallPool ballPool;
        [SerializeField] private BallCoreDatabase ballCoreDatabase;
        [SerializeField] private SkinDatabase skinDatabase;
        [SerializeField] private Transform dynamicRoot;
        [SerializeField] private int spawnBallID = 1;
        [SerializeField] private int randomMinBallID = 1;
        [SerializeField] private int randomMaxBallID = 3;
        [SerializeField] private int activeSkinSeriesID = 1;
        [SerializeField] private float spawnCooldown = 1f;
        private bool _isCoolingDown = false;

        private void OnEnable()
        {
            EventManager.SameIdCollision += HandleSameIdCollision;
        }

        private void OnDisable()
        {
            EventManager.SameIdCollision -= HandleSameIdCollision;
        }

        public Ball SpawnAndAttach(Transform parent)
        {
            if (_isCoolingDown) return null;

            Ball ball = ballPool.GetBall();
        
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;

            RandomizeSpawnBallID();
            BallData data = ResolveBallData();

            if (data != null)
                ball.Setup(data, skinDatabase, activeSkinSeriesID);
            StartCooldown().Forget();
            return ball;
        }

        public async UniTask<Ball> ReleaseAndRespawn(Ball oldBall, Transform parent)
        {
            if (_isCoolingDown) return null;

            _isCoolingDown = true;

            if (oldBall != null)
            {
                oldBall.Release(dynamicRoot);
                oldBall = null;
                
                await UniTask.Delay(TimeSpan.FromSeconds(spawnCooldown));
            }

            Ball ball = SpawnWithoutCooldown(parent);

            _isCoolingDown = false;
            return ball;
        }
        
        private Ball SpawnWithoutCooldown(Transform parent)
        {
            Ball ball = ballPool.GetBall();
            if (ball == null) return null;

            if (parent != null)
            {
                ball.transform.SetParent(parent);
                ball.transform.localPosition = Vector3.zero;
            }

            RandomizeSpawnBallID();
            BallData data = ResolveBallData();

            if (data != null)
                ball.Setup(data, skinDatabase, activeSkinSeriesID);

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

        private void HandleSameIdCollision(Ball firstBall, Ball secondBall)
        {
            if (firstBall == null || secondBall == null) return;
            if (ballPool == null || ballCoreDatabase == null) return;

            int firstID = firstBall.ID;
            int secondID = secondBall.ID;
            if (firstID < 0 || firstID != secondID) return;

            Vector3 mergePosition = (firstBall.transform.position + secondBall.transform.position) * 0.5f;

            ballPool.ReturnPool(firstBall);
            ballPool.ReturnPool(secondBall);

            int nextBallID = firstID + 1;
            SpawnMergedBall(nextBallID, mergePosition);
        }

        private void SpawnMergedBall(int ballID, Vector3 worldPosition)
        {
            BallData nextBallData = ballCoreDatabase.GetBallData(ballID);
            if (nextBallData == null) return;

            Ball mergedBall = ballPool.GetBall();
            if (mergedBall == null) return;

            mergedBall.transform.SetParent(null);
            mergedBall.transform.position = worldPosition;
            mergedBall.Setup(nextBallData, skinDatabase, activeSkinSeriesID);
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
            if (ballCoreDatabase == null || ballCoreDatabase.ballDatas == null || ballCoreDatabase.ballDatas.Count == 0)
                return null;

            var ballData = ballCoreDatabase.GetBallData(spawnBallID);
            if (ballData != null) return ballData;

            return ballCoreDatabase.ballDatas[0];
        }

        private async UniTask StartCooldown()
        {
            _isCoolingDown = true;
            await UniTask.Delay(TimeSpan.FromSeconds(spawnCooldown));
            _isCoolingDown = false;
        }
    }
}
