using UnityEngine;
using UnityEngine.UI;

public class GhostBomb : MonoBehaviour
{
    [Header("Ghost Bomb Settings")]
    public GameObject ghostProjectilePrefab;
    public Transform firePoint;
    public float cooldown = 20f;

    [Header("UI")]
    public Image abilityIcon;
    public Sprite readySprite;
    public Sprite cooldownSprite;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

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
        // Determine direction based on player facing
        float dir = transform.localScale.x > 0 ? 1f : -1f;

        GameObject ghost = Instantiate(ghostProjectilePrefab, firePoint.position, Quaternion.identity);

        // FIXED: pass the player as the source
        ghost.GetComponent<GhostProjectile>().Init(dir, this.gameObject);

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
