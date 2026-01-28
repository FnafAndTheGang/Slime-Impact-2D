using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IfaEscortController : MonoBehaviour
{
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
    public AudioSource audioSource;
    public AudioClip shieldHitSound;
    public AudioClip deathSound;

    [Header("Enemy Detection")]
    public string enemyTag = "Enemy";
    public float enemyAggroRadius = 20f;

    [Header("Status UI")]
    public Image statusImage;
    public Sprite haltedSprite;
    public Sprite walkingSprite;

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

        UpdateStatusSprite();
    }

    void Update()
    {
        if (!escortActive || reachedEnd)
            return;

        HandleHaltToggle();
        HandleMovement();
        HandleGunLogic();
    }

    // Called by EscortIntroTrigger
    public void StartEscort()
    {
        escortActive = true;
        halted = false;
        usingGun = false;

        PlayWalkAnimation();
        UpdateStatusSprite();
    }

    public void StopEscort()
    {
        escortActive = false;
        reachedEnd = true;

        if (!string.IsNullOrEmpty(idleAnimName))
            animator.CrossFade(idleAnimName, 0.1f);
    }

    void HandleHaltToggle()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!halted)
                HaltAndDrawGun();
            else
                StartCoroutine(HolsterThenResume());
        }
    }

    void HaltAndDrawGun()
    {
        halted = true;
        usingGun = true;

        StartCoroutine(DrawGunRoutine());
        UpdateStatusSprite();
    }

    IEnumerator DrawGunRoutine()
    {
        animator.CrossFade(drawGunAnimName, 0.1f);

        // Wait one frame so animator switches states
        yield return null;

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        if (armObject != null)
            armObject.SetActive(true);
    }

    IEnumerator HolsterThenResume()
    {
        halted = true;
        usingGun = false;

        animator.CrossFade(holsterGunAnimName, 0.1f);

        // Arm disappears immediately
        if (armObject != null)
            armObject.SetActive(false);

        // Wait one frame so animator switches states
        yield return null;

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        halted = false;
        PlayWalkAnimation();
        UpdateStatusSprite();
    }

    void HandleMovement()
    {
        if (halted)
            return;

        transform.position += Vector3.right * walkSpeed * Time.deltaTime;
    }

    void PlayWalkAnimation()
    {
        if (!string.IsNullOrEmpty(walkAnimName))
            animator.CrossFade(walkAnimName, 0.1f);
    }

    void HandleGunLogic()
    {
        if (!usingGun || armPivot == null || projectilePrefab == null)
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

        if (!string.IsNullOrEmpty(idleAnimName))
            animator.CrossFade(idleAnimName, 0.1f);
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

    // Called by climb zones
    public void ApplyClimb(float amount)
    {
        if (!escortActive || halted)
            return;

        Vector3 pos = transform.position;
        pos.y += amount;
        transform.position = pos;
    }
}
