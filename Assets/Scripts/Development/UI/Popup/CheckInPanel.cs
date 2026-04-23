using Development.Managers;
using Development.Utils;

namespace Development.UI.Popup
{
    public class CheckInPanel : Panel
    {
        public void OnClickClose()
        {
            PanelManager.Instance.ClosePanel(PanelConfig.CHECKIN_PANEL);
        }
    }
}