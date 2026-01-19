using UnityEngine;
using System.Collections;

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

        PlayerController2D.PlayerDiedEvent += ResetBoss;
    }

    void OnDestroy()
    {
        PlayerController2D.PlayerDiedEvent -= ResetBoss;
    }

    // Called by trigger
    public void StartBoss()
    {
        if (isDead)
            return;

        fightStarted = true;
        canAttack = true;

        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (fightStarted && !isDead)
        {
            if (canAttack && !isTeleporting)
            {
                animator.Play(attackAnimName);
            }

            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    // Animation event at end of RedRiotAttack
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
        if (isDead)
            return;

        StartCoroutine(FlashRed());

        if (!isTeleporting)
            StartTeleport();

        currentHealth--;

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

    // TELEPORT SYSTEM
    void StartTeleport()
    {
        isTeleporting = true;
        canAttack = false;

        GameObject portal = Instantiate(portalPrefab, transform.position, Quaternion.identity);

        PortalController pc = portal.GetComponent<PortalController>();
        pc.Open(teleportOpenClip, this);
    }

    public void OnPortalOpened(PortalController portal)
    {
        StartCoroutine(TeleportRoutine(portal));
    }

    IEnumerator TeleportRoutine(PortalController portal)
    {
        yield return new WaitForSeconds(teleportDelay);

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

        GameObject closePortal = Instantiate(portalPrefab, transform.position, Quaternion.identity);
        PortalController pc = closePortal.GetComponent<PortalController>();
        pc.Close(teleportCloseClip, this);

        Destroy(portal.gameObject);
    }

    public void OnPortalClosed()
    {
        isTeleporting = false;
        canAttack = true;

        animator.Play(idleAnimName);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        canAttack = false;
        isTeleporting = false;

        PlaySound(deathClip);
        animator.Play(deathAnimName);
    }

    public void OnDeathAnimationFinished()
    {
        if (deathDropPrefab != null)
            Instantiate(deathDropPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // RESET WHEN PLAYER DIES
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
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
