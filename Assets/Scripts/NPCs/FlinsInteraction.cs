using UnityEngine;

public class FlinsInteraction : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float interactRadius = 3f;

    [Header("UI Elements")]
    public GameObject pressFIcon;
    public GameObject dialogueUIRoot;
    public DialogueTyper dialogueTyper;

    [Header("Dialogue")]
    [TextArea]
    public string dialogueText;

    [Header("Mission")]
    public string missionText = "Find Flins";       // The mission BEFORE talking
    public string nextMissionText = "Go to Nod-Krai"; // The mission AFTER talking

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private bool hasTalkedOnce = false;

    private BoxCollider2D npcCollider;
    private SpriteRenderer sr;

    void Start()
    {
        npcCollider = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (pressFIcon != null)
            pressFIcon.SetActive(false);

        if (dialogueUIRoot != null)
            dialogueUIRoot.SetActive(false);
    }

    void Update()
    {
        FacePlayer();

        float distance = Vector2.Distance(transform.position, player.position);

        if (hasTalkedOnce)
            return;

        if (distance <= interactRadius && !dialogueActive)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                pressFIcon.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.F))
                StartDialogue();
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                pressFIcon.SetActive(false);
            }
        }
    }

    void FacePlayer()
    {
        if (player == null || sr == null)
            return;

        // Face toward the player
        sr.flipX = player.position.x > transform.position.x;
    }

    void StartDialogue()
    {
        dialogueActive = true;
        pressFIcon.SetActive(false);

        if (dialogueUIRoot != null)
            dialogueUIRoot.SetActive(true);

        dialogueTyper.fullText = dialogueText;
        dialogueTyper.StartTyping();

        StartCoroutine(WaitForDialogueToClose());
    }

    System.Collections.IEnumerator WaitForDialogueToClose()
    {
        while (dialogueTyper != null && !dialogueTyper.hasClosedOnce)
            yield return null;

        dialogueActive = false;
        hasTalkedOnce = true;

        // Disable collider so player can walk through Flins
        if (npcCollider != null)
            npcCollider.enabled = false;

        // Replace "Find Flins" with "Go to Nod-Krai"
        MissionObjectiveUI.instance.SetObjective(nextMissionText);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
