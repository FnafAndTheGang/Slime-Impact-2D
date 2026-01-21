using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPortraitController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite defaultPortrait;     // Normal, no dialogue, no damage/kill
    public Sprite idlePortrait;        // Dialogue active, not talking
    public Sprite talkingPortrait;     // Dialogue active, talking
    public Sprite damagePortrait;      // When player takes damage
    public Sprite killPortrait;        // When player kills an enemy

    [Header("UI Reference")]
    public Image portraitImage;

    [Header("Timers")]
    public float damageDisplayTime = 3f;
    public float killDisplayTime = 3f;

    private Coroutine damageRoutine;
    private Coroutine killRoutine;
    private Coroutine talkingRoutine;

    private bool isTalking = false;
    private bool isDialogueActive = false;

    void Start()
    {
        if (portraitImage != null && defaultPortrait != null)
            portraitImage.sprite = defaultPortrait;
    }

    // -----------------------------
    // DAMAGE LOGIC (lower priority)
    // -----------------------------
    public void OnPlayerDamaged()
    {
        // Kill face has priority, ignore damage if kill is active
        if (killRoutine != null)
            return;

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageTimer());
    }

    IEnumerator DamageTimer()
    {
        if (portraitImage != null && damagePortrait != null)
            portraitImage.sprite = damagePortrait;

        float t = damageDisplayTime;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        damageRoutine = null;
        RestorePortraitAfterStatus();
    }

    // -----------------------------
    // KILL LOGIC (highest priority)
    // -----------------------------
    public void OnPlayerKillEnemy()
    {
        // Kill overrides damage
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        if (killRoutine != null)
            StopCoroutine(killRoutine);

        killRoutine = StartCoroutine(KillTimer());
    }

    IEnumerator KillTimer()
    {
        if (portraitImage != null && killPortrait != null)
            portraitImage.sprite = killPortrait;

        float t = killDisplayTime;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        killRoutine = null;
        RestorePortraitAfterStatus();
    }

    // -----------------------------
    // TALKING / DIALOGUE LOGIC
    // -----------------------------
    public void StartTalking()
    {
        isDialogueActive = true;
        isTalking = true;

        // If kill or damage is active, don't override
        if (killRoutine != null || damageRoutine != null)
            return;

        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);

        talkingRoutine = StartCoroutine(TalkingAnimation());
    }

    public void StopTalking()
    {
        isTalking = false;

        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);

        // If kill or damage is active, don't override
        if (killRoutine != null || damageRoutine != null)
            return;

        // Dialogue still active → idle face
        if (isDialogueActive && idlePortrait != null)
            portraitImage.sprite = idlePortrait;
    }

    public void OnDialogueClosed()
    {
        isDialogueActive = false;
        isTalking = false;

        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);

        // If kill or damage is active, don't override
        if (killRoutine != null || damageRoutine != null)
            return;

        // No status → default
        if (portraitImage != null && defaultPortrait != null)
            portraitImage.sprite = defaultPortrait;
    }

    IEnumerator TalkingAnimation()
    {
        while (isTalking)
        {
            if (portraitImage != null && talkingPortrait != null)
                portraitImage.sprite = talkingPortrait;
            yield return new WaitForSeconds(0.1f);

            if (!isTalking)
                break;

            if (portraitImage != null && idlePortrait != null)
                portraitImage.sprite = idlePortrait;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // -----------------------------
    // STATE RESOLUTION
    // -----------------------------
    void RestorePortraitAfterStatus()
    {
        // Kill still active → keep kill
        if (killRoutine != null)
            return;

        // Damage still active → keep damage
        if (damageRoutine != null)
            return;

        // Dialogue active → talking or idle
        if (isDialogueActive)
        {
            if (isTalking && talkingPortrait != null)
                portraitImage.sprite = talkingPortrait;
            else if (idlePortrait != null)
                portraitImage.sprite = idlePortrait;
            return;
        }

        // Otherwise → default
        if (portraitImage != null && defaultPortrait != null)
            portraitImage.sprite = defaultPortrait;
    }

    [Header("Spear System")]
    public bool hasSpear = false;
    public GameObject spearObject; // assign in Inspector

}
