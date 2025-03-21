using UnityEngine;
using System.Collections;

namespace ClearSky.Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField]
        private string monsterID; // Unique ID for each enemy

        public string MonsterID
        {
            get => monsterID;
            set => monsterID = value;
        }

        public float health;
        public float maxHealth;
        protected bool isDead = false;
        public bool IsDead => isDead;
        protected Animator anim;

        public float maxEnergy = 50f;
        protected float currentEnergy;
        public float CurrentEnergy => currentEnergy;
        public float CurrentHealth => health;
        public float damage;
        public float energyCostPerAttack;
        public float attackCooldown;

        public HealthbarBehaviour healthbar;
        public EnergyBarBehaviour energyBar;

        // Attack components
        public Collider2D attackCollider;
        public bool canAttack = true;

        // Enum to define the enemy type for objectives
        public enum EnemyType
        {
            //enemyStage1
            MMEnemy = 1,
            GummyEnemy = 2,
            Dynamo = 3,
            DynamoTrueForm = 4,
            //enemyStage2
            Brushzor = 5,
            FlourideFury = 6,
            Razormouth = 7,
            RazormouthTrueForm = 8,
            //enemyStage3
            Dusterion = 9,
            Oroflame = 10,
            Pulsefiend = 11,
            PulsefiendTrueForm = 12,
        }

        public virtual void Awake()
        {
            if (string.IsNullOrEmpty(MonsterID))
            {
                MonsterID = $"{gameObject.name}_{transform.position.GetHashCode()}"; // Unique but predictable ID
                Debug.Log($"Generated MonsterID: {MonsterID}");
            }

            anim = GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError("[EnemyBase] Animator component is missing!");
            }
        }
        public EnemyType enemyType;

        protected virtual void Start()
        {

            InitializeStats();

            if (healthbar != null) healthbar.SetHealth(health, maxHealth);
            currentEnergy = maxEnergy;
            if (energyBar != null) energyBar.SetEnergy(currentEnergy, maxEnergy);
        }

        protected virtual void InitializeStats()
        {
            switch (enemyType)
            {
                // Stage 1 Enemies - Lower health and damage, faster attack rate
                case EnemyType.MMEnemy:
                    health = 20f;
                    maxHealth = 20f;
                    damage = 10f;
                    energyCostPerAttack = 5f;
                    attackCooldown = 1.5f;
                    break;

                case EnemyType.GummyEnemy:
                    health = 30f;
                    maxHealth = 30f;
                    damage = 20f;
                    energyCostPerAttack = 10f;
                    attackCooldown = 2.0f;
                    break;

                case EnemyType.Dynamo:
                    health = 50f;
                    maxHealth = 50f;
                    damage = 25f;
                    energyCostPerAttack = 10f;
                    attackCooldown = 3f;
                    break;

                case EnemyType.DynamoTrueForm:
                    health = 100f;
                    maxHealth = 100f;
                    damage =30f;
                    energyCostPerAttack = 10f;
                    attackCooldown = 1f;
                    break;

                // Stage 2 Enemies - Higher health and damage, moderate attack rate
                case EnemyType.Brushzor:
                    health = 30f;
                    maxHealth = 30f;
                    damage = 15f;
                    energyCostPerAttack = 5f;
                    attackCooldown = 2f;
                    break;

                case EnemyType.FlourideFury:
                    health = 50f;
                    maxHealth = 50f;
                    damage = 25f;
                    energyCostPerAttack = 10f;
                    attackCooldown = 2f;
                    break;

                case EnemyType.Razormouth:
                    health = 100f;
                    maxHealth = 100f;
                    damage = 30f;
                    energyCostPerAttack = 30f;
                    attackCooldown = 3f;
                    break;

                case EnemyType.RazormouthTrueForm:
                    health = 100f;
                    maxHealth = 100f;
                    damage = 40f;
                    energyCostPerAttack = 25f;
                    attackCooldown = 5f;
                    break;

                // Stage 3 Enemies - High health and damage, slower but impactful attacks
                case EnemyType.Dusterion:
                    health = 40f;
                    maxHealth = 40f;
                    damage = 35f;
                    energyCostPerAttack = 5f;
                    attackCooldown = 5f;
                    break;

                case EnemyType.Oroflame:
                    health = 50f;
                    maxHealth = 50f;
                    damage = 40f;
                    energyCostPerAttack = 10f;
                    attackCooldown = 5f;
                    break;

                case EnemyType.Pulsefiend:
                    health = 100f;
                    maxHealth = 100f;
                    damage = 45f;
                    energyCostPerAttack = 25f;
                    attackCooldown = 4f;
                    break;

                case EnemyType.PulsefiendTrueForm:
                    health = 100f;
                    maxHealth = 100f;
                    damage = 50f;
                    energyCostPerAttack = 25f;
                    attackCooldown = 5f;
                    break;

                default:
                    Debug.LogError("Unknown enemy type!");
                    break;
            }
        }

        protected virtual void Update()  // Ensure Update is virtual
        {
            if (currentEnergy < maxEnergy)
            {
                float energyGain = Time.deltaTime;  // Define how much energy is gained per update
                currentEnergy += energyGain;
                UpdateEnergy(energyGain);  // Pass the energy gain amount
            }
        }
        protected void UpdateEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
            UpdateUI();
        }
        public virtual void TakeDamage(float damageTaken, bool isCritical)
        {
            if (isDead) return;
            health -= damageTaken;

            if (anim != null)
            {
                anim.SetTrigger("isHit");
                StartCoroutine(ClearIsHitTrigger());
            }

            if (health <= 0 && !isDead) Die();
            UpdateUI();
        }


        protected virtual void UpdateUI()
        {
            healthbar?.SetHealth(health, maxHealth);
            energyBar?.SetEnergy(currentEnergy, maxEnergy);
        }

        protected virtual void Die()
        {
            isDead = true;
            if (anim != null) anim.SetTrigger("die");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDefeated(gameObject);
            }
            else
            {
                Debug.LogError("[EnemyBase] GameManager.Instance is null!");
            }
            DisableAttackCollider();
            MarkAsDefeated();
        }

        // Call this function when the enemy is marked as defeated
        public void MarkAsDefeated()
        {
            // Check if the enemy is already marked as defeated in GameDataManager
            if (!GameDataManager.Instance.IsMonsterDefeated(MonsterID))
            {
                GameDataManager.Instance.MarkMonsterAsDefeated(MonsterID); // Mark the enemy as defeated
                Debug.Log($"Enemy {MonsterID} marked as defeated in GameDataManager.");
            }

            // Remove the enemy from the scene
            StartCoroutine(DelayedDestroy());
            Debug.Log($"Enemy {MonsterID} destroyed and removed from the scene.");
        }

        // Static method to handle all defeated enemies in the scene
        public static void DisableAllDefeatedEnemies()
        {
            EnemyBase[] allEnemies = FindObjectsOfType<EnemyBase>();
            int disabledCount = 0;

            foreach (EnemyBase enemy in allEnemies)
            {
                Debug.Log($"Checking enemy: {enemy.MonsterID}");

                // Validate against defeated monsters
                if (GameDataManager.Instance != null && GameDataManager.Instance.IsMonsterDefeated(enemy.MonsterID))
                {
                    Debug.Log($"Disabling and destroying defeated enemy: {enemy.MonsterID}");
                    Destroy(enemy.gameObject); // Destroy the enemy game object
                    disabledCount++;
                }
                else
                {
                    Debug.LogWarning($"Enemy {enemy.MonsterID} is not marked as defeated. Skipping.");
                }
            }

            Debug.Log($"Total enemies disabled: {disabledCount}");
        }



        private IEnumerator DelayedDestroy()
        {
            float delay = 1.0f; // Default delay (e.g., 1 second)
            if (anim != null)
            {
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                delay = Mathf.Min(stateInfo.length, 1.0f); // Limit delay to a max duration
            }

            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
            Debug.Log($"Enemy {MonsterID} destroyed after delay of {delay} seconds.");
        }


        private IEnumerator ClearIsHitTrigger()
        {
            yield return new WaitForSeconds(0.1f);
            anim.ResetTrigger("isHit");
        }



        public void EnableAttackCollider()
        {
            if (attackCollider != null)
                attackCollider.enabled = true;
        }

        public void DisableAttackCollider()
        {
            if (attackCollider != null)
                attackCollider.enabled = false;
        }
        protected virtual void OnDisable()
        {
            Debug.Log($"Disabling enemy: {name}");
            DisableAttackCollider(); // Ensures no damage after disabling
        }

    }

}
