using JSAM;
using SuikaGame.Scripts.Development.Animations;
using SuikaGame.Scripts.Development.LoadSave;
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
            EventManager.OnProfileChanged += RefreshCoinDisplay;
            RefreshCoinDisplay();
        }

        private void OnDisable()
        {
            EventManager.OnScoreChanged -= HandleScoreChanged;
            EventManager.OnProfileChanged -= RefreshCoinDisplay;
        }
        
        public void OnClickPause()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            EventManager.OnRequestPause?.Invoke();
        }

        private void HandleScoreChanged(int currentScore, int bestScore)
        {
            scoreAnimation.UpdateScore(currentScore);
        }

        private void RefreshCoinDisplay()
        {
            if (coin == null)
            {
                return;
            }

            coin.text = SaveRuntimeData.Player != null
                ? SaveRuntimeData.Player.Coin.ToString()
                : "0";
        }
    }
}
