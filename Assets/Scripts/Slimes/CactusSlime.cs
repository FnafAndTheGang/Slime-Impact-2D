using UnityEngine;
using System.Collections;

public class CactusSlime : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform ifa;              // ⭐ NEW
    public LayerMask ifaLayer;         // ⭐ NEW
    public Animator animator;
    public Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Animation Names")]
    public string idleLeftAnim;
    public string idleRightAnim;
    public string moveLeftAnim;
    public string moveRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float hopForce = 1f;

    [Header("Ranges")]
    public float chaseRange = 5f;
    public float shootRange = 25f;

    [Header("Shooting")]
    public GameObject spikePrefab;
    public Transform shootPointLeft;
    public Transform shootPointRight;
    public Transform shootPointUp;
    public float shootCooldown = 3f;
    private float shootTimer = 0f;

    [Header("Touch Damage")]
    public LayerMask playerLayer;
    public int touchDamage = 2;
    public float touchDamageCooldown = 1f;
    private float touchDamageTimer = 0f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.2f;

    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float knockbackForce = 5f;
    public float damageFlashDuration = 1f;
    public Color damageColor = Color.red;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

    private bool isDead = false;
    private bool facingRight = true;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void Update()
    {
        if (isDead)
            return;

        if (IsGrounded())
            transform.rotation = Quaternion.Euler(0, 0, 0);

        Transform target = GetTarget(); // ⭐ NEW

        if (target == null)
        {
            Idle();
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        CheckTouchDamage();

        facingRight = target.position.x > transform.position.x;

        if (distance <= shootRange && distance > chaseRange)
        {
            Idle();
            ShootLogic(target);
            return;
        }

        if (distance <= chaseRange)
        {
            ChaseTarget(target);
            return;
        }

        Idle();
    }

    // ⭐ NEW — Ifa ALWAYS takes priority when both are in range
    Transform GetTarget()
    {
        bool playerInRange = player != null && Vector2.Distance(transform.position, player.position) <= shootRange;
        bool ifaInRange = ifa != null && Vector2.Distance(transform.position, ifa.position) <= shootRange;

        if (ifaInRange && ifa != null)
            return ifa;

        if (playerInRange)
            return player;

        return null;
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

    void Hop()
    {
        if (!IsGrounded())
            return;

        rb.velocity = new Vector2(rb.velocity.x, hopForce);
    }

    void ChaseTarget(Transform target)
    {
        if (isDead)
            return;

        Vector2 direction = (target.position - transform.position).normalized;

        transform.position += new Vector3(direction.x * moveSpeed * Time.deltaTime, 0, 0);

        Hop();

        if (direction.x > 0)
            animator.Play(moveRightAnim);
        else
            animator.Play(moveLeftAnim);
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

    void ShootLogic(Transform target)
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            shootTimer = shootCooldown;
            ShootSpikes(target);
        }
    }

    void ShootSpikes(Transform target)
    {
        Vector2 dirLeft = new Vector2(-1, 0);
        Vector2 dirRight = new Vector2(1, 0);
        Vector2 dirUp = new Vector2(0, 1);

        FireSpike(dirLeft, shootPointLeft);
        FireSpike(dirRight, shootPointRight);
        FireSpike(dirUp, shootPointUp);
    }

    void FireSpike(Vector2 dir, Transform firePoint)
    {
        GameObject spike = Instantiate(spikePrefab, firePoint.position, firePoint.rotation);
        CactusSpike projectile = spike.GetComponent<CactusSpike>();
        projectile.Init(dir, playerLayer, ifaLayer); // ⭐ NEW
    }

    void CheckTouchDamage()
    {
        touchDamageTimer -= Time.deltaTime;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.8f, playerLayer | ifaLayer);

        if (hit != null && touchDamageTimer <= 0f)
        {
            PlayerController2D p = hit.GetComponent<PlayerController2D>();
            if (p != null)
            {
                p.TakeDamage(touchDamage);
                touchDamageTimer = touchDamageCooldown;
                return;
            }

            IfaEscortController ifaC = hit.GetComponent<IfaEscortController>();
            if (ifaC != null)
            {
                ifaC.TakeDamage(touchDamage);
                touchDamageTimer = touchDamageCooldown;
                return;
            }
        }
    }

    public void TakeHit()
    {
        if (isDead)
            return;

        currentHealth -= 1;
        StartCoroutine(DamageFeedback());

        if (currentHealth > 0)
            return;

        isDead = true;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (facingRight)
            animator.Play(deathRightAnim);
        else
            animator.Play(deathLeftAnim);

        Destroy(gameObject, 0.4f);
    }

    IEnumerator DamageFeedback()
    {
        sr.color = damageColor;

        float dir = transform.position.x < player.position.x ? -1 : 1;

        rb.velocity = new Vector2(dir * knockbackForce, rb.velocity.y);

        yield return new WaitForSeconds(0.1f);
        rb.velocity = new Vector2(0, rb.velocity.y);

        yield return new WaitForSeconds(damageFlashDuration - 0.1f);

        sr.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
