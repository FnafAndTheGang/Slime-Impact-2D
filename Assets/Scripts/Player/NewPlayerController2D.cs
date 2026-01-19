using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 8f;
    public float jumpForce = 14f;
    private Rigidbody2D rb;
    private bool isGrounded;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Wall Slide Settings")]
    public float wallSlideSpeed = 1.5f;
    public float wallJumpForce = 14f;
    public float wallJumpHorizontalForce = 10f;

    private bool isTouchingWall = false;
    private bool isWallSliding = false;

    [Header("Attack Settings")]
    public float baseAttackCooldown = 0.5f;
    public float overloadCooldown = 1.5f;
    public float comboWindow = 4f;
    private float attackCooldownTimer = 0f;

    private int attackCount = 0;
    private float comboTimer = 0f;

    private float attackLockTimer = 0f;
    public bool isAttacking = false;

    [Header("Attack Hitbox")]
    public Collider2D attackHitbox;
    public Vector2 hitboxOffsetRight = new Vector2(0.6f, 0f);
    public Vector2 hitboxOffsetLeft = new Vector2(-0.6f, 0f);
    public LayerMask enemyLayer;

    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;
    public Image[] ghostHearts;

    [Header("Damage Sound")]
    public AudioSource audioSource;
    public AudioClip damageSound;

    [Header("Damage Voice Lines")]
    public AudioClip[] damageVoiceLines;
    public float voiceLineChance = 0.5f;

    [Header("Low Health Warning")]
    public AudioSource lowHealthSource;
    public AudioClip lowHealthLoop;

    [Header("Animation")]
    public Animator animator;

    public string idleRightAnim;
    public string idleLeftAnim;
    public string runRightAnim;
    public string runLeftAnim;
    public string jumpRightAnim;
    public string jumpLeftAnim;
    public string fallRightAnim;
    public string fallLeftAnim;
    public string attackRightAnim;
    public string attackLeftAnim;
    public string deathRightAnim;
    public string deathLeftAnim;

    [Header("Death Settings")]
    public AudioClip deathSound;
    public float deathYOffset = 0f;

    [Header("Weapon System")]
    public bool hasSpear = false;
    public GameObject spearObject;
    public Transform spearFollowRight;
    public Transform spearFollowLeft;

    [Header("Step Climb")]
    public float stepHeight = 1.0f;
    public float stepCheckDistance = 0.2f;

    [Header("Portrait")]
    public PlayerPortraitController portraitController;

    private bool facingRight = true;
    private bool isJumping = false;
    private bool isDead = false;

    private float horizontalInput = 0f;

    public static System.Action PlayerDiedEvent;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        Debug.Log("Player Start() — Health: " + currentHealth);

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (lowHealthSource != null && lowHealthLoop != null)
            lowHealthSource.clip = lowHealthLoop;

        UpdateGhostHearts();

        if (spearObject != null)
            spearObject.SetActive(false);

        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    void Update()
    {
        if (isDead)
            return;

        GroundCheck();
        WallCheck();

        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleAttack();
        UpdateComboTimer();
        UpdateAttackLock();
        UpdateSpearFollow();
        StepClimb();
        UpdateAttackHitboxPosition();

        UpdateAnimationState();
    }

    void GroundCheck()
    {
        bool leftHit = Physics2D.Raycast(leftFoot.position, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightFoot.position, Vector2.down, groundCheckDistance, groundLayer);

        bool wasGrounded = isGrounded;
        isGrounded = leftHit || rightHit;

        if (!wasGrounded && isGrounded)
            Debug.Log("Player landed on ground");
    }

    void WallCheck()
    {
        Vector2 wallOrigin = transform.position + new Vector3(facingRight ? 0.4f : -0.4f, 0, 0);

        isTouchingWall = Physics2D.Raycast(
            wallOrigin,
            facingRight ? Vector2.right : Vector2.left,
            0.1f,
            groundLayer
        );

        if (isTouchingWall)
            Debug.Log("Player touching wall on " + (facingRight ? "right" : "left"));
    }

    void HandleMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0 && !facingRight)
            Debug.Log("Facing RIGHT");
        else if (horizontalInput < 0 && facingRight)
            Debug.Log("Facing LEFT");

        if (horizontalInput > 0)
            facingRight = true;
        else if (horizontalInput < 0)
            facingRight = false;

        rb.velocity = new Vector2(horizontalInput * runSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetButtonDown("Jump");

        if (!jumpPressed)
            return;

        if (isGrounded)
        {
            Debug.Log("Jump!");
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }

        if (isWallSliding)
        {
            Debug.Log("Wall Jump!");
            isJumping = true;

            float jumpDir = facingRight ? -1 : 1;
            rb.velocity = new Vector2(jumpDir * wallJumpHorizontalForce, jumpForce);

            isWallSliding = false;
        }
    }

    void HandleWallSlide()
    {
        isWallSliding = false;

        if (!isGrounded && isTouchingWall)
        {
            if ((facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0))
            {
                if (!isWallSliding)
                    Debug.Log("Wall Sliding");

                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    void HandleAttack()
    {
        if (!hasSpear)
            return;

        attackCooldownTimer -= Time.deltaTime;

        bool attackPressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.JoystickButton2);

        if (!attackPressed)
            return;

        if (attackCooldownTimer > 0f)
        {
            Debug.Log("Attack pressed but on cooldown");
            return;
        }

        Debug.Log("ATTACK STARTED");
        isAttacking = true;
        attackLockTimer = 0.25f;

        attackCount++;
        comboTimer = comboWindow;

        if (attackCount >= 4)
        {
            Debug.Log("OVERLOAD ATTACK");
            attackCooldownTimer = overloadCooldown;
            attackCount = 0;
        }
        else
        {
            attackCooldownTimer = baseAttackCooldown;
        }
    }

    void UpdateAttackHitboxPosition()
    {
        if (attackHitbox == null)
            return;

        Vector2 offset = facingRight ? hitboxOffsetRight : hitboxOffsetLeft;
        attackHitbox.transform.position = (Vector2)transform.position + offset;

        Debug.Log("Hitbox Pos: " + attackHitbox.transform.position + " | Facing: " + (facingRight ? "Right" : "Left"));
    }

    public void StartAttackHitbox()
    {
        if (attackHitbox != null)
        {
            Debug.Log("Hitbox ENABLED");
            attackHitbox.enabled = true;
        }
    }

    public void EndAttackHitbox()
    {
        if (attackHitbox != null)
        {
            Debug.Log("Hitbox DISABLED");
            attackHitbox.enabled = false;
        }
    }

    void UpdateAttackLock()
    {
        if (attackLockTimer > 0)
        {
            attackLockTimer -= Time.deltaTime;
            if (attackLockTimer <= 0)
            {
                Debug.Log("Attack ended");
                isAttacking = false;
            }
        }
    }

    void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                Debug.Log("Combo reset");
                attackCount = 0;
            }
        }
    }

    void UpdateSpearFollow()
    {
        if (!hasSpear || spearObject == null)
            return;

        if (isAttacking)
        {
            spearObject.SetActive(false);
            return;
        }

        spearObject.SetActive(true);

        spearObject.transform.position =
            facingRight ? spearFollowRight.position : spearFollowLeft.position;
    }

    void StepClimb()
    {
        if (horizontalInput == 0)
            return;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D lowerHit = Physics2D.Raycast(
            transform.position + Vector3.down * 0.1f,
            direction,
            stepCheckDistance,
            groundLayer
        );

        RaycastHit2D upperHit = Physics2D.Raycast(
            transform.position + Vector3.up * stepHeight,
            direction,
            stepCheckDistance,
            groundLayer
        );

        if (lowerHit && !upperHit)
        {
            Debug.Log("Step Climb");
            transform.position += new Vector3(0, stepHeight, 0);
        }
    }

    void UpdateAnimationState()
    {
        if (isDead)
            return;

        bool isFalling = !isGrounded && rb.velocity.y < -0.1f && !isJumping;

        if (isJumping && rb.velocity.y <= 0f)
            isJumping = false;

        if (isAttacking)
        {
            PlayAnimation(facingRight ? attackRightAnim : attackLeftAnim);
            return;
        }

        if (isJumping)
        {
            PlayAnimation(facingRight ? jumpRightAnim : jumpLeftAnim);
            return;
        }

        if (isWallSliding)
            return;

        if (isFalling)
        {
            PlayAnimation(facingRight ? fallRightAnim : fallLeftAnim);
            return;
        }

        if (isGrounded && Mathf.Abs(horizontalInput) > 0.01f)
        {
            PlayAnimation(facingRight ? runRightAnim : runLeftAnim);
            return;
        }

        if (isGrounded)
        {
            PlayAnimation(facingRight ? idleRightAnim : idleLeftAnim);
            return;
        }
    }

    void PlayAnimation(string animName)
    {
        if (!string.IsNullOrEmpty(animName))
            animator.Play(animName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttacking)
            return;

        Debug.Log("Hitbox collided with: " + other.name);

        BlueSlime slime = other.GetComponent<BlueSlime>();
        if (slime != null)
        {
            Debug.Log("Slime HIT!");
            slime.TakeHit();
        }
    }

    // ⭐ FIXED: DAMAGE SOUND + VOICE LINES NOW PLAY
    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        Debug.Log("PLAYER TOOK DAMAGE: " + amount);

        // ⭐ Play damage sound
        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);

        // ⭐ Random voice line
        if (damageVoiceLines != null && damageVoiceLines.Length > 0)
        {
            if (Random.value <= voiceLineChance)
            {
                AudioClip clip = damageVoiceLines[Random.Range(0, damageVoiceLines.Length)];
                audioSource.PlayOneShot(clip);
            }
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateGhostHearts();

        if (portraitController != null)
            portraitController.OnPlayerDamaged();

        if (currentHealth <= 0)
            Die();
    }

    void UpdateGhostHearts()
    {
        for (int i = 0; i < ghostHearts.Length; i++)
            ghostHearts[i].enabled = (i < currentHealth);
    }

    void Die()
    {
        if (isDead)
            return;

        Debug.Log("PLAYER DIED");

        isDead = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        string deathAnim = facingRight ? deathRightAnim : deathLeftAnim;
        animator.Play(deathAnim);

        PlayerDiedEvent?.Invoke();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        Vector3 respawnPos = CheckpointManager.instance.GetLastCheckpointPosition();
        transform.position = respawnPos;

        rb.isKinematic = false;
        currentHealth = maxHealth;
        UpdateGhostHearts();
        isDead = false;
    }
}
