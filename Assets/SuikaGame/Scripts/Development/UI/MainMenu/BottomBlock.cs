using JSAM;
using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.MainMenu
{
    public class BottomBlock : MonoBehaviour
    {
        public void OnClickSkin()
        {
            PlayClickSound();
            PanelManager.Instance.OpenPanel(PanelConfig.SHOP_PANEL);
        }

        public void OnClickCheckIn()
        {
            PlayClickSound();
            PanelManager.Instance.OpenPanel(PanelConfig.CHECKIN_PANEL);
        }
        
        public void OnClickLeaderboard()
        {
            PlayClickSound();
        }

        private static void PlayClickSound()
        {
            AudioManager.PlaySound(AudioLibrarySounds._Click);
        }
    }
}
