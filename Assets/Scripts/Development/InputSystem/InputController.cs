using System;
using Core.Enums;
using UnityEngine;

namespace Development.InputSystem
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private InputPlatform inputPlatform = InputPlatform.Auto;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float dragStartThresholdPx = 12f;

        public event Action<float> PointerPressedWorldX;
        public event Action<float> PointerDraggedWorldX;
        public event Action<float> PointerTappedWorldX;
        public event Action PointerReleased;

        private bool _pointerDown;
        private bool _isDragging;
        private Vector2 _pointerDownScreenPos;

        private void Update()
        {
            switch (ResolvePlatform())
            {
                case InputPlatform.PC:
                    HandlePcInput();
                    break;
                case InputPlatform.Android:
                    HandleAndroidInput();
                    break;
            }
        }

        private InputPlatform ResolvePlatform()
        {
            if (inputPlatform != InputPlatform.Auto) return inputPlatform;

#if UNITY_ANDROID && !UNITY_EDITOR
            return InputPlatform.Android;
#else
            return InputPlatform.PC;
#endif
        }

        private void HandlePcInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandlePointerDown(Input.mousePosition);
                return;
            }

            if (Input.GetMouseButton(0))
            {
                HandlePointerHold(Input.mousePosition);
                return;
            }

            if (Input.GetMouseButtonUp(0))
                HandlePointerUp(Input.mousePosition);
        }

        private void HandleAndroidInput()
        {
            if (Input.touchCount <= 0) return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandlePointerDown(touch.position);
                    return;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HandlePointerHold(touch.position);
                    return;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandlePointerUp(touch.position);
                    break;
            }
        }

        private void HandlePointerDown(Vector2 screenPos)
        {
            _pointerDown = true;
            _isDragging = false;
            _pointerDownScreenPos = screenPos;

            PointerPressedWorldX?.Invoke(ScreenToWorldX(screenPos));
        }

        private void HandlePointerHold(Vector2 screenPos)
        {
            if (!_pointerDown) return;

            if (!_isDragging)
            {
                float distance = Vector2.Distance(screenPos, _pointerDownScreenPos);
                if (distance >= dragStartThresholdPx) _isDragging = true;
            }

            if (_isDragging)
                PointerDraggedWorldX?.Invoke(ScreenToWorldX(screenPos));
        }

        private void HandlePointerUp(Vector2 screenPos)
        {
            if (!_pointerDown) return;

            float worldX = ScreenToWorldX(screenPos);
            if (!_isDragging)
                PointerTappedWorldX?.Invoke(worldX);

            PointerReleased?.Invoke();

            _pointerDown = false;
            _isDragging = false;
        }

        private float ScreenToWorldX(Vector2 screenPos)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPos);

            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                return hit.x;
            }

            return ray.origin.x;
        }
    }
}
