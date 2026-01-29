using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IfaEscortController : MonoBehaviour
{
    [Header("Player Reference (ignored for collisions)")]
    public Transform player;
    private Collider2D[] ifaColliders;
    private Collider2D[] playerColliders;

    [Header("Sprites")]
    public Sprite defaultIdleSprite;   // ⭐ NEW — static sprite for idle

    [Header("H Key Sound")]
    public AudioClip haltToggleSound;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public string walkAnimName;
    public string idleAnimName;
    public Animator animator;

    [Header("Gun / Halt")]
    public string drawGunAnimName;
    public string holsterGunAnimName;
    public GameObject armObject;
    public Transform armPivot;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireCooldown = 0.5f;
    public float gunRange = 15f;

    [Header("Health")]
    public int maxHealth = 10;
    public Slider healthBar;
    public GameObject healthBarRoot;
    public AudioSource audioSource;
    public AudioClip shieldHitSound;
    public AudioClip deathSound;

    [Header("Death")]
    public Sprite deathSprite;
    private SpriteRenderer sr;
    private Vector3 startPosition;
    private bool isDead = false;

    [Header("Enemy Detection")]
    public string enemyTag = "Enemy";
    public float enemyAggroRadius = 20f;

    [Header("Status UI")]
    public Image statusImage;
    public Sprite haltedSprite;
    public Sprite walkingSprite;

    [Header("Climb Areas")]
    public List<GameObject> climbAreas = new List<GameObject>();
    public float climbHeight = 0.5f;

    private int currentHealth;
    private bool escortActive = false;
    private bool halted = false;
    private bool usingGun = false;
    private bool reachedEnd = false;
    private float fireTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        ifaColliders = GetComponentsInChildren<Collider2D>();
        if (player != null)
            playerColliders = player.GetComponentsInChildren<Collider2D>();

        IgnorePlayerCollisions();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        if (armObject != null)
            armObject.SetActive(false);

        // ⭐ Start with animator OFF and default sprite
        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        UpdateStatusSprite();
    }

    void IgnorePlayerCollisions()
    {
        if (playerColliders == null || ifaColliders == null)
            return;

        foreach (var ifaCol in ifaColliders)
            foreach (var pCol in playerColliders)
                Physics2D.IgnoreCollision(ifaCol, pCol, true);
    }

    void Update()
    {
        if (!escortActive || reachedEnd || isDead)
            return;

        HandleHaltToggle();
        HandleMovement();
        HandleGunLogic();
    }

    public void StartEscort()
    {
        escortActive = true;
        halted = true;        // ⭐ Start halted
        usingGun = false;
        isDead = false;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        // ⭐ Start with animator OFF and default sprite
        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        UpdateStatusSprite();
    }

    void HandleHaltToggle()
    {
        if (isDead)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (audioSource != null && haltToggleSound != null)
                audioSource.PlayOneShot(haltToggleSound);

            if (halted)
                ResumeWalking();
            else
                HaltAndDrawGun();
        }
    }

    void ResumeWalking()
    {
        halted = false;
        usingGun = false;

        animator.enabled = true; // ⭐ Turn animator back on
        animator.CrossFade(walkAnimName, 0.1f);

        UpdateStatusSprite();
    }

    void HaltAndDrawGun()
    {
        halted = true;
        usingGun = true;

        animator.enabled = true;
        StartCoroutine(DrawGunRoutine());

        UpdateStatusSprite();
    }

    IEnumerator DrawGunRoutine()
    {
        animator.CrossFade(drawGunAnimName, 0.1f);
        yield return null;

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        if (armObject != null)
            armObject.SetActive(true);
    }

    void HandleMovement()
    {
        if (halted || isDead)
            return;

        transform.position += Vector3.right * walkSpeed * Time.deltaTime;
    }

    void HandleGunLogic()
    {
        if (!usingGun || armPivot == null || projectilePrefab == null || isDead)
            return;

        fireTimer -= Time.deltaTime;

        Transform target = FindClosestEnemyInRange(gunRange);
        if (target == null)
            return;

        Vector2 dir = (target.position - armPivot.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        armPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (fireTimer <= 0f)
        {
            fireTimer = fireCooldown;
            ShootProjectile(dir);
        }
    }

    Transform FindClosestEnemyInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        Vector2 pos = transform.position;

        foreach (GameObject e in enemies)
        {
            float d = Vector2.Distance(pos, e.transform.position);
            if (d < range && d < closestDist)
            {
                closestDist = d;
                closest = e.transform;
            }
        }

        return closest;
    }

    void ShootProjectile(Vector2 dir)
    {
        GameObject proj = Instantiate(projectilePrefab, armPivot.position, Quaternion.identity);
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = dir * projectileSpeed;
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (audioSource != null && shieldHitSound != null)
            audioSource.PlayOneShot(shieldHitSound);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        escortActive = false;
        halted = true;
        usingGun = false;

        if (armObject != null)
            armObject.SetActive(false);

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        animator.enabled = false; // ⭐ Stop all animations
        sr.sprite = deathSprite;  // ⭐ Show death sprite

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        transform.position = startPosition;

        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = currentHealth;

        // ⭐ Start with animator OFF and default sprite
        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        isDead = false;
        escortActive = true;
        halted = true;
        usingGun = false;

        UpdateStatusSprite();

        PlayerController2D playerObj = FindObjectOfType<PlayerController2D>();
        if (playerObj != null)
        {
            Vector3 checkpointPos = CheckpointManager.instance.GetLastCheckpointPosition();
            playerObj.transform.position = checkpointPos;

            playerObj.currentHealth = playerObj.maxHealth;
            playerObj.UpdateGhostHearts();
        }
    }

    public float GetAggroRadius()
    {
        return enemyAggroRadius;
    }

    void UpdateStatusSprite()
    {
        if (statusImage == null)
            return;

        statusImage.sprite = halted ? haltedSprite : walkingSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!escortActive || halted || isDead)
            return;

        foreach (GameObject area in climbAreas)
        {
            if (other.gameObject == area)
            {
                transform.position += new Vector3(0f, climbHeight, 0f);
                break;
            }
        }
    }

    public void ApplyClimb(float amount)
    {
        if (!escortActive || halted || isDead)
            return;

        transform.position += new Vector3(0f, amount, 0f);
    }
}
