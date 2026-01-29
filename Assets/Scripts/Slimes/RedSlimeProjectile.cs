using UnityEngine;

public class RedSlimeProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 4f;
    public int damage = 1;
    public LayerMask playerLayer;
    public LayerMask ifaLayer;

    private Vector2 moveDir;

    public void Init(Vector2 direction, LayerMask player, LayerMask ifa)
    {
        moveDir = direction.normalized;
        playerLayer = player;
        ifaLayer = ifa;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Damage player
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        // Damage Ifa
        if (((1 << other.gameObject.layer) & ifaLayer) != 0)
        {
            IfaEscortController ifa = other.GetComponent<IfaEscortController>();
            if (ifa != null)
            {
                ifa.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        // Optional: destroy on ground
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
