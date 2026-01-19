using UnityEngine;

public class RedRiotTrigger : MonoBehaviour
{
    [Header("Boss UI")]
    public GameObject bossUIRoot;
    public TMPro.TextMeshProUGUI bossName;
    public string bossTitle = "Red Riot";

    [Header("Boss Script")]
    public RedRiotBoss boss;   // ⭐ Use the actual boss type

    [Header("Music Control")]
    public AudioSource[] musicToStop;
    public AudioSource bossMusic;

    private bool triggered = false;

    void Start()
    {
        if (bossUIRoot != null)
            bossUIRoot.SetActive(false);

        if (boss != null)
            boss.enabled = false;

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

        if (bossName != null)
            bossName.text = bossTitle;

        // Enable boss script
        if (boss != null)
            boss.enabled = true;

        // ⭐ Start the boss fight
        if (boss != null)
            boss.StartBoss();

        // Stop normal music
        foreach (var track in musicToStop)
        {
            if (track != null && track.isPlaying)
                track.Stop();
        }

        // Start boss theme
        if (bossMusic != null)
            bossMusic.Play();

        Debug.Log("Red Riot Trigger Activated");
    }
}
