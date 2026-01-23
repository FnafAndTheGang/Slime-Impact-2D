using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public PlayerController2D player;

    public GameObject bossHealthUI;
    public GameObject bossUIRoot;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!player.isAttacking)
            return;

        // BLUE SLIME
        BlueSlime blue = other.GetComponent<BlueSlime>();
        if (blue != null)
        {
            blue.TakeHit();
            return;
        }

        // RED SLIME
        RedSlime red = other.GetComponent<RedSlime>();
        if (red != null)
        {
            red.TakeHit();
            return;
        }

        // RED RIOT
        RedRiotBoss boss = other.GetComponent<RedRiotBoss>();
        if (boss != null)
        {
            bossHealthUI.SetActive(true);
            bossUIRoot.SetActive(true);
            boss.TakeHitFrom(this.gameObject);
        }
    }
}
