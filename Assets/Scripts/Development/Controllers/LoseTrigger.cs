using UnityEngine;
using Development.Managers;

namespace Development.Controllers
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
            EventManager.OnLoseLevel?.Invoke();
            Debug.Log("Lost");
        }
    }
}
