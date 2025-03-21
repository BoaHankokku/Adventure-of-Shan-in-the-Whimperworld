using ClearSky;
using ClearSky.Controller;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player
    public Vector3 offset = new Vector3(0, 0, -10); // Default offset
    [Range(1, 10)] public float smoothFactor = 5f; // Smooth following factor

    public Vector3 minValues, maxValues; // Boundaries for the camera movement
    private SimplePlayerController simplePlayerController; // Reference to player movement script

    private bool stopMovingLeft = false; // Variable to control left movement
    public BoxCollider2D[] zoneBoundaries; // Array of zone boundaries
    private int currentZoneIndex = 0; // Track which zone the player is currently in

    private void Start()
    {
        // Validate and assign components
        if (target == null)
        {
            Debug.LogError("[CameraFollow] Target not assigned!");
            return;
        }

        // Get the player movement script
        simplePlayerController = target.GetComponent<SimplePlayerController>();
        if (simplePlayerController == null)
        {
            Debug.LogError("[CameraFollow] SimplePlayerController not found on target!");
            return;
        }

        // Initialize the first zone boundaries if available
        if (zoneBoundaries.Length > 0)
        {
            SetCameraBounds(zoneBoundaries[currentZoneIndex]);
        }
        else
        {
            Debug.LogWarning("[CameraFollow] No zone boundaries set!");
        }
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Follow();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ensure the collision is valid and matches the tag
        if (collision.CompareTag("Boundary"))
        {
            currentZoneIndex++;
            if (currentZoneIndex < zoneBoundaries.Length)
            {
                SetCameraBounds(zoneBoundaries[currentZoneIndex]);
                Debug.Log($"[CameraFollow] Entered Zone {currentZoneIndex}");
            }
            else
            {
                Debug.LogWarning("[CameraFollow] No more zones to enter!");
            }

            // Restrict player's left movement after entering a new zone
            stopMovingLeft = true;
            simplePlayerController.canMoveLeft = false;
        }
    }

    // Updates camera bounds based on the current zone
    private void SetCameraBounds(BoxCollider2D zone)
    {
        if (zone == null)
        {
            Debug.LogError("[CameraFollow] Zone boundary is null!");
            return;
        }

        // Set min and max values based on zone bounds
        minValues = zone.bounds.min;
        maxValues = zone.bounds.max;
        Debug.Log($"[CameraFollow] Camera bounds updated: Min({minValues}), Max({maxValues})");
    }

    // Smoothly follows the player within boundaries
    private void Follow()
    {
        // Target position with offset
        Vector3 targetPosition = target.position + offset;

        // Clamp the target position within bounds
        Vector3 boundPosition = new Vector3(
            Mathf.Clamp(targetPosition.x, minValues.x, maxValues.x), // Clamp X
            Mathf.Clamp(targetPosition.y, minValues.y, maxValues.y), // Clamp Y
            transform.position.z // Keep Z position fixed for 2D
        );

        // Smoothly move the camera towards the target position
        Vector3 smoothPosition = Vector3.Lerp(transform.position, boundPosition, smoothFactor * Time.fixedDeltaTime);
        transform.position = smoothPosition;

        // Restrict player movement after hitting the left boundary
        if (stopMovingLeft)
        {
            simplePlayerController.canMoveLeft = false;
        }
    }
}
