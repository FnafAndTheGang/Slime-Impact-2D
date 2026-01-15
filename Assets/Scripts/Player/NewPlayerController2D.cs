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
    private bool isAttacking = false;

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

    // Death animations inside Animation section
    public string deathRightAnim;
    public string deathLeftAnim;

    [Header("Weapon System")]
    public bool hasSpear = false;
    public GameObject spearObject;
    public Transform spearFollowRight;
    public Transform spearFollowLeft;

    [Header("Step Climb")]
    public float stepHeight = 1.0f;
    public float stepCheckDistance = 0.2f;

    private bool facingRight = true;
    private bool isJumping = false;
    private bool isDead = false;

    private float horizontalInput = 0f;

    public static System.Action PlayerDiedEvent;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (lowHealthSource != null && lowHealthLoop != null)
            lowHealthSource.clip = lowHealthLoop;

        UpdateGhostHearts();

        if (spearObject != null)
            spearObject.SetActive(false);
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

        UpdateAnimationState();
    }

    // -------------------------
    // GROUND CHECK
    // -------------------------
    void GroundCheck()
    {
        bool leftHit = Physics2D.Raycast(leftFoot.position, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightFoot.position, Vector2.down, groundCheckDistance, groundLayer);

        bool wasGrounded = isGrounded;
        isGrounded = leftHit || rightHit;

        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
        }
    }

    // -------------------------
    // WALL CHECK
    // -------------------------
    void WallCheck()
    {
        Vector2 wallOrigin = transform.position + new Vector3(facingRight ? 0.4f : -0.4f, 0, 0);

        isTouchingWall = Physics2D.Raycast(
            wallOrigin,
            facingRight ? Vector2.right : Vector2.left,
            0.1f,
            groundLayer
        );
    }

    // -------------------------
    // MOVEMENT
    // -------------------------
    void HandleMovement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0)
            facingRight = true;
        else if (horizontalInput < 0)
            facingRight = false;

        rb.velocity = new Vector2(horizontalInput * runSpeed, rb.velocity.y);
    }

    // -------------------------
    // JUMPING + WALL JUMP
    // -------------------------
    void HandleJump()
    {
        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetButtonDown("Jump");

        if (!jumpPressed)
            return;

        if (isGrounded)
        {
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }

        if (isWallSliding)
        {
            isJumping = true;

            float jumpDir = facingRight ? -1 : 1;
            rb.velocity = new Vector2(jumpDir * wallJumpHorizontalForce, jumpForce);

            isWallSliding = false;
        }
    }

    // -------------------------
    // WALL SLIDE
    // -------------------------
    void HandleWallSlide()
    {
        isWallSliding = false;

        if (!isGrounded && isTouchingWall)
        {
            if ((facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0))
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    // -------------------------
    // ATTACKING
    // -------------------------
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
            return;

        isAttacking = true;
        attackLockTimer = 0.25f;

        attackCount++;
        comboTimer = comboWindow;

        if (attackCount >= 4)
        {
            attackCooldownTimer = overloadCooldown;
            attackCount = 0;
        }
        else
        {
            attackCooldownTimer = baseAttackCooldown;
        }
    }

    void UpdateAttackLock()
    {
        if (attackLockTimer > 0)
        {
            attackLockTimer -= Time.deltaTime;
            if (attackLockTimer <= 0)
                isAttacking = false;
        }
    }

    void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
                attackCount = 0;
        }
    }

    // -------------------------
    // SPEAR FOLLOW LOGIC
    // -------------------------
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

        if (facingRight)
            spearObject.transform.position = spearFollowRight.position;
        else
            spearObject.transform.position = spearFollowLeft.position;
    }

    // -------------------------
    // STEP CLIMB
    // -------------------------
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
            transform.position += new Vector3(0, stepHeight, 0);
        }
    }

    // -------------------------
    // ANIMATION STATE MACHINE
    // -------------------------
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

    // -------------------------
    // HEALTH SYSTEM
    // -------------------------
    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateGhostHearts();

        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);

        PlayRandomDamageVoiceLine();

        Invoke(nameof(HandleLowHealthLoop), 0.1f);

        if (currentHealth <= 0)
            Die();
    }

    void PlayRandomDamageVoiceLine()
    {
        if (audioSource == null || damageVoiceLines == null || damageVoiceLines.Length == 0)
            return;

        if (Random.value > voiceLineChance)
            return;

        int index = Random.Range(0, damageVoiceLines.Length);
        AudioClip chosenClip = damageVoiceLines[index];

        if (chosenClip != null)
            audioSource.PlayOneShot(chosenClip);
    }

    void HandleLowHealthLoop()
    {
        if (lowHealthSource == null || lowHealthLoop == null)
            return;

        if (currentHealth == 1)
        {
            if (!lowHealthSource.isPlaying)
                lowHealthSource.Play();
        }
        else
        {
            if (lowHealthSource.isPlaying)
                lowHealthSource.Stop();
        }
    }

    void UpdateGhostHearts()
    {
        if (ghostHearts == null || ghostHearts.Length == 0)
        {
            Debug.LogWarning("Ghost hearts array is empty or not assigned.");
            return;
        }

        for (int i = 0; i < ghostHearts.Length; i++)
        {
            if (ghostHearts[i] == null)
            {
                Debug.LogWarning($"Ghost heart at index {i} is NULL.");
                continue;
            }

            ghostHearts[i].enabled = (i < currentHealth);
        }
    }

    // -------------------------
    // DEATH + RESPAWN
    // -------------------------
    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (lowHealthSource != null && lowHealthSource.isPlaying)
            lowHealthSource.Stop();

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        string deathAnim = facingRight ? deathRightAnim : deathLeftAnim;
        if (!string.IsNullOrEmpty(deathAnim))
            animator.Play(deathAnim);

        PlayerDiedEvent?.Invoke();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Wait for death animation to finish
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        // Fade out
        if (ScreenFader.instance != null)
            yield return ScreenFader.instance.FadeOut();

        // Teleport to checkpoint
        transform.position = CheckpointManager.instance.GetLastCheckpointPosition();

        // Reset player
        rb.isKinematic = false;
        currentHealth = maxHealth;
        UpdateGhostHearts();
        isDead = false;

        // Fade in
        if (ScreenFader.instance != null)
            yield return ScreenFader.instance.FadeIn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (leftFoot != null)
            Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.down * groundCheckDistance);

        if (rightFoot != null)
            Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.down * groundCheckDistance);
    }
}
