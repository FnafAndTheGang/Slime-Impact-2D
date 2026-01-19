using UnityEngine;
using System.Collections;

public class BombSlime : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Explosion Light")]
    public UnityEngine.Rendering.Universal.Light2D explosionLight; // ⭐ NEW

    [Header("Animation Names")]
    public string idleLeftAnim;
    public string idleRightAnim;
    public string moveLeftAnim;
    public string moveRightAnim;
    public string explodeLeftAnim;
    public string explodeRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;
    public float explodeRange = 1.5f;

    [Header("Explosion")]
    public float explosionRadius = 1.5f;
    public LayerMask playerLayer;
    public LayerMask slimeLayer;
    public int explosionDamage = 2;
    public float explosionDelay = 0.3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip explosionSound;
    public AudioClip deathSound;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.2f;

    [Header("Health")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float knockbackForce = 5f;
    public float damageFlashDuration = 0.1f;
    public Color damageColor = Color.red;

    private bool isExploding = false;
    private bool isDead = false;
    private bool facingRight = true;
    private bool diedFromExplosion = false;

    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        // Make sure explosion light starts OFF
        if (explosionLight != null)
            explosionLight.enabled = false;
    }

    void Update()
    {
        if (isDead || isExploding)
            return;

        if (IsGrounded())
            transform.rotation = Quaternion.Euler(0, 0, 0);

        if (player != null)
            facingRight = player.position.x > transform.position.x;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= explodeRange)
        {
            StartExplosion();
            return;
        }

        if (distance <= detectionRange)
            ChasePlayer();
        else
            Idle();
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
    }

    void ChasePlayer()
    {
        if (isDead)
            return;

        Vector2 direction = (player.position - transform.position).normalized;

        transform.position += new Vector3(direction.x * moveSpeed * Time.deltaTime, 0, 0);

        if (direction.x > 0)
        {
            facingRight = true;
            animator.Play(moveRightAnim);
        }
        else
        {
            facingRight = false;
            animator.Play(moveLeftAnim);
        }
    }

    void Idle()
    {
        if (isDead)
            return;

        if (facingRight)
            animator.Play(idleRightAnim);
        else
            animator.Play(idleLeftAnim);
    }

    void StartExplosion()
    {
        if (isExploding || isDead)
            return;

        isExploding = true;
        diedFromExplosion = true;

        if (facingRight)
            animator.Play(explodeRightAnim);
        else
            animator.Play(explodeLeftAnim);

        // ⭐ TURN ON LIGHT
        if (explosionLight != null)
            explosionLight.enabled = true;

        if (audioSource != null && explosionSound != null)
            audioSource.PlayOneShot(explosionSound);

        StartCoroutine(ExplosionSequence());
    }

    IEnumerator ExplosionSequence()
    {
        yield return new WaitForSeconds(explosionDelay);

        // ⭐ DAMAGE PLAYER
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);

        if (playerHit != null)
        {
            PlayerController2D p = playerHit.GetComponent<PlayerController2D>();
            if (p != null)
                p.TakeDamage(explosionDamage);
        }

        // ⭐ DAMAGE OTHER SLIMES
        Collider2D[] slimesHit = Physics2D.OverlapCircleAll(transform.position, explosionRadius, slimeLayer);

        foreach (Collider2D c in slimesHit)
        {
            if (c.gameObject == this.gameObject)
                continue;

            BlueSlime blue = c.GetComponent<BlueSlime>();
            if (blue != null)
                blue.TakeHit();

            BombSlime bomb = c.GetComponent<BombSlime>();
            if (bomb != null)
                bomb.TakeExplosionDamage(2);
        }

        yield return new WaitForSeconds(0.1f);

        // ⭐ TURN OFF LIGHT before dying
        if (explosionLight != null)
            explosionLight.enabled = false;

        if (!isDead)
            Die();
    }

    public void TakeExplosionDamage(int dmg)
    {
        if (isDead)
            return;

        currentHealth -= dmg;

        if (currentHealth <= 0)
            Die();
    }

    public void TakeHit()
    {
        if (isDead || isExploding)
            return;

        currentHealth -= 1;

        StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFeedback()
    {
        sr.color = damageColor;

        float dir = transform.position.x < player.position.x ? -1 : 1;
        rb.velocity = new Vector2(dir * knockbackForce, rb.velocity.y);

        yield return new WaitForSeconds(damageFlashDuration);

        sr.color = originalColor;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (!diedFromExplosion)
        {
            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);
        }

        if (facingRight)
            animator.Play(deathRightAnim);
        else
            animator.Play(deathLeftAnim);

        Destroy(gameObject, 0.4f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
