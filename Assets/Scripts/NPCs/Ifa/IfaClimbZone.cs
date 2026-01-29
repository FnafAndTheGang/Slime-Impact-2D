using UnityEngine;

public class IfaClimbZone : MonoBehaviour
{
    public float climbAmount = 0.5f;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IfaEscortController ifa = other.GetComponent<IfaEscortController>();
        if (ifa != null)
        {
            ifa.ApplyClimb(climbAmount);
        }
    }
}
