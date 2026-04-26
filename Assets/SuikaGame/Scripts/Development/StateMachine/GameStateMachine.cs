using Stateless;

namespace Development.StateMachine
{
    public class GameStateMachine : StateMachine<GameState, GameTrigger>
    {
        public GameStateMachine(GameState initialState) : base(initialState)
        {
        }
    }
}
