using UnityEngine;

public class ScientistSpinHitbox : MonoBehaviour
{
    public ScientistBoss boss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (boss.IsSpinning)
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
                player.TakeDamage(1);
        }
    }
}
