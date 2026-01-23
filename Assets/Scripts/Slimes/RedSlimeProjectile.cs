using UnityEngine;

public class RedSlimeProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 4f;
    public int damage = 1;
    public LayerMask playerLayer;

    private Vector2 moveDir;

    public void Init(Vector2 direction)
    {
        moveDir = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Optional: destroy on walls or ground
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
