using System;
using Core;
using Core.Ball;
using Core.Skin;
using Development.Controllers;
using Development.InputSystem;
using Development.Pools;
using Development.StateMachine;
using UnityEngine;

namespace Development
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private BallDatabase ballDatabase;
        [SerializeField] private SkinDatabase skinDatabase;
        [SerializeField] private BallSpawner ballSpawner;
        [SerializeField] private BallPool ballPool;
        [SerializeField] private InputController inputController;
        [SerializeField] private Cloud cloud;

        private GameStateController _gameStateController;
        private GameStateMachine _stateMachine;

        private void Awake()
        {
            InitStateMachine();
            StartGame();
        }

        private void StartGame()
        {
            ballSpawner.Init(ballDatabase, skinDatabase, ballPool);
            cloud.Init(inputController, ballSpawner);
        }

        private void InitStateMachine()
        {
            var gameContext = new GameContext();

            _stateMachine = new GameStateMachine(GameState.Loaded);
            _gameStateController = new GameStateController(_stateMachine, gameContext);

            _stateMachine.OnTransitioned(transition =>
            {
                Debug.Log($"{transition.Source} + {transition.Trigger} => {transition.Destination}");
            });

            _stateMachine.Activate();
        }
    }
}
