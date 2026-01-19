using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RedRiotBoss : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public AudioSource audioSource;
    public SpriteRenderer spriteRenderer;

    [Header("Health")]
    public int maxHealth = 40;
    private int currentHealth;

    [Header("Health Bar (Slider)")]
    public GameObject healthBarObject;
    public Slider healthSlider;

    [Header("Animation Names")]
    public string idleAnimName = "RedRiotIdle";
    public string attackAnimName = "RedRiotAttack";
    public string deathAnimName = "RedRiotDeath";

    [Header("Damage Flash")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.15f;
    private Color originalColor;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float timeBetweenShots = 1.5f;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public Transform bombFirePoint;
    public float bombForce = 8f;
    public int shotsBeforeBomb = 10;

    [Header("Teleport System")]
    public GameObject portalPrefab;
    public Transform[] teleportPoints;
    public float teleportDelay = 0.2f;

    [Tooltip("Offset for portal spawn position")]
    public Vector2 portalOffset = new Vector2(0f, -0.5f);

    [Header("Drop On Death")]
    public GameObject deathDropPrefab;

    [Header("Audio Clips")]
    public AudioClip bulletFireClip;
    public AudioClip bombFireClip;
    public AudioClip teleportOpenClip;
    public AudioClip teleportCloseClip;
    public AudioClip deathClip;

    private bool fightStarted = false;
    private bool canAttack = false;
    private bool isTeleporting = false;
    private bool isDead = false;

    private int shotCounter = 0;
    private int currentTeleportIndex = -1;

    private Vector3 originalPosition;

    void Start()
    {
        currentHealth = maxHealth;
        originalPosition = transform.position;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        animator.Play(idleAnimName);

        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;

        if (healthBarObject != null)
            healthBarObject.SetActive(false);

        PlayerController2D.PlayerDiedEvent += ResetBoss;
    }

    void OnDestroy()
    {
        PlayerController2D.PlayerDiedEvent -= ResetBoss;
    }

    void Update()
    {
        // No cooldown logic needed anymore
    }

    public void StartBoss()
    {
        if (isDead)
            return;

        fightStarted = true;
        canAttack = true;

        if (healthBarObject != null)
            healthBarObject.SetActive(true);

        UpdateHealthBar();

        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (fightStarted && !isDead)
        {
            if (canAttack && !isTeleporting)
                animator.Play(attackAnimName);

            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    public void FireShot()
    {
        if (!fightStarted || isTeleporting || isDead)
            return;

        shotCounter++;

        if (shotCounter >= shotsBeforeBomb)
        {
            shotCounter = 0;
            FireBomb();
        }
        else
        {
            FireBullet();
        }
    }

    void FireBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        RedRiotBullet bullet = bulletObj.GetComponent<RedRiotBullet>();
        if (bullet != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            bullet.Init(dir * bulletSpeed);
        }

        PlaySound(bulletFireClip);
    }

    void FireBomb()
    {
        GameObject bombObj = Instantiate(bombPrefab, bombFirePoint.position, Quaternion.identity);

        RedRiotBomb bomb = bombObj.GetComponent<RedRiotBomb>();
        if (bomb != null)
        {
            Vector2 dir = (player.position - bombFirePoint.position).normalized;
            Vector2 launch = new Vector2(dir.x, 1f).normalized * bombForce;
            bomb.Init(launch);
        }

        PlaySound(bombFireClip);
    }

    public void TakeHit()
    {
        if (isDead || isTeleporting)
            return;

        StartCoroutine(FlashRed());

        StartTeleport();

        currentHealth--;
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    // -------------------------------
    // SIMPLE TELEPORT SYSTEM
    // -------------------------------
    void StartTeleport()
    {
        isTeleporting = true;
        canAttack = false;

        // Spawn portal at current position
        Vector3 spawnPos = transform.position + (Vector3)portalOffset;
        GameObject portal = Instantiate(portalPrefab, spawnPos, Quaternion.identity);

        PortalController pc = portal.GetComponent<PortalController>();
        pc.Open(teleportOpenClip, this);
    }

    // Called by PortalController when open animation finishes
    public void OnPortalOpened(PortalController portal)
    {
        TeleportToRandomPoint();

        // Spawn close portal at new position
        Vector3 spawnPos = transform.position + (Vector3)portalOffset;
        GameObject closePortal = Instantiate(portalPrefab, spawnPos, Quaternion.identity);

        PortalController pc = closePortal.GetComponent<PortalController>();
        pc.Close(teleportCloseClip, this);

        Destroy(portal.gameObject);
    }

    void TeleportToRandomPoint()
    {
        int newIndex = currentTeleportIndex;

        if (teleportPoints.Length > 1)
        {
            while (newIndex == currentTeleportIndex)
                newIndex = Random.Range(0, teleportPoints.Length);
        }
        else
        {
            newIndex = 0;
        }

        currentTeleportIndex = newIndex;
        transform.position = teleportPoints[newIndex].position;
    }

    // Called by PortalController when close animation finishes
    public void OnPortalClosed()
    {
        isTeleporting = false;
        canAttack = true;

        animator.Play(idleAnimName);
    }

    // -------------------------------

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        canAttack = false;
        isTeleporting = false;

        PlaySound(deathClip);
        animator.Play(deathAnimName);

        if (healthBarObject != null)
            healthBarObject.SetActive(false);
    }

    public void OnDeathAnimationFinished()
    {
        if (deathDropPrefab != null)
            Instantiate(deathDropPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void ResetBoss()
    {
        fightStarted = false;
        canAttack = false;
        isTeleporting = false;
        isDead = false;

        currentHealth = maxHealth;
        shotCounter = 0;

        transform.position = originalPosition;

        animator.Play(idleAnimName);

        if (healthBarObject != null)
            healthBarObject.SetActive(false);

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
