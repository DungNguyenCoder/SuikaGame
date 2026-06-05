using Cysharp.Threading.Tasks;
using JSAM;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SuikaGame.Scripts.Development.UI.Popup
{
    public class LosePanel : Panel
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private Image retryButtonImage;

        private Button _retryButton;
        private bool _isRetrying;

        private void Awake()
        {
            EnsureRetryButton();
            BindRetryButton();
        }

        private void OnDestroy()
        {
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(OnClickRetry);
            }
        }

        public override void Open()
        {
            base.Open();
            EnsureRetryButton();
            BindRetryButton();
            _isRetrying = false;
            Time.timeScale = 0f;
            AudioManager.PlaySound(AudioLibrarySounds._GameOver);
            RefreshDisplay();
        }

        public void OnClickRetry()
        {
            if (_isRetrying)
            {
                return;
            }

            AudioManager.PlaySound(AudioLibrarySounds._Click);
            RetryAsync().Forget();
        }

        private async UniTaskVoid RetryAsync()
        {
            _isRetrying = true;
            Time.timeScale = 1f;

            PlayerSaveData playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            int rewardCoin = CalculateRewardCoin(GetCurrentScore());
            playerData.Coin += rewardCoin;

            SaveRuntimeData.SetPlayer(playerData);
            SaveRuntimeData.SetProgress(new ProgressSaveData());

            await JsonRepository.SavePlayerProfile(playerData);
            JsonRepository.DeleteGameProgress();
            EventManager.OnProfileChanged?.Invoke();

            SceneManager.LoadScene(GameConfig.GAMEPLAY_SCENE);
        }

        private void RefreshDisplay()
        {
            int score = GetCurrentScore();
            int rewardCoin = CalculateRewardCoin(score);

            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }

            if (coinText != null)
            {
                coinText.text = rewardCoin.ToString();
            }
        }

        private int GetCurrentScore()
        {
            return SaveRuntimeData.Progress != null ? SaveRuntimeData.Progress.CurrentScore : 0;
        }

        private static int CalculateRewardCoin(int score)
        {
            return Mathf.RoundToInt(score / 10f);
        }

        private void EnsureRetryButton()
        {
            if (_retryButton != null)
            {
                return;
            }

            if (retryButtonImage == null)
            {
                return;
            }

            _retryButton = retryButtonImage.GetComponent<Button>();
            if (_retryButton == null)
            {
                _retryButton = retryButtonImage.gameObject.AddComponent<Button>();
            }

            if (_retryButton.targetGraphic == null)
            {
                _retryButton.targetGraphic = retryButtonImage;
            }
        }

        private void BindRetryButton()
        {
            if (_retryButton == null)
            {
                return;
            }

            _retryButton.onClick.RemoveListener(OnClickRetry);
            _retryButton.onClick.AddListener(OnClickRetry);
        }
    }
}
