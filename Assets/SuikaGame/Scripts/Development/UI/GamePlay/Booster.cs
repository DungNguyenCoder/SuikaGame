using Cysharp.Threading.Tasks;
using JSAM;
using SuikaGame.Scripts.Core.Enums;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using TMPro;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.GamePlay
{
    public class Booster : MonoBehaviour
    {
        [SerializeField] private TMP_Text destructionCountText;
        [SerializeField] private TMP_Text promotionCountText;
        [SerializeField] private TMP_Text biggestCountText;
        [SerializeField] private TMP_Text shuffleCountText;

        private bool _isUsingBooster;

        private void OnEnable()
        {
            EventManager.OnProfileChanged += RefreshCounts;
            RefreshCounts();
        }

        private void OnDisable()
        {
            EventManager.OnProfileChanged -= RefreshCounts;
        }

        public void OnClickDestruction()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            UseBoosterAsync(BoosterType.Destruction).Forget();
        }

        public void OnClickPromotion()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            UseBoosterAsync(BoosterType.Promotion).Forget();
        }

        public void OnClickBiggest()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            UseBoosterAsync(BoosterType.Biggest).Forget();
        }

        public void OnClickShuffle()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
            UseBoosterAsync(BoosterType.Shuffle).Forget();
        }

        private async UniTaskVoid UseBoosterAsync(BoosterType boosterType)
        {
            if (_isUsingBooster)
            {
                return;
            }

            PlayerSaveData playerData = SaveRuntimeData.Player;
            if (playerData == null)
            {
                return;
            }

            if (EventManager.OnRequestUseBooster == null)
            {
                return;
            }

            if (!BoosterInventoryService.TryConsumeUse(playerData, boosterType))
            {
                RefreshCounts();
                return;
            }

            _isUsingBooster = true;
            bool boosterApplied = false;
            try
            {
                boosterApplied = await EventManager.OnRequestUseBooster.Invoke(boosterType);
            }
            finally
            {
                if (!boosterApplied)
                {
                    BoosterInventoryService.AddUses(playerData, boosterType, 1);
                }

                SaveRuntimeData.SetPlayer(playerData);
                EventManager.OnProfileChanged?.Invoke();
                try
                {
                    await JsonRepository.SavePlayerProfile(playerData);
                }
                finally
                {
                    _isUsingBooster = false;
                }
            }
        }

        private void RefreshCounts()
        {
            PlayerSaveData playerData = SaveRuntimeData.Player;
            SetCountText(destructionCountText, playerData, BoosterType.Destruction);
            SetCountText(promotionCountText, playerData, BoosterType.Promotion);
            SetCountText(biggestCountText, playerData, BoosterType.Biggest);
            SetCountText(shuffleCountText, playerData, BoosterType.Shuffle);
        }

        private static void SetCountText(TMP_Text text, PlayerSaveData playerData, BoosterType boosterType)
        {
            if (text == null)
            {
                return;
            }

            int remainingUses = playerData == null
                ? 0
                : BoosterInventoryService.GetRemainingUses(playerData, boosterType);
            text.text = remainingUses.ToString();
        }
    }
}
