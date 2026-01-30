using UnityEngine;

public class DoorInsideTrigger : MonoBehaviour
{
    public AutomaticDoor door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door.CloseAndLockDoor();
    }
}
