using UnityEngine;
using System;
using System.Collections;

namespace ClearSky.Enemy
{
    public class EnemyAIStage2 : EnemyBase
    {
        private bool isAttacking = false;
        private bool attackOnCooldown = false;
        private bool attackInProgress = false;

        private float energyRegenAmount = 1f; // Energy regeneration rate per frame
        public event Action<GameObject> OnDefeat; // Event to notify defeat

        protected override void Start()
        {
            base.Start();
            Debug.Log("[EnemyAIStage2] Start method called.");
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError("[EnemyAIStage2] Animator component is missing!");
            }
            else
            {
                Debug.Log("[EnemyAIStage2] Animator component successfully assigned.");
            }
        }

        protected override void Update()
        {
            base.Update();

            // Regenerate energy over time
            RegenerateEnergy();

            // Run AI behavior logic
            EnemyAIBehaviour();
        }

        /// <summary>
        /// Regenerates energy and updates the UI.
        /// </summary>
        private void RegenerateEnergy()
        {
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += energyRegenAmount * Time.deltaTime;
                currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

                if (energyBar != null)
                    energyBar.SetEnergy(currentEnergy, maxEnergy);
            }
        }

        /// <summary>
        /// Manages AI behavior, triggering attacks when conditions are met.
        /// </summary>
        private void EnemyAIBehaviour()
        {
            if (attackInProgress || attackOnCooldown || isAttacking)
            {
                Debug.Log("[EnemyAIStage2] Skipping attack due to in-progress or cooldown state.");
                return;
            }

            if (currentEnergy >= energyCostPerAttack)
            {
                Debug.Log("[EnemyAIStage2] Conditions met to start attack.");
                StartCoroutine(StartAttack());
            }
        }

        /// <summary>
        /// Starts an attack sequence with animations and cooldown.
        /// </summary>
        private IEnumerator StartAttack()
        {
            if (attackInProgress)
            {
                Debug.LogWarning("[BOSS2] StartAttack skipped due to ongoing attack or cooldown.");
                yield break;
            }

            attackInProgress = true; // Lock the cycle
            isAttacking = true;

            if (anim == null)
            {
                Debug.LogError("[BOSS2] Animator component is missing or not assigned!");
                yield break;
            }

            Debug.Log("[BOSS2] Triggering isAttacking animation.");
            anim.SetTrigger("isAttacking");

            // Log the Animator state before and after setting the trigger
            AnimatorStateInfo stateInfoBefore = anim.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[BOSS2] Before Trigger - Current state: {stateInfoBefore.fullPathHash}");

            yield return null; // Wait for Animator to process the trigger

            AnimatorStateInfo stateInfoAfter = anim.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[BOSS2] After Trigger - Current state: {stateInfoAfter.fullPathHash}");

            // Deduct energy for the attack
            currentEnergy -= energyCostPerAttack;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            UpdateUI();
            Debug.Log($"[BOSS2] Energy reduced after attack. New energy level: {currentEnergy}");

            // Wait for animation duration
            yield return new WaitForSeconds(1f); // Temporarily hardcoded duration

            Debug.Log("[BOSS2] Attack animation completed. Resetting states.");
            isAttacking = false;
            attackOnCooldown = true;

            Debug.Log($"[BOSS2] Starting cooldown with duration: {attackCooldown}");
            yield return new WaitForSeconds(attackCooldown);

            attackOnCooldown = false;
            attackInProgress = false;
            Debug.Log("[BOSS2] Attack cooldown completed, ready to attack again.");
        }
        /// <summary>
        /// Handles enemy defeat logic and invokes the OnDefeat event.
        /// </summary>
        protected override void Die()
        {
            if (isDead)
            {
                Debug.Log("[EnemyAIStage2] Die method called, but enemy is already dead.");
                return;
            }

            isDead = true;
            Debug.Log("[EnemyAIStage2] Triggering death animation.");
            anim.SetTrigger("die");

            // Notify defeat event
            OnDefeat?.Invoke(gameObject);

            Debug.Log("[EnemyAIStage2] Enemy defeated.");

            // Deactivate the enemy GameObject after a short delay
            Invoke(nameof(DeactivateGameObject), 1f);
        }
        /// <summary>
        /// Deactivates the enemy GameObject after death.
        /// </summary>
        private void DeactivateGameObject()
        {
            gameObject.SetActive(false);
            Debug.Log("[EnemyAIStage2] Enemy deactivated.");
        }

        /// <summary>
        /// Resets the enemy for reuse with new health and position.
        /// </summary>
        public void ResetEnemy(float newHealth, Vector3 newPosition)
        {
            if (!isDead && health == maxHealth)
            {
                Debug.LogWarning("[ResetEnemy] Ignored call because enemy is already active.");
                return;
            }
            Debug.Log($"[ResetEnemy] Method called. Caller: {Environment.StackTrace}");

            if (healthbar == null) Debug.LogError("[ResetEnemy] Healthbar is null!");
            if (energyBar == null) Debug.LogError("[ResetEnemy] EnergyBar is null!");
            if (anim == null) Debug.LogError("[ResetEnemy] Animator is null!");

            isDead = false;
            health = newHealth;
            transform.position = newPosition;

            healthbar?.SetHealth(health, maxHealth);
            energyBar?.SetEnergy(currentEnergy, maxEnergy);

            anim?.SetTrigger("idle");
            gameObject.SetActive(true);

            Debug.Log("[ResetEnemy] Enemy reset and ready.");
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            Debug.Log("[EnemyAIStage2] Enemy disabled. Resetting states.");

            // Reset attack-related states
            attackInProgress = false;
            attackOnCooldown = false;
            isAttacking = false;

            // Reset animations to idle or default state
            if (anim != null)
            {
                anim.ResetTrigger("isAttacking");
                anim.SetTrigger("idle");
            }

            // Unregister from events
            OnDefeat = null;

            // Optionally reset energy or health (if needed for reusability)
            currentEnergy = 0;
        }
        public void EnableDynAttackCollider()
        {
            EnableAttackCollider();
        }

        /// <summary>
        /// Disables the attack collider.
        /// </summary>
        public void DisableDynAttackCollider()
        {
            DisableAttackCollider();
        }
    }
}
