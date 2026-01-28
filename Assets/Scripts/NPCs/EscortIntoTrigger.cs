using UnityEngine;

public class EscortIntroTrigger : MonoBehaviour
{
    [Header("References")]
    public IfaEscortController ifa;      // Assign Ifa here
    public GameObject promptImageRoot;   // UI image explaining section
    public GameObject rmbPrompt;         // "RMB to continue" UI
    public AudioSource audioSource;
    public AudioClip promptSound;

    public float initialDelay = 5f;      // Time before RMB prompt appears

    private bool triggered = false;

    void Start()
    {
        if (promptImageRoot != null)
            promptImageRoot.SetActive(false);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        StartCoroutine(ShowPromptAndStart());
    }

    System.Collections.IEnumerator ShowPromptAndStart()
    {
        if (promptImageRoot != null)
            promptImageRoot.SetActive(true);

        if (audioSource != null && promptSound != null)
            audioSource.PlayOneShot(promptSound);

        // Wait before allowing RMB
        yield return new WaitForSeconds(initialDelay);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(true);

        // Wait for RMB
        while (!Input.GetMouseButtonDown(1))
            yield return null;

        if (promptImageRoot != null)
            promptImageRoot.SetActive(false);

        if (rmbPrompt != null)
            rmbPrompt.SetActive(false);

        if (ifa != null)
            ifa.StartEscort();

        // This trigger is done
        gameObject.SetActive(false);
    }
}
