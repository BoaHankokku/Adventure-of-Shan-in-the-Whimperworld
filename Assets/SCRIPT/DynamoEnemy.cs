using UnityEngine;
using System.Collections;

namespace ClearSky.Enemy
{
    public class DynamoEnemy : BossEnemy
    {
        private bool isAttacking = false;
        private bool encounterStarted = false;
        private bool attackOnCooldown = false;
        private bool attackInProgress = false;


        protected override void Start()
        {
            base.Start();
            Debug.Log("[DynamoEnemy] Start method called.");
        }

        protected override void Update() // Use override here to avoid hiding the method in BossEnemy
        {
            if (!encounterStarted)
            {
                return;
            }

            RegenerateEnergy();
            DonutDynamoBehavior();
        }

        public void StartDonutDynamoEncounter()
        {
            encounterStarted = true;
            statsBarObject?.SetActive(true);
            Debug.Log("[DynamoEnemy] Dynamo encounter started: Stats bar activated.");
        }

        private void DonutDynamoBehavior()
        {
            // Skip checking attack conditions if any part of the attack process is in progress
            if (attackInProgress || attackOnCooldown || isAttacking)
            {
                Debug.Log("[DynamoEnemy] Attack or cooldown is already in progress, skipping condition check.");
                return;
            }

            // Check if there is enough energy to attack
            if (currentEnergy >= energyCostPerAttack)
            {
                Debug.Log("[DynamoEnemy] Conditions met for attack. Starting attack.");
                StartCoroutine(StartAttack()); // Begin the attack
            }
        }

        private IEnumerator StartAttack()
        {
            // Check if the entire attack process is already in progress
            if (attackInProgress)
            {
                Debug.Log("[DynamoEnemy] StartAttack skipped due to ongoing attack or cooldown.");
                yield break;
            }

            // Begin the attack phase
            attackInProgress = true; // Lock the entire cycle
            isAttacking = true;
            Debug.Log($"[DynamoEnemy] isAttacking Anim true");
            anim.SetTrigger("isAttacking");
            // Deduct energy for the attack
            currentEnergy -= energyCostPerAttack;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy); // Clamp to avoid negative energy
            UpdateUI();
            Debug.Log($"[DynamoEnemy] Energy reduced after attack. New energy level: {currentEnergy}");

            // Wait for the full duration of the attack animation
            yield return new WaitForSeconds(1f); // Adjust this to match your actual attack animation length


            // Attack phase complete, now start cooldown
            isAttacking = false; // End the attack animation state
            Debug.Log($"[DynamoEnemy] isAttacking anim false");
            anim.ResetTrigger("isAttacking");
            attackOnCooldown = true; // Start cooldown after attack is finished
            anim.SetTrigger("idle");
            Debug.Log("[DynamoEnemy] Attack finished, entering cooldown.");

            // Wait for the cooldown period
            Debug.Log($"[DynamoEnemy] Starting cooldown with duration: {attackCooldown}");
            yield return new WaitForSeconds(attackCooldown);

            // Cooldown complete, reset for next attack
            attackOnCooldown = false;
            attackInProgress = false; // Unlock the entire cycle
            Debug.Log("[DynamoEnemy] Attack cooldown completed, ready to attack again.");
        }


        protected override void Die()
        {
            if (isDead) return;

            isDead = true;
            anim.SetTrigger("die");
            Debug.Log("[DynamoEnemy] Enemy has died.");

            if (statsBarObject != null) statsBarObject.SetActive(false);

            StartCoroutine(HandleDeath());

        }
        private IEnumerator HandleDeath()
        {
            GameManager activeGameManager = FindObjectOfType<GameManager>();
            if (activeGameManager != null)
            {
                Debug.Log("[DynamoEnemy] Calling activeGameManager.EnemyDefeated");
                activeGameManager.EnemyDefeated(gameObject);
            }
            else
            {
                Debug.LogError("[DynamoEnemy] Could not find GameManager in the scene!");
            }

            yield return new WaitForSeconds(2f); // Add delay before triggering dialogue

            if (GameManager.Instance is GameManager stage1Manager)
            {
                Debug.Log("[DynamoEnemy] Triggering Donut Dynamo Dialogue");
                stage1Manager.stage1Dialogue.AftertDonutDynamoEncounter();
            }
            else
            {
                Debug.LogError("[DynamoEnemy] Failed to access GameManager or stage1Dialogue.");
            }

            Destroy(gameObject, 1f);
        }
        public void EnableDynAttackCollider()
        {
            EnableAttackCollider();
        }

        public void DisableDynAttackCollider()
        {
           DisableAttackCollider();
        }
      
    }
}
