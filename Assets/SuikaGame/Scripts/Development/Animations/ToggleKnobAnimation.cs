using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Development.Animations
{
    public class ToggleKnobAnimation : MonoBehaviour
    {
        [SerializeField] private Image knobImage;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float fallbackDistance = 57f;

        private RectTransform _knobRect;
        private float _travelDistance;
        private MotionHandle _motionHandle;
        private float _initialAnchoredDistance;
        private bool _hasInitialAnchoredDistance;

        private void Awake()
        {
            CacheReferences();
        }

        public void SetState(bool isEnabled, bool instant)
        {
            if (!CacheReferences())
            {
                return;
            }

            _travelDistance = ResolveTravelDistance();
            float targetX = isEnabled ? _travelDistance : -_travelDistance;
            if (instant)
            {
                Cancel();
                SetKnobPositionX(targetX);
                return;
            }

            Play(targetX);
        }

        public void Cancel()
        {
            if (_motionHandle.IsActive())
            {
                _motionHandle.Cancel();
            }
        }

        private void Play(float targetX)
        {
            Cancel();
            float currentX = _knobRect.anchoredPosition.x;
            _motionHandle = LMotion.Create(currentX, targetX, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutCubic)
                .Bind(OnKnobXChanged)
                .AddTo(this);
        }

        private void OnKnobXChanged(float value)
        {
            SetKnobPositionX(value);
        }

        private void SetKnobPositionX(float value)
        {
            Vector2 position = _knobRect.anchoredPosition;
            position.x = value;
            _knobRect.anchoredPosition = position;
        }

        private float ResolveTravelDistance()
        {
            RectTransform trackRect = _knobRect.parent as RectTransform;
            if (trackRect != null)
            {
                float computedDistance = (trackRect.rect.width - _knobRect.rect.width) * 0.5f;
                if (computedDistance > 0f)
                {
                    return computedDistance;
                }
            }

            if (_hasInitialAnchoredDistance && _initialAnchoredDistance > 0f)
            {
                return _initialAnchoredDistance;
            }

            return fallbackDistance;
        }

        [Button("Test ON")]
        private void TestOn()
        {
            SetState(true, !Application.isPlaying);
        }

        [Button("Test OFF")]
        private void TestOff()
        {
            SetState(false, !Application.isPlaying);
        }

        [Button("Test Toggle")]
        private void TestToggle()
        {
            if (!CacheReferences())
            {
                return;
            }

            bool isCurrentlyOn = _knobRect.anchoredPosition.x >= 0f;
            SetState(!isCurrentlyOn, !Application.isPlaying);
        }

        private bool CacheReferences()
        {
            if (knobImage == null)
            {
                return false;
            }

            _knobRect = knobImage.rectTransform;
            if (!_hasInitialAnchoredDistance)
            {
                _initialAnchoredDistance = Mathf.Abs(_knobRect.anchoredPosition.x);
                _hasInitialAnchoredDistance = true;
            }

            return true;
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}
