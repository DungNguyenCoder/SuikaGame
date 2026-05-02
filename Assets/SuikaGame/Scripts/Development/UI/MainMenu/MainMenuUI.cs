using Cysharp.Threading.Tasks;
using Development.LoadSave;
using Development.LoadSave.Data;
using Development.Managers;
using Development.Utils;
using SuikaGame.Scripts.Development.UI.CheckIn;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace Development.UI.MainMenu
{
    public class MainMenuUI : Panel
    {
        [SerializeField] private TopBlock topBlock;
        [SerializeField] private ContentBlock contentBlock;
        [SerializeField] private BottomBlock bottomBlock;

        private bool _checkInPromptRequested;

        private void Start()
        {
            PromptDailyCheckInAsync().Forget();
        }

        private async UniTaskVoid PromptDailyCheckInAsync()
        {
            await UniTask.NextFrame();

            if (!isActiveAndEnabled || _checkInPromptRequested)
            {
                return;
            }

            PlayerSaveData playerData = SaveRuntimeData.Player ?? await JsonRepository.LoadPlayerProfile();
            SaveRuntimeData.SetPlayer(playerData);

            if (!DailyCheckInService.CanClaimToday(playerData))
            {
                return;
            }

            _checkInPromptRequested = true;
            PanelManager.Instance.OpenPanel(PanelConfig.CHECKIN_PANEL);
        }
    }
}
