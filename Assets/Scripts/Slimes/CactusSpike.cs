using UnityEngine;

public class CactusSpike : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;
    public int damage = 1;

    private Rigidbody2D rb;
    private LayerMask playerLayer;
    private LayerMask ifaLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Init(Vector2 dir, LayerMask player, LayerMask ifa)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        playerLayer = player;
        ifaLayer = ifa;

        rb.velocity = dir.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController2D p = other.GetComponent<PlayerController2D>();
            if (p != null)
                p.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & ifaLayer) != 0)
        {
            IfaEscortController ifa = other.GetComponent<IfaEscortController>();
            if (ifa != null)
                ifa.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }
    }
}
