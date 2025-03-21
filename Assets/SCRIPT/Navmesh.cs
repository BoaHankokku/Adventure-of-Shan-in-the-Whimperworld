using UnityEngine;

public class PlatformBoundaryController : MonoBehaviour
{
    public GameObject[] boundaries; // Array of all boundary points
    public Transform player; // Reference to the player

    public static PlatformBoundaryController Instance { get; private set; }
    public bool IsPlayerClamped { get; private set; }

    private void Awake()
    {
        Instance = this; // Set the singleton instance
    }

    void Update()
    {
        RestrictPlayerWithinBoundaries();
    }

    /// <summary>
    /// Validates the player's position after loading by clamping it within boundaries.
    /// </summary>
    public void ValidatePlayerPositionAfterLoad()
    {
        if (player == null)
        {
            Debug.LogWarning("Player Transform is null. Ensure the player is properly assigned to the PlatformBoundaryController.");
            return; // Exit the method if the player is not assigned
        }

        RestrictPlayerWithinBoundaries();
        Debug.Log("Player position validated against platform boundaries after loading.");
    }

    /// <summary>
    /// Restricts the player's movement based on all boundaries.
    /// </summary>
    public void RestrictPlayerWithinBoundaries()
    {
        // Check if the player reference is null
        if (player == null)
        {
            Debug.LogWarning("Player Transform is null. Ensure the player is properly assigned to the PlatformBoundaryController.");
            return; // Exit the method if the player is not assigned
        }

        if (boundaries == null || boundaries.Length < 2)
        {
            Debug.LogWarning("PlatformBoundaryController: No boundaries set up or insufficient boundaries.");
            IsPlayerClamped = false; // Player is not clamped if no boundaries are defined
            return;
        }

        Vector3 playerPosition = player.position; // Safely access the player's position after null check
        IsPlayerClamped = false; // Reset clamped status initially

        // Existing boundary checking logic
        for (int i = 0; i < boundaries.Length - 1; i++)
        {
            if (boundaries[i] == null || boundaries[i + 1] == null) continue;

            float leftBoundary = boundaries[i].transform.position.x;
            float rightBoundary = boundaries[i + 1].transform.position.x;

            if (leftBoundary > rightBoundary)
            {
                (leftBoundary, rightBoundary) = (rightBoundary, leftBoundary);
            }

            if (playerPosition.x < leftBoundary || playerPosition.x > rightBoundary)
            {
                player.position = new Vector3(
                    Mathf.Clamp(playerPosition.x, leftBoundary, rightBoundary),
                    playerPosition.y,
                    playerPosition.z
                );

                IsPlayerClamped = true;
                Debug.Log($"Player position restricted between: {leftBoundary} and {rightBoundary}");
                return;
            }
        }

        float firstBoundary = boundaries[0].transform.position.x;
        float lastBoundary = boundaries[boundaries.Length - 1].transform.position.x;

        if (playerPosition.x < firstBoundary || playerPosition.x > lastBoundary)
        {
            player.position = new Vector3(
                Mathf.Clamp(playerPosition.x, firstBoundary, lastBoundary),
                playerPosition.y,
                playerPosition.z
            );
            IsPlayerClamped = true;
            Debug.Log($"Player clamped outside all boundaries to: {player.position.x}");
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize boundaries in the Scene view
        Gizmos.color = Color.red;

        if (boundaries != null && boundaries.Length > 0)
        {
            foreach (var boundary in boundaries)
            {
                if (boundary != null)
                {
                    Gizmos.DrawSphere(boundary.transform.position, 0.2f); // Draw a sphere for each boundary
                }
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < boundaries.Length - 1; i++)
            {
                if (boundaries[i] != null && boundaries[i + 1] != null)
                {
                    Gizmos.DrawLine(boundaries[i].transform.position, boundaries[i + 1].transform.position); // Draw lines between boundaries
                }
            }
        }
    }
}
