using System;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Development.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuikaGame.Scripts.Development.UI.CheckIn
{
    public class DailyCheckInBlock : MonoBehaviour, IPointerClickHandler
    {
        public enum RewardState
        {
            Locked,
            Claimable,
            Claimed
        }

        [SerializeField] private int dayIndex = 1;
        [SerializeField] private int rewardAmount = 500;
        [SerializeField] private TMP_Text dayLabel;
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color claimableColor = Color.white;
        [SerializeField] private Color claimedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] private ScalePulseAnimation claimAnimation;

        private Action<int> _onClicked;
        private bool _canClaim;

        public int DayIndex => dayIndex;
        public int RewardAmount => rewardAmount;

        public void Setup(Action<int> onClicked)
        {
            _onClicked = onClicked;
            
            dayLabel.text = $"{dayIndex}-Day";
            rewardLabel.text = rewardAmount.ToString();
        }

        public void SetState(RewardState state)
        {
            _canClaim = state == RewardState.Claimable;

            backgroundImage.color = state switch
            {
                RewardState.Claimable => claimableColor,
                RewardState.Claimed => claimedColor,
                _ => lockedColor
            };
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_canClaim)
            {
                return;
            }

            _onClicked?.Invoke(dayIndex);
        }

        public async UniTask PlayClaimAnimationAsync()
        {
            await claimAnimation.PlayAsync();
        }
    }
}
