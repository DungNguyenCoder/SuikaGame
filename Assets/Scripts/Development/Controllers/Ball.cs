using Core;
using Development.Managers;
using UnityEngine;

namespace Controllers
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D col;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer sr;
        private BallData _data;
        private bool _isMergeLocked;

        public int ID => _data != null ? _data.ID : -1;
        public bool IsReleased => rb.simulated;

        public void Setup(BallData data, SkinDatabase skinDatabase, int seriesID)
        {
            if (data == null) return;

            _isMergeLocked = false;
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

        private bool TryLockMergeWith(Ball other)
        {
            if (other == null || other == this) return false;
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
            if (_data == null || !IsReleased) return;

            var otherBall = collision.collider.GetComponent<Ball>();
            if (otherBall == null || otherBall._data == null || !otherBall.IsReleased) return;

            if (ID != otherBall.ID) return;
            if (!TryLockMergeWith(otherBall)) return;

            EventManager.SameIdCollision?.Invoke(this, otherBall);
        }
    }
}
