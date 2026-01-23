using UnityEngine;
using System.Collections;

public class RedSlime : MonoBehaviour
{
    [Header("References")]
    public Transform player;
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
    public string shootLeftAnim;
    public string shootRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;

    [Header("Attack")]
    public float meleeRange = 3f;
    public float attackCooldown = 1f;
    public LayerMask playerLayer;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;
    public AudioClip shootSound;   // 🔥 NEW

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.2f;

    [Header("Health")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Damage Feedback")]
    public float knockbackForce = 5f;
    public float damageFlashDuration = 1f;
    public Color damageColor = Color.red;

    [Header("Portrait")]
    public PlayerPortraitController portraitController;

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

        if (IsGrounded())
            transform.rotation = Quaternion.Euler(0, 0, 0);

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (isAttacking)
            return;

        if (player != null)
            facingRight = player.position.x > transform.position.x;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= meleeRange && attackTimer <= 0)
        {
            StartMeleeAttack();
            return;
        }

        if (distance > meleeRange && attackTimer <= 0)
        {
            StartShootAttack();
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
        if (isDead || player == null)
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

    // -----------------------------
    // MELEE ATTACK
    // -----------------------------
    void StartMeleeAttack()
    {
        if (isDead)
            return;

        isAttacking = true;
        attackTimer = attackCooldown;

        if (facingRight)
            animator.Play(attackRightAnim);
        else
            animator.Play(attackLeftAnim);

        Invoke(nameof(DealMeleeDamage), 0.25f);
        Invoke(nameof(EndAttack), 0.6f);
    }

    void DealMeleeDamage()
    {
        if (isDead)
            return;

        Vector2 center = transform.position + new Vector3(facingRight ? 0.6f : -0.6f, 0, 0);

        Collider2D hit = Physics2D.OverlapCircle(center, 0.6f, playerLayer);

        if (hit != null)
        {
            PlayerController2D p = hit.GetComponent<PlayerController2D>();
            if (p != null)
                p.TakeDamage(1);
        }
    }

    // -----------------------------
    // SHOOT ATTACK
    // -----------------------------
    void StartShootAttack()
    {
        if (isDead)
            return;

        isAttacking = true;
        attackTimer = attackCooldown;

        if (facingRight)
            animator.Play(shootRightAnim);
        else
            animator.Play(shootLeftAnim);

        Invoke(nameof(FireProjectile), 0.3f);
        Invoke(nameof(EndAttack), 0.6f);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || shootPoint == null)
            return;

        // 🔥 Play shooting sound
        PlayShootSound();

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        RedSlimeProjectile p = proj.GetComponent<RedSlimeProjectile>();
        if (p != null)
            p.Init(facingRight ? Vector2.right : Vector2.left);
    }

    void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    // -----------------------------
    // DAMAGE + DEATH
    // -----------------------------
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

        if (portraitController != null)
            portraitController.OnPlayerKillEnemy();

        Destroy(gameObject, 0.4f);
    }

    public void TakeDamage(int amount)
    {
        for (int i = 0; i < amount; i++)
            TakeHit();
    }

    IEnumerator DamageFeedback()
    {
        sr.color = damageColor;

        float dir = transform.position.x < player.position.x ? -1 : 1;
        rb.velocity = new Vector2(dir * knockbackForce, rb.velocity.y);

        yield return new WaitForSeconds(damageFlashDuration);

        sr.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + new Vector3(facingRight ? 0.6f : -0.6f, 0, 0), 0.6f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
