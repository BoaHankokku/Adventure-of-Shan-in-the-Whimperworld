using UnityEngine;
using System.Collections;
using ClearSky.Enemy;

public class EnemyAIStage1 : EnemyBase
{
    private bool isAttacking = false;

    private void Update()
    {
        if (!isAttacking && canAttack && currentEnergy >= energyCostPerAttack)
        {
            StartCoroutine(AutoAttack());
        }
    }

    private IEnumerator AutoAttack()
    {
        isAttacking = true;
        currentEnergy -= energyCostPerAttack;

        if (energyBar != null)
        {
            energyBar.SetEnergy(currentEnergy, maxEnergy);
        }

        if (anim != null)
            anim.SetTrigger("attack");

        EnableAttackCollider();
        yield return new WaitForSeconds(attackCooldown);
        DisableAttackCollider();

        isAttacking = false;
    }
}
