using UnityEngine;
using UnityEngine.UI;

public class IfaEscortController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public string walkAnimName;          // You type walk animation name
    public string idleAnimName;          // Optional idle anim
    public Animator animator;

    [Header("Gun / Halt")]
    public string drawGunAnimName;       // You type draw animation name
    public string holsterGunAnimName;    // You type holster animation name
    public GameObject armObject;         // Separate arm+gun object
    public Transform armPivot;           // Pivot to rotate
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireCooldown = 0.5f;
    public float gunRange = 15f;

    [Header("Health")]
    public int maxHealth = 10;
    public Slider healthBar;             // UI at top of screen
    public AudioSource audioSource;
    public AudioClip shieldHitSound;     // Optional: hit sound on Ifa
    public AudioClip deathSound;

    [Header("Enemy Detection")]
    public string enemyTag = "Enemy";
    public float enemyAggroRadius = 20f; // For slimes to use if needed

    private int currentHealth;
    private bool escortActive = false;
    private bool halted = false;
    private bool usingGun = false;
    private bool reachedEnd = false;
    private float fireTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (armObject != null)
            armObject.SetActive(false);
    }

    void Update()
    {
        if (!escortActive || reachedEnd)
            return;

        HandleHaltToggle();
        HandleMovement();
        HandleGunLogic();
    }

    // Called by trigger
    public void StartEscort()
    {
        escortActive = true;
        halted = false;
        usingGun = false;

        if (animator != null && !string.IsNullOrEmpty(walkAnimName))
            animator.Play(walkAnimName);
    }

    // Called by end trigger
    public void StopEscort()
    {
        escortActive = false;
        reachedEnd = true;

        if (animator != null && !string.IsNullOrEmpty(idleAnimName))
            animator.Play(idleAnimName);
    }

    void HandleHaltToggle()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!halted)
                HaltAndDrawGun();
            else
                ResumeAndHolster();
        }
    }

    void HaltAndDrawGun()
    {
        halted = true;

        if (animator != null && !string.IsNullOrEmpty(drawGunAnimName))
            animator.Play(drawGunAnimName);

        usingGun = true;

        if (armObject != null)
            armObject.SetActive(true);
    }

    void ResumeAndHolster()
    {
        halted = false;

        if (animator != null && !string.IsNullOrEmpty(holsterGunAnimName))
            animator.Play(holsterGunAnimName);

        usingGun = false;

        if (armObject != null)
            armObject.SetActive(false);

        if (animator != null && !string.IsNullOrEmpty(walkAnimName))
            animator.Play(walkAnimName);
    }

    void HandleMovement()
    {
        if (halted)
            return;

        // Simple "walk forward" along +X
        Vector2 pos = transform.position;
        pos.x += walkSpeed * Time.deltaTime;
        transform.position = pos;

        if (animator != null && !string.IsNullOrEmpty(walkAnimName))
            animator.Play(walkAnimName);
    }

    void HandleGunLogic()
    {
        if (!usingGun || armPivot == null || projectilePrefab == null)
            return;

        fireTimer -= Time.deltaTime;

        Transform target = FindClosestEnemyInRange(gunRange);
        if (target == null)
            return;

        // Aim arm at enemy
        Vector2 dir = (target.position - armPivot.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        armPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Fire
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
        escortActive = false;
        reachedEnd = true;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (animator != null && !string.IsNullOrEmpty(idleAnimName))
            animator.Play(idleAnimName);
    }

    // For enemies to know he's a valid target if you want:
    public float GetAggroRadius()
    {
        return enemyAggroRadius;
    }
}
