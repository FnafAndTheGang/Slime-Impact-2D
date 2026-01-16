using UnityEngine;
using System.Collections;

public class BlueSlime : MonoBehaviour
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
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;

    [Header("Attack")]
    public float attackHitboxDistance = 1.0f;
    public float attackCooldown = 1.0f;
    public LayerMask playerLayer;

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

        if (IsGrounded())
            transform.rotation = Quaternion.Euler(0, 0, 0);

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (isAttacking)
            return;

        if (PlayerInAttackRange() && attackTimer <= 0)
        {
            StartAttack();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

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

    bool PlayerInAttackRange()
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        return Physics2D.Raycast(origin, direction, attackHitboxDistance, playerLayer);
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

    void StartAttack()
    {
        if (isDead)
            return;

        isAttacking = true;
        attackTimer = attackCooldown;

        if (facingRight)
            animator.Play(attackRightAnim);
        else
            animator.Play(attackLeftAnim);

        Invoke(nameof(DealDamage), 0.25f);
        Invoke(nameof(EndAttack), 0.6f);
    }

    void DealDamage()
    {
        if (isDead)
            return;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackHitboxDistance, playerLayer);

        if (hit.collider != null)
        {
            PlayerController2D p = hit.collider.GetComponent<PlayerController2D>();
            if (p != null)
                p.TakeDamage(1);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    // -------------------------
    // DAMAGE FROM PLAYER
    // -------------------------
    public void TakeHit()
    {
        if (isDead)
            return;

        currentHealth -= 1;

        // Flash red + knockback
        StartCoroutine(DamageFeedback());

        if (currentHealth > 0)
            return;

        // Slime dies
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
        // Flash red
        sr.color = damageColor;

        // Knockback direction (away from player)
        float dir = transform.position.x < player.position.x ? -1 : 1;
        rb.velocity = new Vector2(dir * knockbackForce, rb.velocity.y);

        yield return new WaitForSeconds(damageFlashDuration);

        // Restore color
        sr.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        Gizmos.DrawLine(origin, origin + direction * attackHitboxDistance);
    }
}
