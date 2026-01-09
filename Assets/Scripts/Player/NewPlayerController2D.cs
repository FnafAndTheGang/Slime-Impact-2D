using UnityEngine;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 8f;
    public float jumpForce = 14f;
    private Rigidbody2D rb;
    private bool isGrounded;

    [Header("Ground Check (Dual Raycast)")]
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

    [Header("Health Settings (UI Ghosts)")]
    public int maxHealth = 5;
    public int currentHealth;
    public Image[] ghostHearts;

    [Header("Animation")]
    public Animator animator;

    public string idleRightAnim;
    public string idleLeftAnim;
    public string runRightAnim;
    public string runLeftAnim;
    public string jumpRightAnim;
    public string jumpLeftAnim;
    public string attackRightAnim;
    public string attackLeftAnim;

    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        UpdateGhostHearts();
    }

    void Update()
    {
        GroundCheck();
        WallCheck();

        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleAttack();
        UpdateComboTimer();
    }

    // -------------------------
    // GROUND CHECK
    // -------------------------
    void GroundCheck()
    {
        bool leftHit = Physics2D.Raycast(leftFoot.position, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightFoot.position, Vector2.down, groundCheckDistance, groundLayer);

        isGrounded = leftHit || rightHit;
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
        float input = Input.GetAxisRaw("Horizontal");

        if (input == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);

            if (isGrounded)
                PlayAnimation(facingRight ? idleRightAnim : idleLeftAnim);

            return;
        }

        if (input > 0 && !facingRight)
            Flip(true);
        else if (input < 0 && facingRight)
            Flip(false);

        rb.velocity = new Vector2(input * runSpeed, rb.velocity.y);

        if (isGrounded)
            PlayAnimation(facingRight ? runRightAnim : runLeftAnim);
    }

    // -------------------------
    // JUMPING + WALL JUMP
    // -------------------------
    void HandleJump()
    {
        bool jumpPressed =
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetButtonDown("Jump");

        if (jumpPressed)
        {
            // Normal jump
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                PlayAnimation(facingRight ? jumpRightAnim : jumpLeftAnim);
            }
            // Wall jump
            else if (isWallSliding)
            {
                float jumpDir = facingRight ? -1 : 1;

                rb.velocity = new Vector2(jumpDir * wallJumpHorizontalForce, wallJumpForce);

                // Flip player after wall jump
                Flip(jumpDir > 0);

                isWallSliding = false;
            }
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
            // Only slide if moving toward the wall
            float input = Input.GetAxisRaw("Horizontal");

            if ((facingRight && input > 0) || (!facingRight && input < 0))
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
        attackCooldownTimer -= Time.deltaTime;

        bool attackPressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.JoystickButton2);

        if (attackPressed && attackCooldownTimer <= 0f)
        {
            PlayAnimation(facingRight ? attackRightAnim : attackLeftAnim);

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
    }

    void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                attackCount = 0;
            }
        }
    }

    // -------------------------
    // HEALTH SYSTEM
    // -------------------------
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateGhostHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateGhostHearts()
    {
        for (int i = 0; i < ghostHearts.Length; i++)
        {
            ghostHearts[i].enabled = (i < currentHealth);
        }
    }

    void Die()
    {
        Debug.Log("Player has died.");
    }

    // -------------------------
    // ANIMATION HELPER
    // -------------------------
    void PlayAnimation(string animName)
    {
        if (!string.IsNullOrEmpty(animName))
        {
            animator.Play(animName);
        }
    }

    // -------------------------
    // FLIP
    // -------------------------
    public void Flip(bool faceRight)
    {
        facingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // -------------------------
    // DEBUG RAYS
    // -------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (leftFoot != null)
            Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.down * groundCheckDistance);

        if (rightFoot != null)
            Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.down * groundCheckDistance);
    }
}
