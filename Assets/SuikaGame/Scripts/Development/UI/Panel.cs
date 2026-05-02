using SuikaGame.Scripts.Development.Managers;
using UnityEngine;

namespace SuikaGame.Scripts.Development.UI
{
    public class Panel : MonoBehaviour
    {
        public bool destroyOnClose = false;
        public string PanelName => name;
        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            if (destroyOnClose)
            {
                PanelManager.Instance.Unregister(PanelName);
            }
        }
    }
}
