using SuikaGame.Scripts.Development.Animations;
using SuikaGame.Scripts.Development.Managers;
using TMPro;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.GamePlay
{
    public class GamePlayUI : Panel
    {
        [SerializeField] private ScoreTextAnimation scoreAnimation;
        [SerializeField] private TMP_Text coin;
        [SerializeField] private Booster booster;

        private void OnEnable()
        {
            EventManager.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            EventManager.OnScoreChanged -= HandleScoreChanged;
        }
        
        public void OnClickAddCoin()
        {

        }
        
        public void OnClickPause()
        {
            EventManager.OnRequestPause?.Invoke();
        }

        private void HandleScoreChanged(int currentScore, int bestScore)
        {
            scoreAnimation.UpdateScore(currentScore);
        }
    }
}
