using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;          // plays only when activated
    public string activateAnimName;    // animation to play once

    [Header("Sound")]
    public AudioSource audioSource;    // plays activation sound
    public AudioClip activateSound;    // sound to play when touched

    private bool isActivated = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Ensure animation is NOT playing on start
        if (animator != null)
            animator.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated)
            return;

        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isActivated = true;

        // Save checkpoint position
        CheckpointManager.instance.SetCheckpoint(transform.position);

        // Play activation sound
        if (audioSource != null && activateSound != null)
            audioSource.PlayOneShot(activateSound);

        // Play activation animation
        if (animator != null && !string.IsNullOrEmpty(activateAnimName))
        {
            animator.enabled = true;
            animator.Play(activateAnimName);
        }

        // Hide after animation finishes
        StartCoroutine(HideAfterAnimation());
    }

    System.Collections.IEnumerator HideAfterAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(info.length);
        }

        // Hide sprite + animator permanently
        if (sr != null)
            sr.enabled = false;

        if (animator != null)
            animator.enabled = false;

        // Disable collider so it can't be triggered again
        GetComponent<Collider2D>().enabled = false;
    }
}
