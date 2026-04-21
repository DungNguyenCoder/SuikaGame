using System;
using System.Collections.Generic;
using Core;
using Core.Ball;
using Core.Skin;
using Cysharp.Threading.Tasks;
using Development.Controllers;
using Development.InputSystem;
using Development.LoadSave;
using Development.LoadSave.Data;
using Development.Managers;
using Development.Pools;
using Development.StateMachine;
using Development.Utils;
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

        private readonly List<Ball> _releasedBalls = new();
        private GameStateController _gameStateController;
        private GameStateMachine _stateMachine;
        private GameContext _gameContext;
        private PlayerSaveData _playerSaveData;
        private ProgressSaveData _progressSaveData;

        private void Awake()
        {
            InitStateMachine();
            InitGame();
            StartAsync().Forget();
        }

        private void OnEnable()
        {
            EventManager.OnLoseLevel += HandleLoseLevel;
        }

        private void OnDisable()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
        }

        private void OnDestroy()
        {
            _gameStateController.Destroy();
        }

        private void InitGame()
        {
            ballSpawner.Init(ballDatabase, skinDatabase, ballPool);
            cloud.Init(inputController, ballSpawner);
        }

        private async UniTaskVoid StartAsync()
        {
            _playerSaveData = await JsonRepository.LoadPlayerProfile();
            SaveRuntimeData.SetPlayer(_playerSaveData);

            bool startNewGame = GameLaunchOptions.ConsumeStartNewGameRequest();
            if (startNewGame)
            {
                JsonRepository.DeleteGameProgress();
            }

            bool hasGameProgress = !startNewGame && JsonRepository.HasGameProgress();
            _progressSaveData = hasGameProgress
                ? await JsonRepository.LoadGameProgress()
                : new ProgressSaveData();
            SaveRuntimeData.SetProgress(_progressSaveData);

            _gameContext.PlayerSaveData = _playerSaveData;
            _gameContext.ProgressSaveData = _progressSaveData;
            EventManager.OnScoreChanged?.Invoke(_progressSaveData.CurrentScore, _playerSaveData.HighScore);

            await UniTask.NextFrame();
            if (hasGameProgress)
            {
                ApplySavedProgress();
            }
        }

        private void ApplySavedProgress()
        {
            ballSpawner.RestoreReleasedBalls(_progressSaveData.BoardBalls);
            cloud.RestoreFromSaveData(_progressSaveData.Cloud);
        }

        private void CaptureProgressData()
        {
            _progressSaveData.BoardBalls.Clear();
            ballSpawner.FillReleasedBalls(_releasedBalls);
            foreach (Ball ball in _releasedBalls)
            {
                _progressSaveData.BoardBalls.Add(new BallSaveData(ball.ID, ball.transform.position, ball.Velocity, ball.AngularVelocity));
            }

            _progressSaveData.Cloud = cloud.CaptureSaveData();
            SaveRuntimeData.SetProgress(_progressSaveData);
        }

        private async UniTask SaveRuntimeState()
        {
            CaptureProgressData();
            await JsonRepository.SavePlayerProfile(_playerSaveData);
            await JsonRepository.SaveGameProgress(_progressSaveData);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                return;
            }

            SaveRuntimeState().Forget();
        }

        private void OnApplicationQuit()
        {
            SaveRuntimeState().Forget();
        }

        private void HandleLoseLevel()
        {
            _ = JsonRepository.SavePlayerProfile(_playerSaveData);
            JsonRepository.DeleteGameProgress();
            _progressSaveData = new ProgressSaveData();
            SaveRuntimeData.SetProgress(_progressSaveData);
            _gameContext.ProgressSaveData = _progressSaveData;
            EventManager.OnScoreChanged?.Invoke(_progressSaveData.CurrentScore, _playerSaveData.HighScore);
        }

        private void InitStateMachine()
        {
            _gameContext = new GameContext
            {
                PlayerSaveData = _playerSaveData,
                ProgressSaveData = _progressSaveData
            };

            _stateMachine = new GameStateMachine(GameState.Loaded);
            _gameStateController = new GameStateController(_stateMachine, _gameContext);

            _stateMachine.OnTransitioned(transition =>
            {
                Debug.Log($"{transition.Source} + {transition.Trigger} => {transition.Destination}");
            });

            _stateMachine.Activate();
        }
    }
}
