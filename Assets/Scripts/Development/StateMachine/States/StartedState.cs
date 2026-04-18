using System;
using Development.Managers;

namespace Development.StateMachine.States
{
    public class StartedState : BaseGameState, IDisposable
    {
        public StartedState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Started)
                .OnEntry(OnEntry)
                .OnExit(OnExit)
                .Permit(GameTrigger.Lose, GameState.Lost);
        }

        private void OnEntry()
        {
            EventManager.OnLoseLevel += HandleLoseLevel;
        }

        private void HandleLoseLevel()
        {
            StateMachine.Fire(GameTrigger.Lose);
        }

        private void OnExit()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
        }

        public void Dispose()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
        }
    }
}
