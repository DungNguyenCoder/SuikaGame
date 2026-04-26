using System;
using Development.Managers;
using Development.Utils;
using UnityEngine;

namespace Development.StateMachine.States
{
    public class PausedState : BaseGameState, IDisposable
    {
        public PausedState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Paused)
                .OnEntry(OnEnter)
                .OnExit(OnExit)
                .Permit(GameTrigger.Resume, GameState.Started);
        }

        private void OnEnter()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.PAUSE_PANEL);
            EventManager.OnRequestResume += HandleRequestResume;
            Time.timeScale = 0f;
        }

        private void HandleRequestResume()
        {
            PanelManager.Instance.ClosePanel(PanelConfig.PAUSE_PANEL);
            StateMachine.Fire(GameTrigger.Resume);
        }

        private void OnExit()
        {
            EventManager.OnRequestResume -= HandleRequestResume;
        }

        public void Dispose()
        {
            EventManager.OnRequestResume -= HandleRequestResume;
        }
    }
}
