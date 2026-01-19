using UnityEngine;

public class RedRiotBomb : MonoBehaviour
{
    public float lifetime = 10f;
    public int damage = 1;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public AudioSource audioSource;
    public AudioClip impactClip;

    private Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 launchVelocity)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.velocity = launchVelocity;
        rb.gravityScale = 1f;

        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded)
            return;

        int layer = collision.gameObject.layer;

        bool hitGround = (groundLayer.value & (1 << layer)) != 0;
        bool hitPlayer = (playerLayer.value & (1 << layer)) != 0;

        if (hitPlayer)
        {
            PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null)
                player.TakeDamage(damage);
        }

        if (hitGround || hitPlayer)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        if (audioSource != null && impactClip != null)
            audioSource.PlayOneShot(impactClip);

        // You can add explosion VFX here later

        Destroy(gameObject, 0.1f);
    }
}
