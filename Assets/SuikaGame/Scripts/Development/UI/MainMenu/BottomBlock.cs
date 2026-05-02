using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI.MainMenu
{
    public class BottomBlock : MonoBehaviour
    {
        public void OnClickHome()
        {
            
        }

        public void OnClickShop()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.SHOP_PANEL);
        }

        public void OnClickSkin()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.SHOP_PANEL);
        }

        public void OnClickCheckIn()
        {
            PanelManager.Instance.OpenPanel(PanelConfig.CHECKIN_PANEL);
        }
        
        public void OnClickLeaderboard()
        {
            
        }
    }
}
