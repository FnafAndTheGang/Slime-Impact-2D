using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 lastCheckpoint;

    void Awake()
    {
        instance = this;
        lastCheckpoint = transform.position; // default spawn
    }

    public void SetCheckpoint(Vector3 pos)
    {
        lastCheckpoint = pos;
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpoint;
    }
}
