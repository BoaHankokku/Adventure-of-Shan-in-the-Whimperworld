using UnityEngine;

public class PlayerBarrier : MonoBehaviour
{
    public float freezeDuration = 1f; // Duration to freeze the player
    public float activationDistance = 5f; // Distance to activate the barrier effect
    private bool isPlayerFrozen = false; // Flag to check if the player is frozen
    private Rigidbody2D playerRb; // Reference to the player's Rigidbody2D
    private Collider2D barrierCollider; // Reference to the barrier's Collider2D

    private void Start()
    {
        // Find the player's Rigidbody2D in the scene
        GameObject player = GameObject.Find("Player"); // Use the name from the hierarchy
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Debug.Log("[PlayerBarrier] Player Rigidbody2D successfully assigned.");
            }
            else
            {
                Debug.LogError("[PlayerBarrier] Rigidbody2D component not found on Player!");
            }
        }
        else
        {
            Debug.LogError("[PlayerBarrier] Player GameObject not found in the scene!");
        }

        // Get the Collider2D component of the barrier
        barrierCollider = GetComponent<Collider2D>();
        if (barrierCollider == null)
        {
            Debug.LogError("[PlayerBarrier] Barrier Collider2D not found!");
        }
    }

    private void Update()
    {
        if (playerRb == null || isPlayerFrozen) return;

        // Calculate the distance between the player and the barrier
        float distance = Vector2.Distance(playerRb.transform.position, transform.position);
        Debug.Log($"[PlayerBarrier] Distance to barrier: {distance}");

        // Trigger the freeze if within activation distance
        if (distance < activationDistance)
        {
            Debug.Log($"[PlayerBarrier] Player entered barrier range ({activationDistance}). Freezing movement.");
            FreezePlayer(); // Direct freeze without Coroutine
        }
    }

    private void FreezePlayer()
    {
        isPlayerFrozen = true;

        // Stop the player's velocity
        playerRb.velocity = Vector2.zero;

        // Temporarily disable Rigidbody2D physics
        playerRb.isKinematic = true;

        // Disable the barrier's trigger immediately to block the player
        if (barrierCollider != null)
        {
            barrierCollider.isTrigger = false; // Set the barrier as solid
            Debug.Log("[PlayerBarrier] Barrier trigger disabled.");
        }

        // Use Invoke to re-enable player movement after a freeze duration
        Invoke(nameof(UnfreezePlayer), freezeDuration);
    }

    private void UnfreezePlayer()
    {
        // Re-enable Rigidbody2D physics
        playerRb.isKinematic = false;
        isPlayerFrozen = false;

        Debug.Log("[PlayerBarrier] Player movement restored.");
    }
}
