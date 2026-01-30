using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IfaEscortController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject hitboxObject;
    private Collider2D hitboxCollider;

    [Header("Sprites")]
    public Sprite defaultIdleSprite;

    [Header("H Key Sound")]
    public AudioClip haltToggleSound;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public string walkAnimName;
    public string idleAnimName;
    public Animator animator;

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
    public bool isDead = false;

    [Header("Status UI")]
    public Image statusImage;
    public Sprite haltedSprite;
    public Sprite walkingSprite;

    [Header("Climb Areas")]
    public List<GameObject> climbAreas = new List<GameObject>();
    public float climbHeight = 0.5f;

    [Header("Dialogue Trigger")]
    public GameObject dialogueTyperObject; // Assign your IfaDialogue object here

    public bool escortActive = false;
    private bool halted = false;
    private bool reachedEnd = false;

    private int currentHealth;
    private float hCooldown = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        sr = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        if (hitboxObject != null)
            hitboxCollider = hitboxObject.GetComponent<Collider2D>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        UpdateStatusSprite();
    }

    void Update()
    {
        if (hCooldown > 0)
            hCooldown -= Time.deltaTime;

        if (!escortActive || reachedEnd || isDead)
            return;

        HandleHaltToggle();
        HandleMovement();
    }

    public void StartEscort()
    {
        escortActive = true;
        halted = true;
        isDead = false;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        UpdateStatusSprite();
    }

    void HandleHaltToggle()
    {
        if (isDead || !escortActive)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (hCooldown > 0)
                return;

            hCooldown = 1f;

            if (audioSource != null && haltToggleSound != null)
                audioSource.PlayOneShot(haltToggleSound);

            halted = !halted;

            if (halted)
            {
                animator.enabled = false;
                sr.sprite = defaultIdleSprite;
            }
            else
            {
                animator.enabled = true;
                animator.CrossFade(walkAnimName, 0.1f);
            }

            UpdateStatusSprite();
        }
    }

    void HandleMovement()
    {
        if (halted || isDead)
            return;

        transform.position += Vector3.right * walkSpeed * Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        if (!escortActive)
            return; // Safe after reaching destination

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
        halted = true;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        animator.enabled = false;
        sr.sprite = deathSprite;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // Kill player ONLY if escort is active
        PlayerController2D p = FindObjectOfType<PlayerController2D>();
        if (p != null && escortActive)
            p.ForceKill();

        escortActive = false;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        transform.position = startPosition;

        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.value = currentHealth;

        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        isDead = false;
        escortActive = true;
        halted = true;

        UpdateStatusSprite();
    }

    public void ForceKill()
    {
        if (!isDead)
            Die();
    }

    // ⭐ Called by the trigger when Ifa reaches the end
    public void ReachDestination()
    {
        reachedEnd = true;
        escortActive = false;
        halted = true;
        isDead = false;

        animator.enabled = false;
        sr.sprite = defaultIdleSprite;

        if (healthBarRoot != null)
            healthBarRoot.SetActive(false);

        if (hitboxCollider != null)
            hitboxCollider.enabled = false;

        if (statusImage != null)
            statusImage.enabled = false;

        // ⭐ Start dialogue typing properly
        if (dialogueTyperObject != null)
        {
            dialogueTyperObject.SetActive(true);

            DialogueTyper typer = dialogueTyperObject.GetComponent<DialogueTyper>();
            if (typer != null)
                typer.StartTyping();
        }
    }

    void UpdateStatusSprite()
    {
        if (statusImage == null)
            return;

        if (!escortActive)
        {
            statusImage.enabled = false;
            return;
        }

        statusImage.enabled = true;
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
