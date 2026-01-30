using UnityEngine;

public class DoorEntryTrigger : MonoBehaviour
{
    public AutomaticDoor door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            door.OpenDoor();
    }
}
