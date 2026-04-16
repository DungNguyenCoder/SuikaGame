using Core;
using UnityEngine;

namespace Controllers
{
    public class Ball : MonoBehaviour
    {
        private CircleCollider2D _col;
        private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer _sr;
        private BallData _data;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CircleCollider2D>();
        }

        public void Setup(BallData data, SkinDatabase skinDatabase, int seriesID)
        {
            if (data == null) return;

            _data = data;

            if (_sr != null)
            {
                var sprite = skinDatabase != null ? skinDatabase.GetSkinSprite(seriesID, data.ID) : null;
                _sr.sprite = sprite;
            }

            if (_col != null)
            {
                _col.radius = data.ColliderRadius;
            }

            if (_rb != null)
            {
                _rb.simulated = false;
            }
        }
        public void Release()
        {
            transform.SetParent(null);
            _rb.simulated = true;
        }
    }
}
