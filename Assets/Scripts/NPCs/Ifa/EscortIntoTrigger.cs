using UnityEngine;
using System.Collections;

public class EscortIntroTrigger : MonoBehaviour
{
    [Header("References")]
    public IfaEscortController ifa;
    public GameObject promptImageRoot;
    public GameObject rmbPrompt;
    public AudioSource audioSource;
    public AudioClip promptSound;

    [Header("Timing")]
    public float initialDelay = 5f;   // Time before RMB prompt appears
    public float promptDelay = 1f;    // Delay before showing prompt

    private bool started = false;

    void Start()
    {
        if (promptImageRoot != null)
            promptImageRoot.SetActive(false);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(false);

        // ⭐ Hide status UI until prompt is done
        if (ifa != null && ifa.statusImage != null)
            ifa.statusImage.gameObject.SetActive(false);
    }

    public void BeginEscortSequence()
    {
        if (started)
            return;

        started = true;
        StartCoroutine(ShowPromptAndStart());
    }

    IEnumerator ShowPromptAndStart()
    {
        // ⭐ Delay before showing prompt
        yield return new WaitForSeconds(promptDelay);

        if (promptImageRoot != null)
            promptImageRoot.SetActive(true);

        if (audioSource != null && promptSound != null)
            audioSource.PlayOneShot(promptSound);

        // Wait before showing RMB prompt
        yield return new WaitForSeconds(initialDelay);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(true);

        // Wait for RMB click
        while (!Input.GetMouseButtonDown(1))
            yield return null;

        // Hide UI
        if (promptImageRoot != null)
            promptImageRoot.SetActive(false);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(false);

        // ⭐ NOW show the bottom status UI
        if (ifa != null && ifa.statusImage != null)
            ifa.statusImage.gameObject.SetActive(true);

        // Start escort
        if (ifa != null)
            ifa.StartEscort();
    }
}
