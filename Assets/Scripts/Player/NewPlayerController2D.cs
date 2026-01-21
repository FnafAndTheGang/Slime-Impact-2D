using UnityEngine;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public Rigidbody2D rb;
    public Animator animator;
    private bool isGrounded;
    private bool isJumping;
    private bool facingRight = true;

    [Header("Animation Names (Left/Right)")]
    public string idleLeftAnim;
    public string idleRightAnim;
    public string runLeftAnim;
    public string runRightAnim;
    public string jumpLeftAnim;
    public string jumpRightAnim;
    public string fallLeftAnim;
    public string fallRightAnim;
    public string attackLeftAnim;
    public string attackRightAnim;
    public string hurtLeftAnim;
    public string hurtRightAnim;
    public string deathLeftAnim;
    public string deathRightAnim;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public Collider2D attackHitbox;
    public LayerMask enemyLayer;
    public Vector2 hitboxOffsetRight = new Vector2(0.6f, 0f);
    public Vector2 hitboxOffsetLeft = new Vector2(-0.6f, 0f);
    public float attackDuration = 0.2f;
    public bool isAttacking = false;

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Ghost Hearts (UI Images Only)")]
    public Image[] ghostHearts;

    [Header("Spear System")]
    public bool hasSpear = false;
    public GameObject spearObject;
    public Transform spearFollowRight;
    public Transform spearFollowLeft;

    private bool isDead = false;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        if (attackHitbox != null)
            attackHitbox.enabled = false;

        if (spearObject != null)
            spearObject.SetActive(hasSpear);

        UpdateGhostHearts();
    }

    void Update()
    {
        if (isDead)
            return;

        HandleMovement();
        HandleJump();
        HandleAttack();
        UpdateAttackHitboxPosition();
        UpdateSpearFollow();
        UpdateAnimationState();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput > 0) facingRight = true;
        else if (moveInput < 0) facingRight = false;
    }

    void HandleJump()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }

        if (isJumping && rb.velocity.y <= 0)
            isJumping = false;
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            isAttacking = true;

            StartAttackHitbox();
            Invoke(nameof(EndAttackHitbox), attackDuration);

            animator.Play(facingRight ? attackRightAnim : attackLeftAnim);
        }
    }

    void UpdateAnimationState()
    {
        // PRIORITY 1 — DEATH
        if (isDead)
        {
            animator.Play(facingRight ? deathRightAnim : deathLeftAnim);
            return;
        }

        // PRIORITY 2 — ATTACK
        if (isAttacking)
        {
            animator.Play(facingRight ? attackRightAnim : attackLeftAnim);
            return;
        }

        // PRIORITY 3 — FALL
        if (!isGrounded && rb.velocity.y < 0)
        {
            animator.Play(facingRight ? fallRightAnim : fallLeftAnim);
            return;
        }

        // PRIORITY 4 — JUMP
        if (!isGrounded && rb.velocity.y > 0)
        {
            animator.Play(facingRight ? jumpRightAnim : jumpLeftAnim);
            return;
        }

        // PRIORITY 5 — RUN
        if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            animator.Play(facingRight ? runRightAnim : runLeftAnim);
            return;
        }

        // PRIORITY 6 — IDLE
        animator.Play(facingRight ? idleRightAnim : idleLeftAnim);
    }

    void UpdateAttackHitboxPosition()
    {
        if (attackHitbox == null)
            return;

        Vector2 offset = facingRight ? hitboxOffsetRight : hitboxOffsetLeft;
        attackHitbox.transform.position = (Vector2)transform.position + offset;
    }

    public void StartAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
            attackHitbox.enabled = true;
        }
    }

    public void EndAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;

        isAttacking = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isAttacking)
            return;

        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            RedRiotBoss redRiot = other.GetComponentInParent<RedRiotBoss>();
            if (redRiot != null)
            {
                redRiot.TakeHitFrom(gameObject);
                return;
            }

            BlueSlime slime = other.GetComponentInParent<BlueSlime>();
            if (slime != null)
            {
                slime.TakeHit();
                return;
            }
        }
    }

    void UpdateSpearFollow()
    {
        if (!hasSpear || spearObject == null)
            return;

        if (isAttacking)
            return;

        if (facingRight && spearFollowRight != null)
            spearObject.transform.position = spearFollowRight.position;
        else if (!facingRight && spearFollowLeft != null)
            spearObject.transform.position = spearFollowLeft.position;

        spearObject.SetActive(true);
    }

    // -----------------------------
    // DAMAGE + DEATH + RESPAWN
    // -----------------------------

    public void TakeDamage()
    {
        TakeDamage(1);
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;

        UpdateGhostHearts();

        if (currentHealth <= 0)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            animator.Play(facingRight ? deathRightAnim : deathLeftAnim);

            Invoke(nameof(RespawnAtCheckpoint), 1.0f);
            return;
        }

        animator.Play(facingRight ? hurtRightAnim : hurtLeftAnim);
    }

    void RespawnAtCheckpoint()
    {
        transform.position = CheckpointManager.instance.GetLastCheckpointPosition();
        currentHealth = maxHealth;
        UpdateGhostHearts();
        isDead = false;
    }

    public void UpdateGhostHearts()
    {
        if (ghostHearts == null || ghostHearts.Length == 0)
            return;

        for (int i = 0; i < ghostHearts.Length; i++)
            ghostHearts[i].enabled = i < currentHealth;
    }
}
