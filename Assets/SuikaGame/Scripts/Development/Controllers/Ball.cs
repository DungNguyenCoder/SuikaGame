using Core;
using Core.Skin;
using Development.Managers;
using UnityEngine;

namespace Development.Controllers
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D col;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer sr;
        private BallData _data;
        private bool _isMergeLocked;
        private int _loseTriggerTouchCount;

        public int ID => _data.ID;
        public bool IsReleased => rb.simulated;
        public int LoseTriggerTouchCount => _loseTriggerTouchCount;
        public Vector2 Velocity => rb.velocity;
        public float AngularVelocity => rb.angularVelocity;

        public void Setup(BallData data, SkinDatabase skinDatabase, int seriesID)
        {
            _isMergeLocked = false;
            _loseTriggerTouchCount = 0;
            _data = data;

            sr.sprite = skinDatabase.GetSkinSprite(seriesID, data.ID);
            col.radius = data.ColliderRadius;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        public void Release(Transform releasedParent)
        {
            transform.SetParent(releasedParent, true);
            rb.simulated = true;
        }

        public void SetMotion(Vector2 velocity, float angularVelocity)
        {
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
        }

        public void RegisterLoseTriggerTouch()
        {
            _loseTriggerTouchCount++;
        }

        private bool TryLockMergeWith(Ball other)
        {
            if (_isMergeLocked || other._isMergeLocked) return false;

            _isMergeLocked = true;
            other._isMergeLocked = true;
            return true;
        }

        public void PrepareForPool()
        {
            _isMergeLocked = false;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsReleased) return;

            var otherBall = collision.collider.GetComponent<Ball>();
            if (otherBall == null || !otherBall.IsReleased) return;

            if (ID != otherBall.ID) return;
            if (!TryLockMergeWith(otherBall)) return;

            EventManager.SameIdCollision?.Invoke(this, otherBall);
        }
    }
}
