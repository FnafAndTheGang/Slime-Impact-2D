using UnityEngine;

public class MavuikaInteraction : MonoBehaviour
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
    public string nextMissionText = "Go to Nod-Krai";

    [Header("Post-Dialogue Actions")]
    public GameObject tilemapToDisable;
    public EscortIntroTrigger escortTrigger; // ⭐ DIRECT REFERENCE

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

        if (hasTalkedOnce)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

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

        sr.flipX = player.position.x > transform.position.x;
    }

    void StartDialogue()
    {
        dialogueActive = true;
        pressFIcon.SetActive(false);

        dialogueUIRoot.SetActive(true);

        dialogueTyper.fullText = dialogueText;
        dialogueTyper.StartTyping();

        StartCoroutine(WaitForDialogueToClose());
    }

    System.Collections.IEnumerator WaitForDialogueToClose()
    {
        // Wait until dialogueTyper says it's closed
        while (!dialogueTyper.hasClosedOnce)
            yield return null;

        dialogueActive = false;
        hasTalkedOnce = true;

        npcCollider.enabled = false;

        MissionObjectiveUI.instance.SetObjective(nextMissionText);

        if (tilemapToDisable != null)
            tilemapToDisable.SetActive(false);

        // ⭐ START ESCORT PROMPT IMMEDIATELY
        if (escortTrigger != null)
            escortTrigger.BeginEscortSequence();
    }
}
