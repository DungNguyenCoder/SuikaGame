using SuikaGame.Scripts.Development.Managers;
using SuikaGame.Scripts.Development.Utils;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public class LoseTrigger : MonoBehaviour
    {
        [SerializeField] private string ballTag = "Ball";
        [SerializeField] private BoxCollider2D triggerArea;
        private bool _hasLost;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasLost) return;
            if (!other.CompareTag(ballTag)) return;

            var ball = other.GetComponent<Ball>();
            if (!ball.IsReleased) return;

            ball.RegisterLoseTriggerTouch();
            if (ball.LoseTriggerTouchCount < 2) return;

            _hasLost = true;
            VibrationService.Vibrate();
            EventManager.OnLoseLevel?.Invoke();
            Debug.Log("Lost");
        }
    }
}
