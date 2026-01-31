using UnityEngine;
using TMPro;
using System.Collections;

public class WaitForIfaTriggerTMP : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    [Header("Ifa Reference")]
    public IfaEscortController ifaController;

    private bool dialogueStarted = false;
    private bool dialogueFinished = false;
    private bool showingMessage = false;

    void Start()
    {
        // Make sure the text is hidden at game start
        if (messageText != null)
            messageText.enabled = false;
    }

    private void Update()
    {
        if (ifaController == null)
            return;

        // Detect when dialogue STARTS
        if (!dialogueStarted &&
            ifaController.dialogueTyperObject != null &&
            ifaController.dialogueTyperObject.activeInHierarchy)
        {
            dialogueStarted = true;
        }

        // Detect when dialogue FINISHES (only after it has started)
        if (dialogueStarted &&
            (ifaController.dialogueTyperObject == null ||
             !ifaController.dialogueTyperObject.activeInHierarchy))
        {
            dialogueFinished = true;
            Destroy(gameObject); // Remove trigger permanently
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dialogueFinished)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (!showingMessage)
            StartCoroutine(ShowMessageRoutine());
    }

    IEnumerator ShowMessageRoutine()
    {
        showingMessage = true;

        if (messageText != null)
        {
            messageText.text = "You must wait for Ifa";
            messageText.enabled = true;
        }

        yield return new WaitForSeconds(messageDuration);

        if (messageText != null)
            messageText.enabled = false;

        showingMessage = false;
    }
}
