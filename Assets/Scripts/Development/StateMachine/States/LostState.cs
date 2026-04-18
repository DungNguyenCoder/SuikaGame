using System;
using UnityEngine.SceneManagement;

namespace Development.StateMachine.States
{
    public class LostState : BaseGameState, IDisposable
    {
        public LostState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Lost)
                .OnEntry(OnEntry)
                .Permit(GameTrigger.Restart, GameState.Started);
        }

        private void OnEntry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Dispose()
        {
        }
    }
}
