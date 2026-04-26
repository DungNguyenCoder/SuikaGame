namespace Development.StateMachine
{
    public abstract class BaseGameState : State<GameState, GameTrigger, GameContext>
    {
        protected BaseGameState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {

        }
    }
}
