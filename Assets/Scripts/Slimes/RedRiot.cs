using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss UI")]
    public GameObject bossUIRoot;
    public TMPro.TextMeshProUGUI bossName;
    public string bossTitle = "Red Riot";

    [Header("Boss Script")]
    public MonoBehaviour bossScript;

    [Header("Music Control")]
    public AudioSource[] musicToStop;      // All normal music tracks
    public AudioSource bossMusic;          // Boss theme

    private bool triggered = false;

    void Start()
    {
        if (bossUIRoot != null)
            bossUIRoot.SetActive(false);

        if (bossScript != null)
            bossScript.enabled = false;

        // Boss music should be off at start
        if (bossMusic != null)
            bossMusic.Stop();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        // Show UI
        if (bossUIRoot != null)
            bossUIRoot.SetActive(true);

        // Set boss name
        if (bossName != null)
            bossName.text = bossTitle;

        // Activate boss logic
        if (bossScript != null)
            bossScript.enabled = true;

        // Stop all other music
        foreach (var track in musicToStop)
        {
            if (track != null && track.isPlaying)
                track.Stop();
        }

        // Start boss theme
        if (bossMusic != null)
            bossMusic.Play();

        Debug.Log("Boss Trigger Activated: " + bossTitle);
    }
}
