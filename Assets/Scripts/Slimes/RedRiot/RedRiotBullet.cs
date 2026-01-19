using UnityEngine;

public class RedRiotBullet : MonoBehaviour
{
    public float lifetime = 10f;
    public int damage = 1;
    public LayerMask playerLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 velocity)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.velocity = velocity;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
                player.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
