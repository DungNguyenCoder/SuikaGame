using System;
using Cysharp.Threading.Tasks;
using JSAM;
using SuikaGame.Scripts.Development.LoadSave;
using SuikaGame.Scripts.Development.LoadSave.Data;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.CheckIn
{
    public class CheckInPanel : Panel
    {
        [SerializeField] private DailyCheckInBlock[] dayBlocks;
        [SerializeField] private Transform panel;
        [SerializeField] private float closeDelayAfterClaim = 0.5f;

        private void OnEnable()
        {
            panel.localScale = Vector3.zero;
        }

        private PlayerSaveData _playerData;
        private bool _isClaiming;

        public override void Open()
        {
            base.Open();
            AudioManager.PlaySound(AudioLibrarySounds._Popup);
            LoadAndRefreshAsync().Forget();
        }

        public void OnClickClose()
        {
            PlayClickSound();
            PanelManager.Instance.ClosePanel(PanelConfig.CHECKIN_PANEL);
        }

        private async UniTaskVoid LoadAndRefreshAsync()
        {
            _playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            SaveRuntimeData.SetPlayer(_playerData);

            if (!isActiveAndEnabled)
            {
                return;
            }

            RefreshBlocks();
        }

        private void HandleRewardBlockClicked(int dayIndex)
        {
            ClaimRewardAsync(dayIndex).Forget();
        }

        private async UniTaskVoid ClaimRewardAsync(int dayIndex)
        {
            if (_isClaiming)
            {
                return;
            }

            int claimableDay = DailyCheckInService.GetClaimableDay(_playerData);
            if (dayIndex != claimableDay || !DailyCheckInService.CanClaimToday(_playerData))
            {
                return;
            }

            PlayClickSound();

            DailyCheckInBlock block = FindBlock(dayIndex);

            _isClaiming = true;
            try
            {
                DailyCheckInService.ClaimToday(_playerData, block.RewardAmount);
                AudioManager.PlaySound(AudioLibrarySounds._CoinSound);
                SaveRuntimeData.SetPlayer(_playerData);
                await JsonRepository.SavePlayerProfile(_playerData);

                EventManager.OnProfileChanged?.Invoke();
                await block.PlayClaimAnimationAsync();
                RefreshBlocks();
                await UniTask.Delay(TimeSpan.FromSeconds(closeDelayAfterClaim), ignoreTimeScale: true);
                PanelManager.Instance.ClosePanel(PanelConfig.CHECKIN_PANEL);
            }
            finally
            {
                _isClaiming = false;
            }
        }

        private void RefreshBlocks()
        {
            bool canClaimToday = DailyCheckInService.CanClaimToday(_playerData);
            int claimableDay = DailyCheckInService.GetClaimableDay(_playerData);
            int lastClaimedDay = DailyCheckInService.GetLastClaimedDay(_playerData);

            foreach (DailyCheckInBlock block in dayBlocks)
            {
                block.Setup(HandleRewardBlockClicked);
                block.SetState(ResolveBlockState(block.DayIndex, canClaimToday, claimableDay, lastClaimedDay));
            }
        }

        private DailyCheckInBlock.RewardState ResolveBlockState(
            int dayIndex,
            bool canClaimToday,
            int claimableDay,
            int lastClaimedDay)
        {
            if (canClaimToday)
            {
                if (dayIndex == claimableDay)
                {
                    return DailyCheckInBlock.RewardState.Claimable;
                }

                return dayIndex < claimableDay
                    ? DailyCheckInBlock.RewardState.Claimed
                    : DailyCheckInBlock.RewardState.Locked;
            }

            return lastClaimedDay > 0 && dayIndex <= lastClaimedDay
                ? DailyCheckInBlock.RewardState.Claimed
                : DailyCheckInBlock.RewardState.Locked;
        }

        private DailyCheckInBlock FindBlock(int dayIndex)
        {
            foreach (DailyCheckInBlock block in dayBlocks)
            {
                if (block != null && block.DayIndex == dayIndex)
                {
                    return block;
                }
            }

            return null;
        }

        private static void PlayClickSound()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
        }
    }
}
