using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    private RedRiotBoss boss;

    // Called by boss when spawning an OPEN portal
    public void Open(AudioClip openClip, RedRiotBoss bossRef)
    {
        boss = bossRef;

        if (audioSource != null && openClip != null)
            audioSource.PlayOneShot(openClip);

        animator.Play("PortalOpen", 0, 0f); // ⭐ force play from start
    }

    // Animation event at end of PortalOpen
    public void OnPortalOpenFinished()
    {
        boss.OnPortalOpened(this);
    }

    // Called by boss when spawning a CLOSE portal
    public void Close(AudioClip closeClip, RedRiotBoss bossRef)
    {
        boss = bossRef;

        if (audioSource != null && closeClip != null)
            audioSource.PlayOneShot(closeClip);

        animator.Play("PortalClose", 0, 0f); // ⭐ force play from start
    }

    // Animation event at end of PortalClose
    public void OnPortalCloseFinished()
    {
        boss.OnPortalClosed();
        Destroy(gameObject);
    }
}
