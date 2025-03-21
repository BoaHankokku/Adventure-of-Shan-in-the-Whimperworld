using ClearSky.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIStage3 : EnemyBase
{
    private bool isAttacking = false;
    private bool attackOnCooldown = false;
    private bool attackInProgress = false;
    private float energyRegenAmount = 1f;

    protected override void Start()
    {
        base.Start();
        Debug.Log("[EnemyAIStage3] Start method called.");
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("[EnemyAIStage3] Animator component is missing!");
        }
        else
        {
            Debug.Log("[EnemyAIStage3] Animator component successfully assigned.");
        }
        Debug.Log("[EnemyAIStage3] Verifying component assignments.");
        if (energyBar == null) Debug.LogError("[EnemyAIStage3] energyBar is missing!");
        if (anim == null) Debug.LogError("[EnemyAIStage3] Animator is missing!");
    }
    protected override void Update()
    {
        base.Update();

        // Regenerate energy over time
        RegenerateEnergy();

        // Run AI behavior logic
        EnemyAIBehaviour();
    }
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
    private IEnumerator StartAttack()
    {
        if (attackInProgress)
        {
            Debug.Log("[EnemyAIStage2] Attack already in progress. Skipping.");
            yield break;
        }

        attackInProgress = true;
        isAttacking = true;

        Debug.Log("[EnemyAIStage2] Triggering attack animation.");
        anim.SetTrigger("isAttacking");

        currentEnergy -= energyCostPerAttack;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        if (energyBar != null)
            energyBar.SetEnergy(currentEnergy, maxEnergy);

        Debug.Log($"[EnemyAIStage2] Attacking. Energy reduced to: {currentEnergy}");

        yield return new WaitForSeconds(1f); // Adjust based on attack animation duration

        isAttacking = false;
        anim.ResetTrigger("isAttacking");
        attackOnCooldown = true;

        Debug.Log("[EnemyAIStage2] Attack finished. Entering cooldown.");
        anim.SetTrigger("idle");

        yield return new WaitForSeconds(attackCooldown);

        attackOnCooldown = false;
        attackInProgress = false;

        Debug.Log("[EnemyAIStage2] Cooldown complete. Ready to attack again.");
    }
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

        Debug.Log("[EnemyAIStage2] Enemy defeated.");
        GameManager3.Instance.EnemyDefeated(gameObject);
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
