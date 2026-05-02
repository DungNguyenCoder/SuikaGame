using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Animations
{
    public class ScalePulseAnimation : MonoBehaviour
    {
        [SerializeField] private float peakScale = 1.1f;
        [SerializeField] private float duration = 0.2f;

        private Vector3 _defaultScale;
        private MotionHandle _motionHandle;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        private void OnDisable()
        {
            Cancel();
            transform.localScale = _defaultScale;
        }

        public async UniTask PlayAsync()
        {
            Cancel();

            Vector3 peak = _defaultScale * peakScale;
            float halfDuration = duration * 0.5f;

            _motionHandle = LMotion.Create(_defaultScale, peak, halfDuration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutCubic)
                .BindToLocalScale(transform)
                .AddTo(this);
            await _motionHandle;

            _motionHandle = LMotion.Create(peak, _defaultScale, halfDuration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithEase(Ease.OutCubic)
                .BindToLocalScale(transform)
                .AddTo(this);
            await _motionHandle;
        }

        private void Cancel()
        {
            if (_motionHandle.IsActive())
            {
                _motionHandle.Cancel();
            }
        }
    }
}
