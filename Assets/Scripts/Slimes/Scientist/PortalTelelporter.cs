using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [Header("Bob Animation")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;

    private Vector3 startPos;

    [Header("Teleport Settings")]
    public Transform teleportTarget;      // Where the player will appear

    [Header("Audio")]
    public AudioSource portalEnterAudio;  // Plays when player touches portal
    public AudioSource arrivalAudio;      // Plays when player arrives at destination

    [Header("Player Tag")]
    public string playerTag = "Player";

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Simple bobbing motion
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0f, offset, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        // Play portal enter sound
        if (portalEnterAudio != null)
            portalEnterAudio.Play();

        // Teleport player
        if (teleportTarget != null)
            other.transform.position = teleportTarget.position;

        // Play arrival sound
        if (arrivalAudio != null)
            arrivalAudio.Play();
    }
}
