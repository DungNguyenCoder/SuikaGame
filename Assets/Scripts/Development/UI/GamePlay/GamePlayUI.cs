using Development.Managers;
using Development.Utils;
using TMPro;
using UnityEngine;

namespace Development.UI.GamePlay
{
    public class GamePlayUI : Panel
    {
        [SerializeField] private TMP_Text score;
        [SerializeField] private TMP_Text coin;
        [SerializeField] private Booster booster;

        public void OnClickAddCoin()
        {

        }
        
        public void OnClickPause()
        {
            EventManager.OnRequestPause?.Invoke();
        }
    }
}
