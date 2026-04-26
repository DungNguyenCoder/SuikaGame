using LitMotion;
using TMPro;
using UnityEngine;

namespace Development.Animations
{
    public class ScoreTextAnimation : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private const float ScoreAnimMinDuration = 0.12f;
        private const float ScoreAnimMaxDuration = 0.35f;
        private const float ScoreAnimDeltaFactor = 2000f;

        private int _displayedScore;
        private bool _isScoreInitialized;
        private Vector3 _defaultScale;
        private MotionHandle _countHandle;
        private MotionHandle _punchHandle;

        private void Awake()
        {
            _defaultScale = scoreText.rectTransform.localScale;
            if (int.TryParse(scoreText.text, out int currentScore))
            {
                _displayedScore = currentScore;
            }
        }

        private void OnDisable()
        {
            ResetScale();
        }

        public void UpdateScore(int targetScore)
        {
            if (!_isScoreInitialized)
            {
                _isScoreInitialized = true;
                _displayedScore = targetScore;
                SetScoreText(targetScore);
                return;
            }

            PlayCountAnimation(targetScore);
            PlayPunchAnimation();
        }

        private void PlayCountAnimation(int targetScore)
        {
            if (_countHandle.IsActive())
            {
                _countHandle.Cancel();
            }

            float duration = GetDuration(targetScore);
            _countHandle = LMotion.Create(_displayedScore, targetScore, duration)
                .WithEase(Ease.OutQuad)
                .Bind(OnScoreValueChanged)
                .AddTo(this);
        }

        private void PlayPunchAnimation()
        {
            if (_punchHandle.IsActive())
            {
                _punchHandle.Cancel();
            }

            ResetScale();
            _punchHandle = LMotion.Punch.Create(1f, 0.14f, 0.22f)
                .Bind(OnScaleFactorChanged)
                .AddTo(this);
        }

        private float GetDuration(int targetScore)
        {
            int delta = Mathf.Abs(targetScore - _displayedScore);
            float duration = delta / ScoreAnimDeltaFactor;
            return Mathf.Clamp(duration, ScoreAnimMinDuration, ScoreAnimMaxDuration);
        }

        private void OnScoreValueChanged(int value)
        {
            _displayedScore = value;
            SetScoreText(value);
        }

        private void OnScaleFactorChanged(float scaleFactor)
        {
            scoreText.rectTransform.localScale = _defaultScale * scaleFactor;
        }

        private void SetScoreText(int value)
        {
            scoreText.text = value.ToString();
        }

        private void ResetScale()
        {
            scoreText.rectTransform.localScale = _defaultScale;
        }
    }
}
