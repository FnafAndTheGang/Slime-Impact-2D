using UnityEngine;
using UnityEngine.UI;

public class GhostBomb : MonoBehaviour
{
    [Header("Ghost Bomb Settings")]
    public GameObject ghostProjectilePrefab;
    public Transform firePoint;
    public float cooldown = 20f;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioSource audioSource;

    [Header("UI")]
    public Image abilityIcon;
    public Sprite readySprite;
    public Sprite cooldownSprite;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    // Reference to player controller for facing direction
    private PlayerController2D player;

    void Start()
    {
        player = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        if (!isOnCooldown && Input.GetKeyDown(KeyCode.E))
            FireGhost();

        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                ResetCooldown();
        }
    }

    void FireGhost()
    {
        if (player == null)
            return;

        // Use the REAL facing direction from PlayerController2D
        float dir = player.FacingRight ? 1f : -1f;

        GameObject ghost = Instantiate(ghostProjectilePrefab, firePoint.position, Quaternion.identity);

        // Pass direction + player as source
        ghost.GetComponent<GhostProjectile>().Init(dir, this.gameObject);

        // Play fire sound
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        StartCooldown();
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldown;

        if (abilityIcon != null && cooldownSprite != null)
            abilityIcon.sprite = cooldownSprite;
    }

    void ResetCooldown()
    {
        isOnCooldown = false;

        if (abilityIcon != null && readySprite != null)
            abilityIcon.sprite = readySprite;
    }
}
