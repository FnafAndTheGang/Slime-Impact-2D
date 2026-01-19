using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    private RedRiotBoss boss;
    private bool isClosing = false;

    public void Open(AudioClip openClip, RedRiotBoss owner)
    {
        boss = owner;

        if (animator != null)
            animator.Play("Portal_Open");

        if (audioSource != null && openClip != null)
            audioSource.PlayOneShot(openClip);
    }

    public void Close(AudioClip closeClip, RedRiotBoss owner)
    {
        boss = owner;
        isClosing = true;

        if (animator != null)
            animator.Play("Portal_Close");

        if (audioSource != null && closeClip != null)
            audioSource.PlayOneShot(closeClip);
    }

    // Animation Event at end of Portal_Open
    public void OnPortalOpenFinished()
    {
        if (boss != null)
            boss.OnPortalOpened(this);
    }

    // Animation Event at end of Portal_Close
    public void OnPortalCloseFinished()
    {
        if (boss != null)
            boss.OnPortalClosed();

        Destroy(gameObject);
    }
}
