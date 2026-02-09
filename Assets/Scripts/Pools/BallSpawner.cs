using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private BallPool ballPool;
    [SerializeField] private BallDatabase ballDatabase;
    [SerializeField] private float spawnCooldown = 1f;
    private bool _isCoolingDown = false;

    public Ball SpawnAndAttach(Transform parent)
    {
        if (ballPool == null) return null;
        // Enforce cooldown between manual spawns using UniTask delay (non-blocking)
        if (_isCoolingDown) return null;

        Ball ball = ballPool.GetBall();
        if (ball == null) return null;

        if (parent != null)
        {
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;
        }

        BallData data = null;
        if (ballDatabase != null && ballDatabase.ballDatas != null && ballDatabase.ballDatas.Count > 0)
            data = ballDatabase.ballDatas[0];

        if (data != null)
            ball.Setup(data, ballDatabase);
        // start cooldown after manual spawn so subsequent spawns/releases wait
        StartCooldown().Forget();
        return ball;
    }

    public async UniTask<Ball> ReleaseAndRespawn(Ball oldBall, Transform parent)
    {
        if (_isCoolingDown) return null;

        _isCoolingDown = true;

        if (oldBall != null)
        {
            oldBall.Release();
            oldBall = null;

            // wait before respawning (non-blocking)
            await UniTask.Delay(TimeSpan.FromSeconds(spawnCooldown));
        }

        Ball ball = SpawnWithoutCooldown(parent);

        _isCoolingDown = false;
        return ball;
    }

    // Internal helper that spawns without checking the cooldown.
    private Ball SpawnWithoutCooldown(Transform parent)
    {
        if (ballPool == null) return null;

        Ball ball = ballPool.GetBall();
        if (ball == null) return null;

        if (parent != null)
        {
            ball.transform.SetParent(parent);
            ball.transform.localPosition = Vector3.zero;
        }

        BallData data = null;
        if (ballDatabase != null && ballDatabase.ballDatas != null && ballDatabase.ballDatas.Count > 0)
            data = ballDatabase.ballDatas[0];

        if (data != null)
            ball.Setup(data, ballDatabase);

        return ball;
    }

    private async UniTask StartCooldown()
    {
        _isCoolingDown = true;
        await UniTask.Delay(TimeSpan.FromSeconds(spawnCooldown));
        _isCoolingDown = false;
    }
}