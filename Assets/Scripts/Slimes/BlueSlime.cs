using UnityEngine;
using System.Collections;

public class BlueSlime : MonoBehaviour
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
    public string attackLeftAnim;
    public string attackRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;
    public float hopForce = 1f;

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

        Transform target = GetTarget();   // ⭐ NEW

        if (target != null)
            facingRight = target.position.x > transform.position.x;

        if (TargetInAttackRange(target) && attackTimer <= 0)
        {
            StartAttack(target);
            return;
        }

        float distance = target != null ? Vector2.Distance(transform.position, target.position) : Mathf.Infinity;

        if (distance <= detectionRange)
            ChaseTarget(target);
        else
            Idle();
    }

    // ⭐ NEW — chooses Ifa over player if both are in range
    Transform GetTarget()
    {
        bool playerInRange = player != null && Vector2.Distance(transform.position, player.position) <= detectionRange;
        bool ifaInRange = ifa != null && Vector2.Distance(transform.position, ifa.position) <= detectionRange;

        if (ifaInRange && ifa != null)
            return ifa; // Ifa ALWAYS takes priority

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

    // ⭐ NEW — checks attack range for either target
    bool TargetInAttackRange(Transform target)
    {
        if (target == null)
            return false;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        int combinedMask = playerLayer | ifaLayer;

        return Physics2D.Raycast(origin, direction, attackHitboxDistance, combinedMask);
    }

    void ChaseTarget(Transform target)
    {
        if (isDead || target == null)
            return;

        Vector2 direction = (target.position - transform.position).normalized;

        transform.position += new Vector3(direction.x * moveSpeed * Time.deltaTime, 0, 0);

        Hop();

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

        Invoke(nameof(DealDamage), 0.25f);
        Invoke(nameof(EndAttack), 0.6f);
    }

    void DealDamage()
    {
        if (isDead)
            return;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        int combinedMask = playerLayer | ifaLayer;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, attackHitboxDistance, combinedMask);

        if (hit.collider == null)
            return;

        // Damage player
        PlayerController2D p = hit.collider.GetComponent<PlayerController2D>();
        if (p != null)
        {
            p.TakeDamage(1);
            return;
        }

        // ⭐ Damage Ifa
        IfaEscortController ifaController = hit.collider.GetComponent<IfaEscortController>();
        if (ifaController != null)
        {
            ifaController.TakeDamage(1);
            return;
        }
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

        if (portraitController != null)
            portraitController.OnPlayerKillEnemy();

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position + new Vector3(direction.x * 0.5f, 0, 0);

        Gizmos.DrawLine(origin, origin + direction * attackHitboxDistance);
    }

    public void TakeDamage(int amount)
    {
        for (int i = 0; i < amount; i++)
            TakeHit();
    }
}
