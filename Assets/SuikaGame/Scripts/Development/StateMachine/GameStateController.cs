using System;
using System.Collections.Generic;
using Development.StateMachine.States;
using Development.Utils;

namespace Development.StateMachine
{
    public class GameStateController
    {
        private readonly GameStateMachine _stateMachine;
        private readonly GameContext _context;
        private readonly List<IDisposable> _stateDispose = new();

        public GameStateController(GameStateMachine stateMachine, GameContext context)
        {
            MLog.Log("Create state machine");
            _stateMachine = stateMachine;
            _context = context;
            RegisterStates();
        }

        public GameState CurrentState => _stateMachine.State;

        private void RegisterStates()
        {
            _stateDispose.Add(new LoadedState(_stateMachine, _context));
            _stateDispose.Add(new StartedState(_stateMachine, _context));
            _stateDispose.Add(new LostState(_stateMachine, _context));
            _stateDispose.Add(new PausedState(_stateMachine, _context));
        }
        
        public void Destroy()
        {
            foreach (var t in _stateDispose)
            {
                t.Dispose();
            }
        }
    }
}
