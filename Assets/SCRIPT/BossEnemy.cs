using UnityEngine;

namespace ClearSky.Enemy
{
    public class BossEnemy : EnemyBase
    {
        public GameObject statsBarObject;
        protected BossStats bossStats;
        public float energyRegenRate = 1.0f;

        protected override void Start()
        {
            base.Start();
            if (statsBarObject != null)
            {
                bossStats = statsBarObject.GetComponent<BossStats>();
                statsBarObject.SetActive(false);
                bossStats?.InitializeBars(maxHealth, maxEnergy);
                Debug.Log("Boss stats initialized with max health and energy.");
            }
        }

        protected override void Update()  // Ensure Update is virtual for further overrides
        {
            base.Update();
            RegenerateEnergy();
        }

        protected virtual void RegenerateEnergy()
        {
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += Time.deltaTime * energyRegenRate;
                currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
                bossStats?.UpdateStats(health, currentEnergy);
            }
        }
        protected override void UpdateUI()
        {
            base.UpdateUI();
            bossStats?.UpdateStats(health, currentEnergy);
        }
    }
}
