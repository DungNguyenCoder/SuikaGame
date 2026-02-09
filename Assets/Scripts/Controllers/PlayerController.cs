using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BallSpawner ballSpawner;
    [SerializeField] private Transform ballPos;
    private Ball ball;
    private float _minX = -1.1f;
    private float _maxX = 2f;
    private float _dragStartThresholdPx = 12f;
    private bool _clampX = true;
    private bool _pointerDown = false;
    private bool _isDragging = false;
    private Camera _cam;
    private Vector2 _pointerDownScreenPos;
    private float _pointerDownPlayerXOffset;
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        if (ballSpawner != null)
            ball = ballSpawner.SpawnAndAttach(ballPos);
    }

    private void Update()
    {
        if (Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleTouch()
    {
        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            _pointerDown = true;
            _isDragging = false;
            _pointerDownScreenPos = t.position;

            float worldX = ScreenToWorldX(t.position);
            _pointerDownPlayerXOffset = transform.position.x - worldX;
        }
        else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
        {
            if (!_pointerDown) return;

            if (!_isDragging)
            {
                float dist = Vector2.Distance(t.position, _pointerDownScreenPos);
                if (dist >= _dragStartThresholdPx) _isDragging = true;
            }

            if (_isDragging)
            {
                float worldX = ScreenToWorldX(t.position);
                SetPlayerX(worldX + _pointerDownPlayerXOffset);
            }
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            if (!_pointerDown) return;

            if (!_isDragging)
            {
                float worldX = ScreenToWorldX(t.position);
                SetPlayerX(worldX);
            }

            HandleBall();

            _pointerDown = false;
            _isDragging = false;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _pointerDown = true;
            _isDragging = false;
            _pointerDownScreenPos = Input.mousePosition;

            float worldX = ScreenToWorldX(Input.mousePosition);
            _pointerDownPlayerXOffset = transform.position.x - worldX;
        }
        else if (Input.GetMouseButton(0))
        {
            if (!_pointerDown) return;

            Vector2 cur = Input.mousePosition;

            if (!_isDragging)
            {
                float dist = Vector2.Distance(cur, _pointerDownScreenPos);
                if (dist >= _dragStartThresholdPx) _isDragging = true;
            }

            if (_isDragging)
            {
                float worldX = ScreenToWorldX(cur);
                SetPlayerX(worldX + _pointerDownPlayerXOffset);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!_pointerDown) return;

            if (!_isDragging)
            {
                float worldX = ScreenToWorldX(Input.mousePosition);
                SetPlayerX(worldX);
            }
            HandleBall();

            _pointerDown = false;
            _isDragging = false;
        }
    }

    private void HandleBall()
    {
        HandleBallAsync().Forget();
    }

    private async UniTaskVoid HandleBallAsync()
    {
        if (ball != null && ballSpawner != null)
        {
            Ball newBall = await ballSpawner.ReleaseAndRespawn(ball, ballPos);
            ball = newBall;
        }
    }

    private void SetPlayerX(float x)
    {
        if (_clampX) x = Mathf.Clamp(x, _minX, _maxX);

        Vector3 pos = transform.position;
        pos.x = x;

        transform.position = pos;
    }

    private float ScreenToWorldX(Vector2 screenPos)
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return transform.position.x;

        Ray ray = _cam.ScreenPointToRay(screenPos);

        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            return hit.x;
        }

        return transform.position.x;
    }
}
