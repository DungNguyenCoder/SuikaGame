using System;

namespace Development.StateMachine.States
{
    public class LoadedState : BaseGameState, IDisposable
    {
        public LoadedState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Loaded)
                .OnActivate(OnActivate)
                .Permit(GameTrigger.Start, GameState.Started);
        }

        private void OnActivate()
        {
            StateMachine.Fire(GameTrigger.Start);
        }

        public void Dispose()
        {
        }
    }
}
