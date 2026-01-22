using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifetime = 5f;

    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Audio")]
    public AudioClip explosionSound;
    public AudioSource audioSource;

    [Header("Sprites")]
    public Sprite rightSprite;
    public Sprite leftSprite;
    private SpriteRenderer spriteRenderer;

    [Header("Damage Source")]
    public GameObject sourceObject; // The player or ability object that fired this

    private float direction;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called by GhostBomb when firing
    public void Init(float dir, GameObject source)
    {
        direction = dir;
        sourceObject = source;
        timer = lifetime;

        // Set sprite based on direction
        if (spriteRenderer != null)
        {
            if (direction > 0 && rightSprite != null)
                spriteRenderer.sprite = rightSprite;

            if (direction < 0 && leftSprite != null)
                spriteRenderer.sprite = leftSprite;
        }
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
        // Blue Slime
        BlueSlime slime = other.GetComponent<BlueSlime>();
        if (slime != null)
        {
            slime.TakeHit();
            Explode();
            return;
        }

        // Red Riot Boss
        RedRiotBoss boss = other.GetComponent<RedRiotBoss>();
        if (boss != null)
        {
            boss.TakeHitFrom(sourceObject);
            Explode();
            return;
        }

        // Cracked tile
        if (other.CompareTag("Cracked"))
        {
            Destroy(other.gameObject);
            Explode();
            return;
        }

        // Anything else
        Explode();
    }

    void Explode()
    {
        // Play explosion sound
        if (audioSource != null && explosionSound != null)
            audioSource.PlayOneShot(explosionSound);

        // Spawn explosion VFX
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
