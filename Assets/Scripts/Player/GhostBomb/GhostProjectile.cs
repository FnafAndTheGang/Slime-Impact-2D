using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifetime = 5f;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Damage Source")]
    public GameObject sourceObject; // The player or ability object that fired this

    private float direction;
    private float timer;

    // Called by GhostBomb when firing
    public void Init(float dir, GameObject source)
    {
        direction = dir;
        sourceObject = source;
        timer = lifetime;
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit Blue Slime
        BlueSlime slime = other.GetComponent<BlueSlime>();
        if (slime != null)
        {
            slime.TakeHit();
            Explode();
            return;
        }

        // Hit Red Riot Boss
        RedRiotBoss boss = other.GetComponent<RedRiotBoss>();
        if (boss != null)
        {
            boss.TakeHitFrom(sourceObject);
            Explode();
            return;
        }

        // Hit cracked tile
        if (other.CompareTag("Cracked"))
        {
            Destroy(other.gameObject);
            Explode();
            return;
        }

        // Hit anything else
        Explode();
    }

    void Explode()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
