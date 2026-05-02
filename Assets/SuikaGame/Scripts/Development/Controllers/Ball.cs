using Cysharp.Threading.Tasks;
using SuikaGame.Scripts.Core.Ball;
using SuikaGame.Scripts.Core.Skin;
using SuikaGame.Scripts.Development.Animations;
using SuikaGame.Scripts.Development.Managers;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Controllers
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D col;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer sr;
        private BallData _data;
        private bool _isMergeLocked;
        private bool _isBoosterLocked;
        private int _loseTriggerTouchCount;
        private Vector3 _defaultScale;

        public int ID => _data.ID;
        public bool IsReleased => rb.simulated;
        public int LoseTriggerTouchCount => _loseTriggerTouchCount;
        public Vector2 Velocity => rb.velocity;
        public float AngularVelocity => rb.angularVelocity;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        public void Setup(BallData data, SkinDatabase skinDatabase, int seriesID)
        {
            _isMergeLocked = false;
            _isBoosterLocked = false;
            _loseTriggerTouchCount = 0;
            transform.localScale = _defaultScale;

            ApplyData(data, skinDatabase, seriesID);
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

        public void SetBoosterLocked(bool locked)
        {
            _isBoosterLocked = locked;
        }

        public async UniTask PlayDisappearAsync(float duration)
        {
            _isBoosterLocked = true;
            rb.simulated = false;

            await BallBoosterAnimation.PlayDisappearAsync(transform, duration, this);
        }

        public async UniTask PlayPromotionAsync(BallData promotedData, SkinDatabase skinDatabase, int seriesID, float peakScale, float duration)
        {
            _isBoosterLocked = true;

            bool wasReleased = rb.simulated;
            Vector2 velocity = rb.velocity;
            float angularVelocity = rb.angularVelocity;
            rb.simulated = false;

            await BallBoosterAnimation.PlayPromotionAsync(
                transform,
                _defaultScale,
                peakScale,
                duration,
                () => ApplyData(promotedData, skinDatabase, seriesID),
                this);

            rb.simulated = wasReleased;
            if (wasReleased)
            {
                SetMotion(velocity, angularVelocity);
            }

            _isBoosterLocked = false;
        }

        public void RegisterLoseTriggerTouch()
        {
            _loseTriggerTouchCount++;
        }

        private bool TryLockMergeWith(Ball other)
        {
            if (_isBoosterLocked || other._isBoosterLocked) return false;
            if (_isMergeLocked || other._isMergeLocked) return false;

            _isMergeLocked = true;
            other._isMergeLocked = true;
            return true;
        }

        public void PrepareForPool()
        {
            _isMergeLocked = false;
            _isBoosterLocked = false;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
            transform.localScale = _defaultScale;
        }

        private void ApplyData(BallData data, SkinDatabase skinDatabase, int seriesID)
        {
            _data = data;
            sr.sprite = skinDatabase.GetSkinSprite(seriesID, data.ID);
            col.radius = data.ColliderRadius;
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
