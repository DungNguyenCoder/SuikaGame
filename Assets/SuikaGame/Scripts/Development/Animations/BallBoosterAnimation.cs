using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Animations
{
    public static class BallBoosterAnimation
    {
        public static async UniTask PlayDisappearAsync(Transform target, float duration, MonoBehaviour owner)
        {
            if (duration <= 0f)
            {
                target.localScale = Vector3.zero;
                return;
            }

            MotionHandle motionHandle = LMotion.Create(target.localScale, Vector3.zero, duration)
                .WithEase(Ease.InBack)
                .BindToLocalScale(target)
                .AddTo(owner);
            await motionHandle;
        }

        public static async UniTask PlayPromotionAsync(
            Transform target,
            Vector3 defaultScale,
            float peakScale,
            float duration,
            Action onPeak,
            MonoBehaviour owner)
        {
            Vector3 peak = defaultScale * peakScale;
            float halfDuration = Mathf.Max(0f, duration * 0.5f);

            if (halfDuration > 0f)
            {
                MotionHandle growHandle = LMotion.Create(defaultScale, peak, halfDuration)
                    .WithEase(Ease.OutBack)
                    .BindToLocalScale(target)
                    .AddTo(owner);
                await growHandle;
            }
            else
            {
                target.localScale = peak;
            }

            onPeak?.Invoke();

            if (halfDuration > 0f)
            {
                MotionHandle shrinkHandle = LMotion.Create(peak, defaultScale, halfDuration)
                    .WithEase(Ease.OutCubic)
                    .BindToLocalScale(target)
                    .AddTo(owner);
                await shrinkHandle;
            }
            else
            {
                target.localScale = defaultScale;
            }
        }
    }
}
