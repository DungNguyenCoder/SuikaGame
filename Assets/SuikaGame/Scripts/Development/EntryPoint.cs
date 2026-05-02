using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Core.Ball;
using SuikaGame.Scripts.Core.Enums;
using SuikaGame.Scripts.Core.Skin;
using SuikaGame.Scripts.Development.Controllers;
using SuikaGame.Scripts.Development.InputSystem;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Pools;
using SuikaGame.Scripts.Development.StateMachine;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private BallDatabase ballDatabase;
        [SerializeField] private SkinDatabase skinDatabase;
        [SerializeField] private BallSpawner ballSpawner;
        [SerializeField] private BallPool ballPool;
        [SerializeField] private InputController inputController;
        [SerializeField] private Cloud cloud;
        [SerializeField] private GameplayBackground gameplayBackground;

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
            EventManager.OnRequestUseBooster += HandleUseBoosterAsync;
        }

        private void OnDisable()
        {
            EventManager.OnLoseLevel -= HandleLoseLevel;
            EventManager.OnRequestUseBooster -= HandleUseBoosterAsync;
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
            EventManager.OnProfileChanged?.Invoke();
            ApplySelectedSkins();

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

        private void ApplySelectedSkins()
        {
            ballSpawner.SetActiveSkinSeriesID(_playerSaveData.SelectedSkinSeriesId);
            gameplayBackground.ApplyBackground(_playerSaveData.SelectedBackgroundId);
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

        private UniTask<bool> HandleUseBoosterAsync(BoosterType boosterType)
        {
            return boosterType switch
            {
                BoosterType.Destruction => ballSpawner.RemoveRandomReleasedBallsAsync(3),
                BoosterType.Promotion => ballSpawner.PromoteRandomReleasedBallsAsync(2),
                BoosterType.Biggest => ballSpawner.RemoveBiggestReleasedBallAsync(),
                BoosterType.Shuffle => ballSpawner.ShuffleReleasedBallsAsync(),
                _ => UniTask.FromResult(false)
            };
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
