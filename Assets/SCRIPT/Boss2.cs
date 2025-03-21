using ClearSky.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : BossEnemy
{
    private bool isAttacking = false;
    private bool encounterStarted = false;
    private bool attackOnCooldown = false;
    private bool attackInProgress = false;

    protected override void Start()
    {
        base.Start();
        Debug.Log("[BOSS2] Start method called.");
    }

    protected override void Update() // Use override here to avoid hiding the method in BossEnemy
    {
        if (!encounterStarted)
        {
            return;
        }

        RegenerateEnergy();
        BOSSDynamoBehavior();
    }

    public void StartBossEncounter()
    {
        encounterStarted = true;
        statsBarObject?.SetActive(true);
        Debug.Log("[BOSS2] Dynamo encounter started: Stats bar activated.");
    }

    private void BOSSDynamoBehavior()
    {
        // Skip checking attack conditions if any part of the attack process is in progress
        if (attackInProgress || attackOnCooldown || isAttacking)
        {
            Debug.Log("[BOSS2] Attack or cooldown is already in progress, skipping condition check.");
            return;
        }

        // Check if there is enough energy to attack
        if (currentEnergy >= energyCostPerAttack)
        {
            Debug.Log("[BOSS2] Conditions met for attack. Starting attack.");
            StartCoroutine(StartAttack()); // Begin the attack
        }
    }

    private IEnumerator StartAttack()
    {
        if (attackInProgress)
        {
            Debug.LogWarning("[BOSS2] StartAttack skipped due to ongoing attack or cooldown.");
            yield break;
        }

        attackInProgress = true;
        isAttacking = true;

        if (anim == null)
        {
            Debug.LogError("[BOSS2] Animator reference is missing or not assigned!");
            yield break;
        }

        // Force the attack animation to play
        anim.Play("attack"); // Replace "Attack" with the actual animation state name
        Debug.Log("[BOSS2] Forcibly playing Attack animation.");

        // Log Animator state after playing
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[BOSS2] Current state after Play: {stateInfo.fullPathHash}");

        // Wait for animation duration (use the actual length of the attack animation)
        yield return new WaitForSeconds(2f); // Adjust to match your attack animation duration

        Debug.Log("[BOSS2] Attack animation completed. Resetting states.");
        isAttacking = false;
        attackOnCooldown = true;

        Debug.Log($"[BOSS2] Starting cooldown with duration: {attackCooldown}");
        yield return new WaitForSeconds(attackCooldown);

        attackOnCooldown = false;
        attackInProgress = false;
        Debug.Log("[BOSS2] Attack cooldown completed, ready to attack again.");
    }
    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("die");
        Debug.Log("[BOSS2] Enemy has died.");

        if (statsBarObject != null) statsBarObject.SetActive(false);

        GameManager2.Instance.EnemyDefeated(gameObject);
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

