using UnityEngine;

public class IfaEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        IfaEscortController ifa = other.GetComponent<IfaEscortController>();
        if (ifa != null)
            ifa.ReachDestination();
    }
}
