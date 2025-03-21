using UnityEngine;
using System.Collections;

namespace ClearSky.Enemy
{
    public class TrueFormEnemy : BossEnemy
    {
        [Header("Detection and Patrol")]
        public GameObject pointA; // Left patrol point
        public GameObject pointB; // Right patrol point
        public float patrolSpeed = 2f; // Speed when patrolling

        private Transform currentPoint; // Current target patrol point
        private Rigidbody2D rb;
        private Transform targetPlayer;
        private bool encounterStarted = false;
        private bool isAttacking = false;
        private bool attackInProgress = false;
        private bool attackOnCooldown = false;

        [Header("Trigger Area")]
        public GameObject triggerArea;
        [SerializeField] private Collider2D triggerAreaCollider; // Visible for debugging in Inspector
        private bool playerInTriggerArea = false;

        protected override void Start()
        {
            base.Start();

            rb = GetComponent<Rigidbody2D>();
            targetPlayer = GameObject.FindWithTag("Player")?.transform;

            if (targetPlayer == null)
            {
                Debug.LogError("Player not found. Ensure Player object has the 'Player' tag.");
                return;
            }

            // Set up the trigger area
            if (triggerAreaCollider == null)
            {
                Debug.LogError("TriggerAreaCollider is not assigned! Ensure it is set in the Inspector.");
            }

            // Set the initial patrol point and orientation
            currentPoint = pointB.transform;
            
        }


        public void StartTrueFormEncounter()
        {
            encounterStarted = true;
            statsBarObject?.SetActive(true);
            Debug.Log("True Form encounter started: Stats bar activated.");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"Collided with: {collision.name}, Tag: {collision.tag}");
            if (collision.CompareTag("Player"))
            {
                playerInTriggerArea = true;
                FlipTowardsPlayer();
                Debug.Log("OnTriggerEnter2D: Player entered the trigger area.");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Debug.Log($"OnTriggerExit2D called with: {collision.name}, Tag: {collision.tag}");

            if (collision.CompareTag("Player"))
            {
                playerInTriggerArea = false;
                isAttacking = false;
                anim.SetBool("isAttacking", false);

                Debug.Log("OnTriggerExit2D: Player exited the trigger area.");

                // Determine next patrol point
                currentPoint = transform.position.x > (pointA.transform.position.x + pointB.transform.position.x) / 2
                    ? pointA.transform
                    : pointB.transform;
                Debug.Log($"Next patrol point set to {currentPoint.name}.");

                rb.velocity = Vector2.zero; // Reset velocity
                PatrolBetweenPoints(); // Resume patrolling
            }
        }

        protected override void Update()
        {
            if (!encounterStarted)
            {
                Debug.Log("Update: Encounter not started. Enemy is idle.");
                return;
            }

            if (playerInTriggerArea)
            {
         
                if (!isAttacking)
                {
                    Debug.Log("Update: Player detected. Starting attack...");
                    StartCoroutine(StartAttack());
                }
            }
            else
            {
                Debug.Log("Update: Player not in trigger area. Patrolling...");
                PatrolBetweenPoints();
            }
            RegenerateEnergy();
        }

        private void PatrolBetweenPoints()
        {
            // Skip patrolling if the player is in the trigger area
            if (playerInTriggerArea) return;

            // Calculate the distance to the current target point
            float distance = Vector2.Distance(transform.position, currentPoint.position);

            // If close to the current patrol point, switch to the next point
            if (distance < 3f) // Reduced threshold for precise point switching
            {
                // Switch patrol points
                currentPoint = currentPoint == pointB.transform ? pointA.transform : pointB.transform;
                Debug.Log($"Switch patrol point: Now moving to {(currentPoint == pointB.transform ? "Point B" : "Point A")}");
            }

            // Call FlipTowardsPoints to ensure the sprite is flipped correctly
            FlipTowardsPoints();

            // Move toward the current patrol point
            Vector2 direction = (currentPoint.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);

            Debug.Log($"Moving towards {(currentPoint == pointB.transform ? "Point B" : "Point A")}, Velocity: {rb.velocity}");

            // Ensure the walking animation is active
            anim.SetBool("isWalking", true);
        }


        private void FlipTowardsPoints()
        {
            // Calculate the direction to the current patrol point
            float direction = currentPoint.position.x - transform.position.x;

            if (direction > 0) // Moving to the right (Point B)
            {
                Flip(false);
                Debug.Log("FlipTowardsPoints: Flipping to face Point B (Right).");
            }
            else if (direction < 0) // Moving to the left (Point A)
            {
                Flip(true);
                Debug.Log("FlipTowardsPoints: Flipping to face Point A (Left).");
            }
        }



        private void Flip(bool faceRight)
        {
            Vector3 localScale = transform.localScale;

            // Force flip regardless of current scale
            localScale.x = faceRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
            Debug.Log($"Flip: Sprite flipped to {(faceRight ? "right" : "left")}. New LocalScale: {transform.localScale}");
        }



        private IEnumerator StartAttack()
        {
            // Check if the entire attack process is already in progress
            if (attackInProgress)
            {
                Debug.Log("[TrueFormEnemy] StartAttack skipped due to ongoing attack or cooldown.");
                yield break;
            }

            // Begin the attack phase
            attackInProgress = true; // Lock the entire cycle
            isAttacking = true; // Begin attacking phase
            rb.velocity = Vector2.zero; // Stop movement while attacking

            FlipTowardsPlayer(); // Ensure the enemy faces the player
            Debug.Log("[TrueFormEnemy] Enemy flipped towards player for attack.");

            // Check if there is enough energy for the attack
            if (currentEnergy >= energyCostPerAttack)
            {
                // Deduct energy and update UI
                currentEnergy -= energyCostPerAttack;
                currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy); // Clamp to avoid negative energy
                UpdateUI();
                Debug.Log($"[TrueFormEnemy] Energy reduced by {energyCostPerAttack}. Remaining energy: {currentEnergy}");
            }
            else
            {
                // Insufficient energy, abort attack
                Debug.LogWarning("[TrueFormEnemy] Not enough energy to attack.");
                attackInProgress = false;
                isAttacking = false;
                yield break;
            }

            // Set the attacking animation
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", true);
            Debug.Log("[TrueFormEnemy] Starting attack animation.");

            // Wait for the attack animation to complete
            yield return new WaitForSeconds(1f); // Adjust to match your attack animation length

            // Attack animation completed
            Debug.Log("[TrueFormEnemy] Attack animation completed.");

            // End the attack phase
            isAttacking = false;
            anim.SetBool("isAttacking", false);
            anim.SetBool("isWalking", true);

            // Begin cooldown phase
            attackOnCooldown = true;
            Debug.Log("[TrueFormEnemy] Attack finished, entering cooldown.");
            yield return new WaitForSeconds(attackCooldown); // Wait for cooldown duration

            // Cooldown completed
            attackOnCooldown = false;
            attackInProgress = false; // Unlock the cycle
            Debug.Log("[TrueFormEnemy] Attack cooldown completed, ready to attack again.");

            // Post-attack behavior based on unique TrueFormEnemy logic
            if (playerInTriggerArea && currentEnergy >= energyCostPerAttack)
            {
                // Player is still within range and there is enough energy for another attack
                Debug.Log("[TrueFormEnemy] Player still in trigger area. Continuing to attack.");
                StartCoroutine(StartAttack());
            }
            else
            {
                // Player is out of range or not enough energy; return to patrolling
                Debug.Log("[TrueFormEnemy] Player left trigger area or insufficient energy. Returning to patrol.");
                anim.SetBool("isWalking", true);
                PatrolBetweenPoints();
            }
        }



        protected override void Die()
        {
            if (isDead) return;

            isDead = true;
            anim.SetTrigger("die");
            Debug.Log("[TrueFormEnemy] Enemy has died.");

            // Update UI before destruction
            if (statsBarObject != null) statsBarObject.SetActive(false);

            if (GameManager.Instance != null)
            {
                Debug.Log("[TrueFormEnemy] Calling GameManager.Instance.EnemyDefeated");
                GameManager.Instance.EnemyDefeated(gameObject);
            }
            else
            {
                Debug.LogError("[TrueFormEnemy] GameManager.Instance is null! Trying FindObjectOfType<GameManager>()");
                GameManager fallbackManager = FindObjectOfType<GameManager>();
                if (fallbackManager != null)
                {
                    fallbackManager.EnemyDefeated(gameObject);
                }
                else
                {
                    Debug.LogError("[TrueFormEnemy] No GameManager found in scene!");
                }
            }

            Destroy(gameObject, 1f);

            if (GameManager.Instance is GameManager stage1Manager)
            {
                Debug.Log("[TrueFormEnemy] Triggering Donut Dynamo Dialogue");
                stage1Manager.stage1Dialogue.AfterTFormEncounter();
            }
            else
            {
                Debug.LogError("[TrueFormEnemy] Failed to access GameManager or stage1Dialogue.");
            }

        }


        private void FlipTowardsPlayer()
        {
            if (targetPlayer == null) return;

            float direction = targetPlayer.position.x - transform.position.x;

            if (direction > 0) 
            {
                Flip(false);
                Debug.Log($"FlipTowardsPlayer: Flipping to face the player on the right. Direction: {direction}");
            }
            else if (direction < 0) 
            {
                Flip(true);
                Debug.Log($"FlipTowardsPlayer: Flipping to face the player on the left. Direction: {direction}");
            }
        }



        public void EnableTformAttackCollider()
        {
            EnableAttackCollider();
        }

        public void DisableTformAttackCollider()
        {
            DisableAttackCollider();
        }
    }
}