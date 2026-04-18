using System;

namespace Development.StateMachine.States
{
    public class LoadedState : BaseGameState, IDisposable
    {
        public LoadedState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Loaded)
                .Permit(GameTrigger.Start, GameState.Started);
        }

        public void Dispose()
        {
        }
    }
}
