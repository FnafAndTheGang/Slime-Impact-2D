using UnityEngine;

public class GhostBombPickup : MonoBehaviour
{
    public AudioClip pickupSound;
    public GameObject abilityUI; // UI that should appear when picked up

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController2D player = collision.GetComponent<PlayerController2D>();
        if (player == null) return;

        GhostBomb ability = player.GetComponent<GhostBomb>();
        if (ability != null)
            ability.enabled = true;

        if (abilityUI != null)
            abilityUI.SetActive(true);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        gameObject.SetActive(false);
    }
}
