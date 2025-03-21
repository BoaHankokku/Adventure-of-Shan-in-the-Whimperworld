using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int checkpointID; // Unique ID for this checkpoint

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Checkpoint {checkpointID} triggered!");
            SaveCheckpointProgress();
            Destroy(gameObject);
        }
    }

    private void SaveCheckpointProgress()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        GameDataManager.Instance.SavePlayerTransform(playerTransform);
        GameDataManager.Instance.SaveObjectives(checkpointID);
        GameDataManager.Instance.SavePlayerStats();
        Debug.Log($"Checkpoint {checkpointID}: Player progress saved.");
    }

}
