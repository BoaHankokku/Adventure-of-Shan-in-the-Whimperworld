using UnityEngine;

public class ZoneActivator : MonoBehaviour
{
    public int zoneIndex;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"ZoneActivator triggered for zoneIndex: {zoneIndex}");

            // Check if GameManager exists and call ActivateZone
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActivateZone(zoneIndex);
                Debug.Log("ActivateZone called on GameManager.");
            }
            if (GameManager2.Instance != null)
            {
                GameManager2.Instance.ActivateZone(zoneIndex);
                Debug.Log("ActivateZone called on GameManager.");
            }
            if (GameManager3.Instance != null)
            {
                GameManager3.Instance.ActivateZone(zoneIndex);
                Debug.Log("ActivateZone called on GameManager.");
            }
            Debug.Log($"ZoneActivator GameObject ({gameObject.name}) is being destroyed.");
            Destroy(gameObject);
        }
    }
}
