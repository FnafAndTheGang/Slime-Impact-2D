using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    [Header("Door Movement")]
    public Transform door;
    public float openHeight = 3f;
    public float openSpeed = 3f;
    public float closeSpeed = 6f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockSound;

    [Header("Warning Light")]
    public SpriteRenderer warningLight;
    public float flashSpeed = 10f;

    private Vector3 closedPos;
    private Vector3 openPos;

    private bool isOpening = false;
    private bool isClosing = false;
    private bool isLocked = false;

    private bool playedOpen = false;
    private bool playedClose = false;
    private bool playedLock = false;

    void Start()
    {
        closedPos = door.position;
        openPos = closedPos + new Vector3(0, openHeight, 0);

        if (warningLight != null)
            warningLight.enabled = false;
    }

    void Update()
    {
        if (isOpening && !isLocked)
        {
            if (!playedOpen)
            {
                Play(openSound);
                playedOpen = true;
            }

            FlashWarningLight(false);

            door.position = Vector3.MoveTowards(door.position, openPos, openSpeed * Time.deltaTime);

            if (Vector3.Distance(door.position, openPos) <= 0.001f)
            {
                isOpening = false;
                StopAudio();
            }
        }

        if (isClosing)
        {
            if (!playedClose)
            {
                Play(closeSound);
                playedClose = true;
            }

            FlashWarningLight(true);

            door.position = Vector3.MoveTowards(door.position, closedPos, closeSpeed * Time.deltaTime);

            if (Vector3.Distance(door.position, closedPos) <= 0.001f)
            {
                isClosing = false;
                StopAudio();
                FlashWarningLight(false);

                if (!playedLock)
                {
                    Play(lockSound);
                    playedLock = true;
                }

                isLocked = true;
            }
        }
    }

    public void OpenDoor()
    {
        if (isLocked) return;

        isOpening = true;
        isClosing = false;
        playedOpen = false;

        FlashWarningLight(false);
    }

    public void CloseAndLockDoor()
    {
        isClosing = true;
        isOpening = false;
        playedClose = false;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }

    void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    void FlashWarningLight(bool active)
    {
        if (warningLight == null)
            return;

        if (!active)
        {
            warningLight.enabled = false;
            return;
        }

        warningLight.enabled = true;

        float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
        Color c = warningLight.color;
        c.a = alpha;
        warningLight.color = c;
    }
}
