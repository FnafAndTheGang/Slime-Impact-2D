using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPortraitController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite defaultPortrait;
    public Sprite damagePortrait;
    // Future: public Sprite killPortrait;

    [Header("UI Reference")]
    public Image portraitImage; // Assign your UI Image here

    [Header("Settings")]
    public float damageDisplayTime = 3f;

    private Coroutine damageRoutine;

    void Start()
    {
        // Set default portrait on start
        portraitImage.sprite = defaultPortrait;
    }

    // Call this from the player when they take damage
    public void OnPlayerDamaged()
    {
        // If already showing damage portrait, restart timer
        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamagePortraitTimer());
    }

    private IEnumerator DamagePortraitTimer()
    {
        portraitImage.sprite = damagePortrait;

        yield return new WaitForSeconds(damageDisplayTime);

        portraitImage.sprite = defaultPortrait;
        damageRoutine = null;
    }

    // Future expansion:
    // public void OnPlayerKillEnemy() { ... }
}
