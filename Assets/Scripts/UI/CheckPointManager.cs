using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    // The last saved checkpoint position
    [SerializeField]
    private Vector3 currentCheckpoint;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // optional, but usually desired
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called by Checkpoint.cs when a checkpoint is activated
    public void SetCheckpoint(Vector3 pos)
    {
        currentCheckpoint = pos;
    }

    // Used by IfaEscortController
    public Vector3 GetCheckpoint()
    {
        return currentCheckpoint;
    }

    // Used by NewPlayerController2D.RespawnAtCheckpoint
    public Vector3 GetLastCheckpointPosition()
    {
        return currentCheckpoint;
    }
}
