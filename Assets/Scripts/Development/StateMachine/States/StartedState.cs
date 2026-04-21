using System;
using Development.Managers;
using UnityEngine;

namespace Development.StateMachine.States
{
    public class StartedState : BaseGameState, IDisposable
    {
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

        }

        private void HandleLoseLevel()
        {
            StateMachine.Fire(GameTrigger.Lose);
        }

        private void HandleRequestPause()
        {
            StateMachine.Fire(GameTrigger.Pause);
        }

        private void OnExit()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
            EventManager.OnRequestPause -= HandleRequestPause;
        }

        public void Dispose()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
            EventManager.OnRequestPause -= HandleRequestPause;
        }
    }
}
