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

    [Header("Attack Cooldowns")]
    public float attackCooldown = 0.2f;
    private float nextAttackTime = 0f;

    [Header("Combo System")]
    public float comboWindow = 3f;
    public int comboRequired = 4;
    public float comboLockout = 2f;
    private int comboCount = 0;
    private float comboTimer = 0f;
    private bool comboLocked = false;

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

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip heartbeatSound;

    private bool heartbeatPlaying = false;
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
        UpdateComboTimer();
        UpdateHeartbeat();
    }

    // -----------------------------
    // MOVEMENT
    // -----------------------------

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

    // -----------------------------
    // ATTACK + COMBO SYSTEM
    // -----------------------------

    void HandleAttack()
    {
        if (comboLocked)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;

            // Play sound
            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound);

            // Start hitbox
            StartAttackHitbox();
            Invoke(nameof(EndAttackHitbox), attackDuration);

            // Animation
            animator.Play(facingRight ? attackRightAnim : attackLeftAnim);

            // Cooldown
            nextAttackTime = Time.time + attackCooldown;

            // Combo tracking
            comboCount++;
            comboTimer = comboWindow;

            if (comboCount >= comboRequired)
            {
                comboLocked = true;
                Invoke(nameof(ResetComboLockout), comboLockout);
            }
        }
    }

    void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
                comboCount = 0;
        }
    }

    void ResetComboLockout()
    {
        comboLocked = false;
        comboCount = 0;
    }

    // -----------------------------
    // ANIMATION PRIORITY
    // -----------------------------

    void UpdateAnimationState()
    {
        if (isDead)
        {
            animator.Play(facingRight ? deathRightAnim : deathLeftAnim);
            return;
        }

        if (isAttacking)
        {
            animator.Play(facingRight ? attackRightAnim : attackLeftAnim);
            return;
        }

        if (!isGrounded && rb.velocity.y < 0)
        {
            animator.Play(facingRight ? fallRightAnim : fallLeftAnim);
            return;
        }

        if (!isGrounded && rb.velocity.y > 0)
        {
            animator.Play(facingRight ? jumpRightAnim : jumpLeftAnim);
            return;
        }

        if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            animator.Play(facingRight ? runRightAnim : runLeftAnim);
            return;
        }

        animator.Play(facingRight ? idleRightAnim : idleLeftAnim);
    }

    // -----------------------------
    // ATTACK HITBOX
    // -----------------------------

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

    // -----------------------------
    // SPEAR FOLLOW
    // -----------------------------

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

        // Play damage sound
        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);

        if (currentHealth <= 0)
        {
            isDead = true;
            rb.velocity = Vector2.zero;

            // Death sound
            if (audioSource != null && deathSound != null)
                audioSource.PlayOneShot(deathSound);

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

    // -----------------------------
    // HEART UI + HEARTBEAT
    // -----------------------------

    public void UpdateGhostHearts()
    {
        if (ghostHearts == null || ghostHearts.Length == 0)
            return;

        for (int i = 0; i < ghostHearts.Length; i++)
            ghostHearts[i].enabled = i < currentHealth;
    }

    void UpdateHeartbeat()
    {
        if (currentHealth == 1 && !heartbeatPlaying)
        {
            heartbeatPlaying = true;
            if (audioSource != null && heartbeatSound != null)
                audioSource.PlayOneShot(heartbeatSound);
        }
        else if (currentHealth > 1)
        {
            heartbeatPlaying = false;
        }
    }
}
