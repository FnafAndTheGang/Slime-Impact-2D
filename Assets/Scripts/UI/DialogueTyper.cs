using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueTyper : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI continuePrompt;
    public PlayerPortraitController portraitController;

    [Header("UI Root")]
    public GameObject dialogueUIRoot;

    [Header("Typing Settings")]
    [TextArea]
    public string fullText;
    public float typeSpeed = 0.05f;
    public AudioSource typingAudio;
    public AudioClip typingSound;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    [Header("Hide While Talking")]
    public List<GameObject> hideObjects = new List<GameObject>();

    public bool hasClosedOnce = false; // 🔥 Made public

    private bool isTyping = false;
    private bool isFinished = false;
    private Coroutine bobRoutine;

    public void StartTyping() // 🔥 Public trigger method
    {
        hasClosedOnce = false;
        dialogueText.text = "";
        continuePrompt.gameObject.SetActive(false);

        foreach (var obj in hideObjects)
            if (obj != null) obj.SetActive(false);

        dialogueCanvas.alpha = 1f;
        dialogueCanvas.blocksRaycasts = true;

        StartCoroutine(TypeText());
    }

    void Update()
    {
        if (isFinished && !hasClosedOnce && Input.GetMouseButtonDown(1))
        {
            hasClosedOnce = true;
            StartCoroutine(FadeOutDialogue());
        }
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        portraitController.StartTalking();

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            if (typingAudio != null && typingSound != null)
                typingAudio.PlayOneShot(typingSound);

            yield return new WaitForSeconds(typeSpeed);
        }

        if (typingAudio != null)
            typingAudio.Stop();

        isTyping = false;
        isFinished = true;
        portraitController.StopTalking();

        continuePrompt.gameObject.SetActive(true);
        bobRoutine = StartCoroutine(BobContinuePrompt());
    }

    IEnumerator FadeOutDialogue()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            dialogueCanvas.alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        dialogueCanvas.alpha = 0f;
        dialogueCanvas.blocksRaycasts = false;

        foreach (var obj in hideObjects)
            if (obj != null) obj.SetActive(true);

        portraitController.OnDialogueClosed();

        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        if (dialogueUIRoot != null)
            Destroy(dialogueUIRoot);

        Destroy(gameObject);
    }

    IEnumerator BobContinuePrompt()
    {
        Vector3 basePos = continuePrompt.rectTransform.localPosition;
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime * 2f;
            float offset = Mathf.Sin(timer) * 5f;

            continuePrompt.rectTransform.localPosition =
                basePos + new Vector3(0, offset, 0);

            yield return null;
        }
    }
}
