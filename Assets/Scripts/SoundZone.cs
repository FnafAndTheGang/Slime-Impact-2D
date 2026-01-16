using UnityEngine;

public class SoundZone : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource zoneAudio;   // The sound to play while inside
    public bool loopSound = true;   // Should the sound loop?

    private bool playerInside = false;

    void Start()
    {
        if (zoneAudio != null)
            zoneAudio.loop = loopSound;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (zoneAudio != null && !zoneAudio.isPlaying)
            zoneAudio.Play();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (zoneAudio != null)
            zoneAudio.Stop();
    }
}
