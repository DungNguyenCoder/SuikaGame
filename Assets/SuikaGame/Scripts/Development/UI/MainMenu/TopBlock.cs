using Development.Managers;
using Development.LoadSave;
using Development.UI.Popup;
using Development.Utils;
using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Development.UI.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Development.UI.MainMenu
{
    public class TopBlock : MonoBehaviour
    {
        [SerializeField] private TMP_Text coin;
        [SerializeField] private TMP_Text account;
        [SerializeField] private Image accountImage;
        private const string DefaultUserName = "Guest";
        private Sprite _defaultAccountSprite;

        private void Awake()
        {
            _defaultAccountSprite = accountImage.sprite;
        }

        private void OnEnable()
        {
            EventManager.OnProfileChanged += RefreshProfileDisplay;
            EventManager.OnProfileAvatarChanged += HandleProfileAvatarChanged;
            EnsureProfileLoadedAndRefresh().Forget();
        }

        private void OnDisable()
        {
            EventManager.OnProfileChanged -= RefreshProfileDisplay;
            EventManager.OnProfileAvatarChanged -= HandleProfileAvatarChanged;
        }

        public void OnClickAddCoin()
        {
            
        }

        public void OnClickModifyAccount()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.ACCOUNT_PANEL);
        }
        
        public void OnClickSetting()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.SETTING_PANEL);
        }

        private async UniTaskVoid EnsureProfileLoadedAndRefresh()
        {
            if (SaveRuntimeData.Player == null)
            {
                var playerData = await JsonRepository.LoadPlayerProfile();
                SaveRuntimeData.SetPlayer(playerData);
            }

            if (!isActiveAndEnabled)
            {
                return;
            }

            RefreshProfileDisplay();
            await RefreshAvatarDisplayAsync();
        }

        private void RefreshProfileDisplay()
        {
            string userName = SaveRuntimeData.Player != null ? SaveRuntimeData.Player.UserName : string.Empty;
            account.text = string.IsNullOrWhiteSpace(userName) ? DefaultUserName : userName.Trim();
        }

        private void HandleProfileAvatarChanged(Sprite avatarSprite)
        {
            accountImage.sprite = avatarSprite != null ? avatarSprite : _defaultAccountSprite;
        }

        private async UniTask RefreshAvatarDisplayAsync()
        {
            string avatarPath = SaveRuntimeData.Player != null ? SaveRuntimeData.Player.AvatarId : string.Empty;
            if (string.IsNullOrEmpty(avatarPath))
            {
                HandleProfileAvatarChanged(null);
                return;
            }

            var panel = await PanelManager.Instance.GetPanelAsync(PanelConfig.ACCOUNT_PANEL);
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (panel is AccountPanel accountPanel && accountPanel.TryGetAvatarSprite(avatarPath, out Sprite avatarSprite))
            {
                HandleProfileAvatarChanged(avatarSprite);
                return;
            }

            HandleProfileAvatarChanged(null);
        }
    }
}
