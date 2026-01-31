using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScientistBoss : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Moving,
        Laser,
        Missile,
        ChargeSpin,
        Spinning,
        Dead
    }

    [Header("Core References")]
    public PlayerController2D player;
    public CameraFollow cam;
    public Slider healthSlider;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public AudioSource audioSource;

    [Header("Room Bounds")]
    public Transform leftBound;
    public Transform rightBound;

    [Header("Spin Movement Points")]
    public Transform spinPointA;
    public Transform spinPointB;

    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Health")]
    public int maxHealth = 40;
    public Sprite deadSprite;

    [Header("Animation Names")]
    public string idleLeftAnim;
    public string idleRightAnim;

    public string walkLeftAnim;
    public string walkRightAnim;

    public string laserLeftAnim;
    public string laserRightAnim;

    public string missileLeftAnim;
    public string missileRightAnim;

    public string chargeLeftAnim;
    public string chargeRightAnim;

    public string spinLeftAnim;
    public string spinRightAnim;

    public string deathAnim;

    [Header("Laser Attack")]
    public Transform laserFirePointLeft;
    public Transform laserFirePointRight;
    public GameObject laserLeftPrefab;
    public GameObject laserRightPrefab;
    public float laserDistance = 10f;
    public float laserCooldown = 5f;
    public AudioClip laserChargeSfx;
    public AudioClip laserFireSfx;

    [Header("Missile Attack")]
    public Transform missileFirePointLeft;
    public Transform missileFirePointRight;
    public GameObject missilePrefab;
    public float missileCooldown = 20f;
    public AudioClip missileFireSfx;

    [Header("Spin Attack")]
    public float spinCooldown = 40f;
    public float spinSpeed = 12f;
    public float spinDuration = 10f;
    public AudioClip spinSfx;

    [Header("Spin Hitbox")]
    public GameObject spinHitbox;

    [Header("Death")]
    public AudioClip deathSfx;

    private BossState state = BossState.Idle;
    private int currentHealth;
    private float laserTimer;
    private float missileTimer;
    private float spinTimer;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isDead = false;

    private float spinTimeRemaining;
    private Transform currentSpinTarget;

    public bool IsSpinning => state == BossState.Spinning;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        if (spinHitbox != null)
            spinHitbox.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        laserTimer += Time.deltaTime;
        missileTimer += Time.deltaTime;
        spinTimer += Time.deltaTime;

        float dist = Mathf.Abs(player.transform.position.x - transform.position.x);

        switch (state)
        {
            case BossState.Idle:
            case BossState.Moving:
                HandleMovement(dist);
                HandleAttacks(dist);
                break;

            case BossState.Spinning:
                HandleSpinMovement();
                break;
        }
    }

    // ---------------------------------------------------------
    // MOVEMENT — NO KEEP DISTANCE, PANIC RUN WHEN CLOSE
    // ---------------------------------------------------------
    void HandleMovement(float dist)
    {
        if (isAttacking) return;

        state = BossState.Moving;

        float myX = transform.position.x;
        float playerX = player.transform.position.x;

        facingRight = playerX > myX;

        // NEW: If player gets within 5 units → sprint to the furthest bound
        if (dist < 5f)
        {
            float distToLeft = Mathf.Abs(myX - leftBound.position.x);
            float distToRight = Mathf.Abs(myX - rightBound.position.x);

            bool goRight = distToRight > distToLeft;
            float dir = goRight ? 1f : -1f;

            transform.Translate(Vector3.right * dir * 15f * Time.deltaTime);

            anim.Play(goRight ? walkRightAnim : walkLeftAnim);
            return;
        }

        // Otherwise idle
        anim.Play(facingRight ? idleRightAnim : idleLeftAnim);
    }

    // ---------------------------------------------------------
    // ATTACK LOGIC
    // ---------------------------------------------------------
    void HandleAttacks(float dist)
    {
        if (isAttacking) return;

        if (spinTimer >= spinCooldown)
        {
            StartCoroutine(DoSpin());
            return;
        }

        if (missileTimer >= missileCooldown)
        {
            StartCoroutine(DoMissile());
            return;
        }

        if (dist > laserDistance && laserTimer >= laserCooldown)
        {
            StartCoroutine(DoLaser());
            return;
        }
    }

    // ---------------------------------------------------------
    // LASER ATTACK
    // ---------------------------------------------------------
    IEnumerator DoLaser()
    {
        isAttacking = true;
        state = BossState.Laser;
        laserTimer = 0f;

        facingRight = player.transform.position.x > transform.position.x;

        if (laserChargeSfx != null)
            audioSource.PlayOneShot(laserChargeSfx);

        anim.Play(facingRight ? laserRightAnim : laserLeftAnim);

        yield return new WaitForSeconds(1.5f);

        isAttacking = false;
        state = BossState.Idle;
    }

    public void FireLaserRightEvent()
    {
        Instantiate(laserRightPrefab, laserFirePointRight.position, Quaternion.identity);
        if (laserFireSfx != null)
            audioSource.PlayOneShot(laserFireSfx);
    }

    public void FireLaserLeftEvent()
    {
        Instantiate(laserLeftPrefab, laserFirePointLeft.position, Quaternion.identity);
        if (laserFireSfx != null)
            audioSource.PlayOneShot(laserFireSfx);
    }

    // ---------------------------------------------------------
    // MISSILE ATTACK
    // ---------------------------------------------------------
    IEnumerator DoMissile()
    {
        isAttacking = true;
        state = BossState.Missile;
        missileTimer = 0f;

        facingRight = player.transform.position.x > transform.position.x;

        anim.Play(facingRight ? missileRightAnim : missileLeftAnim);

        yield return new WaitForSeconds(0.5f);

        Transform firePoint = facingRight ? missileFirePointRight : missileFirePointLeft;

        Instantiate(missilePrefab, firePoint.position, Quaternion.identity);

        if (missileFireSfx != null)
            audioSource.PlayOneShot(missileFireSfx);

        yield return new WaitForSeconds(0.7f);

        isAttacking = false;
        state = BossState.Idle;
    }

    // ---------------------------------------------------------
    // SPIN ATTACK (UNCHANGED — WORKING PERFECTLY)
    // ---------------------------------------------------------
    IEnumerator DoSpin()
    {
        isAttacking = true;
        state = BossState.ChargeSpin;
        spinTimer = 0f;

        facingRight = player.transform.position.x > transform.position.x;

        anim.Play(facingRight ? chargeRightAnim : chargeLeftAnim);

        yield return new WaitForSeconds(0.7f);

        state = BossState.Spinning;
        spinTimeRemaining = spinDuration;

        if (spinHitbox != null)
            spinHitbox.SetActive(true);

        if (spinSfx != null)
            audioSource.PlayOneShot(spinSfx);

        currentSpinTarget = (Mathf.Abs(transform.position.x - spinPointA.position.x) <
                             Mathf.Abs(transform.position.x - spinPointB.position.x))
                             ? spinPointA : spinPointB;
    }

    void HandleSpinMovement()
    {
        if (spinTimeRemaining > 0)
        {
            spinTimeRemaining -= Time.deltaTime;

            float dir = Mathf.Sign(currentSpinTarget.position.x - transform.position.x);
            facingRight = dir > 0;

            transform.Translate(Vector3.right * dir * spinSpeed * Time.deltaTime);

            anim.Play(facingRight ? spinRightAnim : spinLeftAnim);

            if (Mathf.Abs(transform.position.x - currentSpinTarget.position.x) < 0.2f)
            {
                currentSpinTarget = (currentSpinTarget == spinPointA) ? spinPointB : spinPointA;
            }
        }
        else
        {
            if (spinHitbox != null)
                spinHitbox.SetActive(false);

            isAttacking = false;
            state = BossState.Idle;
        }
    }

    // ---------------------------------------------------------
    // DAMAGE + DEATH
    // ---------------------------------------------------------
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(DoDeath());
        }
    }

    IEnumerator DoDeath()
    {
        isDead = true;
        isAttacking = false;
        state = BossState.Dead;

        if (spinHitbox != null)
            spinHitbox.SetActive(false);

        cam.SetTarget(transform);

        if (deathSfx != null)
            audioSource.PlayOneShot(deathSfx);

        anim.Play(deathAnim);

        yield return new WaitForSeconds(2f);

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        anim.enabled = false;
        spriteRenderer.sprite = deadSprite;

        yield return new WaitForSeconds(1f);

        cam.SetTarget(player.transform);

        this.enabled = false;
    }

    public void OnPlayerDied()
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + 10);

        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }
}
