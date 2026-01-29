using UnityEngine;
using System.Collections;

public class RedSlime : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform ifa;
    public LayerMask ifaLayer;
    public Animator animator;
    public Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Animation Names")]
    public string idleLeftAnim;
    public string idleRightAnim;
    public string moveLeftAnim;
    public string moveRightAnim;
    public string attackLeftAnim;
    public string attackRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;
    public float attackCooldown = 1.2f;
    public float attackRange = 6f;
    public LayerMask playerLayer;

    [Header("Health")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float knockbackForce = 5f;
    public float damageFlashDuration = 1f;
    public Color damageColor = Color.red;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

    private bool isAttacking = false;
    private bool isDead = false;
    private bool facingRight = true;
    private float attackTimer = 0f;

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

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (isAttacking)
            return;

        Transform target = GetTarget();

        if (target != null)
            facingRight = target.position.x > transform.position.x;

        float distance = target != null ? Vector2.Distance(transform.position, target.position) : Mathf.Infinity;

        if (distance <= attackRange && attackTimer <= 0)
        {
            StartAttack(target);
            return;
        }

        if (distance <= detectionRange)
            ChaseTarget(target);
        else
            Idle();
    }

    Transform GetTarget()
    {
        bool playerInRange = player != null && Vector2.Distance(transform.position, player.position) <= detectionRange;
        bool ifaInRange = ifa != null && Vector2.Distance(transform.position, ifa.position) <= detectionRange;

        if (ifaInRange && ifa != null)
            return ifa;

        if (playerInRange)
            return player;

        return null;
    }

    void ChaseTarget(Transform target)
    {
        if (isDead || target == null)
            return;

        Vector2 direction = (target.position - transform.position).normalized;

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

    void StartAttack(Transform target)
    {
        if (isDead)
            return;

        isAttacking = true;
        attackTimer = attackCooldown;

        if (facingRight)
            animator.Play(attackRightAnim);
        else
            animator.Play(attackLeftAnim);

        Invoke(nameof(FireProjectile), 0.25f);
        Invoke(nameof(EndAttack), 0.6f);
    }

    void FireProjectile()
    {
        if (isDead || projectilePrefab == null)
            return;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RedSlimeProjectile p = proj.GetComponent<RedSlimeProjectile>();
        if (p != null)
            p.Init(direction, playerLayer, ifaLayer); // ✅ FIXED: passes all required arguments
    }

    void EndAttack()
    {
        isAttacking = false;
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

        yield return new WaitForSeconds(damageFlashDuration);

        sr.color = originalColor;
    }

    public void TakeDamage(int amount)
    {
        for (int i = 0; i < amount; i++)
            TakeHit();
    }
}
