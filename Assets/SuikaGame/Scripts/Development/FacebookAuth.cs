using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Development
{
    public class FacebookAuth : MonoBehaviour
    {
        [SerializeField] private TMP_Text username;
        [SerializeField] private TMP_Text email;
        [SerializeField] private Image avatar;
        [SerializeField] private GameObject login, account;

        public void OnLogIn()
        {
            Debug.Log("Login Success");
            login.SetActive(false);
            account.SetActive(true);
        }
        
        public void OnLogout()
        {
            Debug.Log("Logout Success");
            login.SetActive(true);
            account.SetActive(false);
        }
    }
}