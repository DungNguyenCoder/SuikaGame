using Cysharp.Threading.Tasks;
using Development.InputSystem;
using Development.LoadSave.Data;
using Development.Pools;
using UnityEngine;

namespace Development.Controllers
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField] private Transform ballPos;
        private InputController _inputController;
        private BallSpawner _ballSpawner;
        private Ball _ball;
        private readonly float _minX = -1.1f;
        private readonly float _maxX = 2f;
        private readonly bool _clampX = true;
        private bool _isHandlingBall = false;
        private float _pointerDownPlayerXOffset;

        public void Init(InputController inputController, BallSpawner ballSpawner)
        {
            _inputController = inputController;
            _ballSpawner = ballSpawner;
        }

        private void Start()
        {
            _ball = _ballSpawner.SpawnAndAttach(ballPos);
        }

        public CloudSaveData CaptureSaveData()
        {
            return new CloudSaveData
            {
                HasBall = _ball.gameObject.activeInHierarchy && !_ball.IsReleased,
                BallId = _ball.ID,
                PositionX = transform.position.x
            };
        }

        public void RestoreFromSaveData(CloudSaveData saveData)
        {
            SetPlayerX(saveData.PositionX);
            _ballSpawner.ReturnToPool(_ball);
            if (saveData.HasBall)
            {
                _ball = _ballSpawner.SpawnAndAttachById(ballPos, saveData.BallId);
                return;
            }

            _ball = _ballSpawner.SpawnAndAttach(ballPos);
        }

        private void OnEnable()
        {
            _inputController.PointerPressedWorldX += HandlePointerPressed;
            _inputController.PointerDraggedWorldX += HandlePointerDragged;
            _inputController.PointerTappedWorldX += HandlePointerTapped;
            _inputController.PointerReleased += HandlePointerReleased;
        }

        private void OnDisable()
        {
            _inputController.PointerPressedWorldX -= HandlePointerPressed;
            _inputController.PointerDraggedWorldX -= HandlePointerDragged;
            _inputController.PointerTappedWorldX -= HandlePointerTapped;
            _inputController.PointerReleased -= HandlePointerReleased;
        }

        private void HandlePointerPressed(float worldX)
        {
            _pointerDownPlayerXOffset = transform.position.x - worldX;
        }

        private void HandlePointerDragged(float worldX)
        {
            SetPlayerX(worldX + _pointerDownPlayerXOffset);
        }

        private void HandlePointerTapped(float worldX)
        {
            SetPlayerX(worldX);
        }

        private void HandlePointerReleased()
        {
            HandleBall();
        }

        private void HandleBall()
        {
            HandleBallAsync().Forget();
        }

        private async UniTaskVoid HandleBallAsync()
        {
            if (_isHandlingBall) return;

            _isHandlingBall = true;
            try
            {
                _ball = await _ballSpawner.ReleaseAndRespawn(_ball, ballPos);
            }
            finally
            {
                _isHandlingBall = false;
            }
        }

        private void SetPlayerX(float x)
        {
            if (_clampX) x = Mathf.Clamp(x, _minX, _maxX);

            Vector3 pos = transform.position;
            pos.x = x;

            transform.position = pos;
        }
    }
}
