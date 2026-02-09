using UnityEngine;

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
    public void Setup(BallData data, BallDatabase database)
    {
        _data = data;

        if (database != null)
        {
            var sprite = database.GetSkinSprite(data.SeriesID, data.SkinID);
            if (_sr != null && sprite != null) _sr.sprite = sprite;
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
