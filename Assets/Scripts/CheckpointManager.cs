using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public Vector3 RespawnPosition { get; private set; }
    public bool HasCheckpoint { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //　シーン内限定ならDontDestroyOnLoadは不要
        //　DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(Vector3 pos)
    {
        RespawnPosition = pos;
        HasCheckpoint = true;
    }
}
