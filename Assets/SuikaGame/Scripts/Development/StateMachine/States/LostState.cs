using System;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development.StateMachine.States
{
    public class LostState : BaseGameState, IDisposable
    {
        public LostState(GameStateMachine stateMachine, GameContext context) : base(stateMachine, context)
        {
            stateMachine.Configure(GameState.Lost)
                .OnEntry(OnEntry);
        }

        private void OnEntry()
        {
            Time.timeScale = 0f;
            PanelManager.Instance.OpenPanel(PanelConfig.LOSE_PANEL);
        }

        public void Dispose()
        {
        }
    }
}
