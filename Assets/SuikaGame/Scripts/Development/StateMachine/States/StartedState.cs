using System;
using Development.Controllers;
using Development.Managers;
using UnityEngine;

namespace Development.StateMachine.States
{
    public class StartedState : BaseGameState, IDisposable
    {
        private const int BaseMergeScore = 50;
        private const int MaxBallId = 10;

        public StartedState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Started)
                .OnEntry(OnEntry)
                .OnExit(OnExit)
                .Permit(GameTrigger.Pause, GameState.Paused)
                .Permit(GameTrigger.Lose, GameState.Lost);
        }

        private void OnEntry()
        {
            Time.timeScale = 1f;
            EventManager.OnLoseLevel += HandleLoseLevel;
            EventManager.OnRequestPause += HandleRequestPause;
            EventManager.SameIdCollision += HandleSameIdCollision;
        }

        private void HandleLoseLevel()
        {
            StateMachine.Fire(GameTrigger.Lose);
        }

        private void HandleRequestPause()
        {
            StateMachine.Fire(GameTrigger.Pause);
        }

        private void HandleSameIdCollision(Ball firstBall, Ball secondBall)
        {
            int mergedBallId = firstBall.ID;
            if (mergedBallId >= MaxBallId)
            {
                return;
            }

            int scoreToAdd = CalculateMergeScore(mergedBallId);
            Context.ProgressSaveData.CurrentScore += scoreToAdd;

            int currentScore = Context.ProgressSaveData.CurrentScore;
            if (currentScore > Context.PlayerSaveData.HighScore)
            {
                Context.PlayerSaveData.HighScore = currentScore;
            }

            EventManager.OnScoreChanged?.Invoke(currentScore, Context.PlayerSaveData.HighScore);
        }

        private int CalculateMergeScore(int ballId)
        {
            return BaseMergeScore << (ballId - 1);
        }

        private void OnExit()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
            EventManager.OnRequestPause -= HandleRequestPause;
            EventManager.SameIdCollision -= HandleSameIdCollision;
        }

        public void Dispose()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
            EventManager.OnRequestPause -= HandleRequestPause;
            EventManager.SameIdCollision -= HandleSameIdCollision;
        }
    }
}
