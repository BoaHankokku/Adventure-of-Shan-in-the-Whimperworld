using ClearSky.Player;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ClearSky.Enemy;
using UnityEngine.UIElements;
using TMPro;

namespace ClearSky.Controller
{
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement and Jumping")]
        public float movePower = 10f;
        public float jumpPower = 15f;
        public JoystickScript joystick;

        private Rigidbody2D rb;
        private Animator anim;
        private bool isJumping = false;
        private bool isGrounded = false;
        private GameObject currentSkill;

        [Header("Skills")]
        public GameObject skill1, skill2, skill3, skill4;
        public UnityEngine.UI.Button skill1Button, skill2Button, skill3Button, skill4Button;
        public UnityEngine.UI.Button healthPotionButton, manaPotionButton, jumpButton;
        public BoxCollider2D playerCollider;

        [Header("Player Stats & UI")]
        public PlayerStats playerStats;
        public TextMeshProUGUI healthPotionCountText;
        public TextMeshProUGUI manaPotionCountText;
        private int healthPotionCount;
        private int manaPotionCount;

        public bool canMoveLeft = true;
        public bool canMoveRight = true;

        public float skill1Damage = 10f;
        public float skillCooldown = 0.5f;
        private bool canAttack = true;
        private bool isInvulnerable = false;

        // Track purchased skills
        public bool Skill2Unlocked = false;
        public bool Skill3Unlocked = false;
        public bool Skill4Unlocked = false;

       

        public enum SkillType { Skill1, Skill2, Skill3, Skill4 }

        private void Start()
        {
            LoadPlayerData();

            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError("Animator component not found!");
            }

            // Hide all skill buttons initially
            skill2Button.gameObject.SetActive(false);
            skill3Button.gameObject.SetActive(false);
            skill4Button.gameObject.SetActive(false);


            // Set up button listeners for skill activation
            skill1Button.onClick.AddListener(() => OnSkillButtonClicked(SkillType.Skill1));
            skill2Button.onClick.AddListener(() => OnSkillButtonClicked(SkillType.Skill2));
            skill3Button.onClick.AddListener(() => OnSkillButtonClicked(SkillType.Skill3));
            skill4Button.onClick.AddListener(() => OnSkillButtonClicked(SkillType.Skill4));

            // Attach jump button listener
            jumpButton.onClick.AddListener(OnJumpButtonClicked);

            //POTIONS
            healthPotionCount = GameDataManager.Instance.GetHealthPotionCount();
            manaPotionCount = GameDataManager.Instance.GetManaPotionCount();
            healthPotionButton.onClick.AddListener(() =>
            {
                Debug.Log("Health Potion Button Clicked");
                UseHealthPotion();
            });

            manaPotionButton.onClick.AddListener(() =>
            {
                Debug.Log("Mana Potion Button Clicked");
                UseManaPotion();
            });
            UpdatePotionUI();
            // Initialize player stats UI, if available
            if (playerStats != null)
            {
                playerStats.statsBar.SetHealth(playerStats.health, playerStats.maxHealth);
                playerStats.statsBar.SetMana(playerStats.mana, playerStats.maxMana);
                playerStats.statsBar.SetExp(playerStats.Exp, playerStats.maxExp);
            }
            else
            {
                Debug.LogError("PlayerStats is not assigned!");
            }

            DisableAllSkills();
            LoadUnlockedSkills();
        }

        private void Run()
        {
            Vector3 moveVelocity = Vector3.zero;
            Vector2 rayDirection = Vector2.zero;
            Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y);
            float rayDistance = 0.5f;
            bool isMoving = false;

            // Determine movement direction
            if (Input.GetAxisRaw("Horizontal") < 0) // Move left
            {
                moveVelocity = Vector3.left;
                rayDirection = Vector2.left;
                transform.localScale = new Vector3(-1, 1, 1);
                isMoving = true;
            }
            else if (Input.GetAxisRaw("Horizontal") > 0) // Move right
            {
                moveVelocity = Vector3.right;
                rayDirection = Vector2.right;
                transform.localScale = new Vector3(1, 1, 1);
                isMoving = true;
            }

            // Visualize raycast in Scene view
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.blue);

            // Perform raycast to check for walls
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Wall"));

            if (hit.collider == null) // No wall detected
            {
                // Apply movement only if boundaries allow it
                Vector3 newPosition = transform.position + (moveVelocity * movePower * Time.deltaTime);

                if (PlatformBoundaryController.Instance != null)
                {
                    // Restrict movement based on boundaries
                    PlatformBoundaryController.Instance.RestrictPlayerWithinBoundaries();

                    // If player is clamped, stop movement but keep animation active
                    if (PlatformBoundaryController.Instance.IsPlayerClamped)
                    {
                        Debug.Log("Player is clamped at the boundary.");
                        isMoving = true; // Keep animation running
                    }
                    else
                    {
                        transform.position = newPosition;
                    }
                }
                else
                {
                    transform.position = newPosition;
                }
            }
            else
            {
                Debug.Log("Wall detected! Blocking movement.");
                isMoving = false; // Stop movement if hitting a wall
            }

            // Update animation state
            anim.SetBool("isRun", isMoving);
            Debug.Log($"isRun set to: {isMoving}");
        }

        private void OnJumpButtonClicked()
        {
            Debug.Log("Jump Button Pressed. IsGrounded: " + isGrounded + ", IsJumping: " + isJumping);

            if (isGrounded && !isJumping)
            {
                Jump();
            }
            else
            {
                Debug.Log("Jump prevented. IsGrounded: " + isGrounded + ", IsJumping: " + isJumping);
            }
        }


        void Jump()
        {
            if (isGrounded && !isJumping)
            {
                isJumping = true;
                isGrounded = false;
                anim.SetBool("isJump", true); // Starts the jump animation
                rb.velocity = Vector2.zero;
                Vector2 jumpVelocity = new Vector2(0, jumpPower);
                rb.AddForce(jumpVelocity, ForceMode2D.Impulse);
                Debug.Log("Jump animation triggered.");
            }
        }

        public void OnSkillButtonClicked(SkillType skillType)
        {
            float damage = 0f;
            float manaCost = 0f;
            float cooldown = 0f;
            string trigger = "";

            switch (skillType)
            {
                case SkillType.Skill1:
                    damage = skill1Damage;
                    manaCost = 1f;
                    cooldown = skillCooldown;
                    trigger = "skill1_attack";
                    currentSkill = skill1;
                    break;

                case SkillType.Skill2:
                    if (!Skill2Unlocked) return; // Ensure Skill 2 is unlocked
                    damage = 20f;
                    manaCost = 10f;
                    cooldown = 1f;
                    trigger = "skill2_attack";
                    currentSkill = skill2;
                    break;

                case SkillType.Skill3:
                    if (!Skill3Unlocked) return; // Ensure Skill 3 is unlocked
                    damage = 30f;
                    manaCost = 15f;
                    cooldown = 1.5f;
                    trigger = "skill3_attack";
                    currentSkill = skill3;
                    break;

                case SkillType.Skill4:
                    if (!Skill4Unlocked) return; // Ensure Skill 4 is unlocked
                    damage = 40f;
                    manaCost = 20f;
                    cooldown = 2f;
                    trigger = "skill4_attack";
                    currentSkill = skill4;
                    break;

                default:
                    return;
            }

            if (playerStats.mana < manaCost) return;

            playerStats.ModifyMana(-manaCost);
            EnableSkillCollider();
            currentSkill.SetActive(true);
            anim.SetTrigger(trigger);
            AttackEnemies(damage);
            StartCoroutine(ResetAttackCooldown(cooldown));
            StartCoroutine(DisableSkillAfterEnemyHit(currentSkill));
        }

        private IEnumerator ResetAttackCooldown(float cooldown)
        {
            canAttack = false;
            yield return new WaitForSeconds(cooldown);
            canAttack = true;
        }

        private void AttackEnemies(float damage)
        {
            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("Enemy"));
            foreach (Collider2D enemy in enemiesHit)
            {
                EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
                if (enemyScript != null && !enemyScript.IsDead)
                {
                    enemyScript.TakeDamage(skill1Damage, currentSkill);
                    Debug.Log($"Damaged enemy: {enemy.name}");
                }
            }
        }

        private IEnumerator DisableSkillAfterEnemyHit(GameObject skill)
        {
            while (true)
            {
                Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("Enemy"));
                if (enemiesHit.Length > 0)
                {
                    DisableSkillCollider();
                    skill.SetActive(false);
                    anim.SetBool("isRun", false);
                    break;
                }
                yield return null;
            }
        }

        public void TakeDamage(float damage)
        {
            playerStats?.ModifyHealth(-damage);
            anim.SetTrigger("hurt");

            if (playerStats.health <= 0)
            {
                Debug.Log("Player has died!");
            }
        }
        private void EnableSkillCollider()
        {
            Collider2D skillCollider = currentSkill?.GetComponent<Collider2D>();
            if (skillCollider != null) skillCollider.enabled = true;
        }

        private void DisableSkillCollider()
        {
            Collider2D skillCollider = currentSkill?.GetComponent<Collider2D>();
            if (skillCollider != null) skillCollider.enabled = false;
        }
        private void DisableAllSkills()
        {
            skill1.SetActive(false);
            skill2.SetActive(false);
            skill3.SetActive(false);
            skill4.SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision Entered with: " + collision.gameObject.name); // Log the name of the colliding object

            if (collision.gameObject.CompareTag("Ground")) // Check if the collided object has the tag "Ground"
            {
                isGrounded = true; // Set grounded status to true
                isJumping = false; // Reset jumping status
                anim.SetBool("isJump", false); // Stop jump animation
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Debug.Log("Collision Exited with: " + collision.gameObject.name); // Log when exiting collision

            if (collision.gameObject.CompareTag("Ground")) // Check if the collided object has the tag "Ground"
            {
                Debug.Log("Player left the ground."); // Log message when the player leaves the ground
                isGrounded = false; // Set grounded status to false
            }
        }
        public void UnlockSkill(int skillID)
        {
            switch (skillID)
            {
                case 2:
                    Skill2Unlocked = true;
                    GameDataManager.Instance.IsSkill2Unlocked = true;
                    skill2Button.gameObject.SetActive(true);
                    Debug.Log("Skill 2 (Ice Frost) unlocked.");
                    break;

                case 3:
                    Skill3Unlocked = true;
                    GameDataManager.Instance.IsSkill3Unlocked = true;
                    skill3Button.gameObject.SetActive(true);
                    Debug.Log("Skill 3 unlocked after finishing Stage 1.");
                    break;

                case 4:
                    Skill4Unlocked = true;
                    GameDataManager.Instance.IsSkill4Unlocked = true;
                    skill4Button.gameObject.SetActive(true);
                    Debug.Log("Skill 4 unlocked after finishing Stage 2.");
                    break;

                default:
                    Debug.LogWarning($"Invalid skill index {skillID}");
                    break;
            }

            UpdateSkillButtons();
            GameDataManager.Instance.SavePlayerStats();
        }
        private void UpdateSkillButtons()
        {
            skill2Button.interactable = Skill2Unlocked;
            skill3Button.interactable = Skill3Unlocked;
            skill4Button.interactable = Skill4Unlocked;
        }
        public void DisablePlayerInput()
        {
            joystick.gameObject.SetActive(false);
            skill1Button.gameObject.SetActive(false);
            skill2Button.gameObject.SetActive(false);
            skill3Button.gameObject.SetActive(false);
            skill4Button.gameObject.SetActive(false);
            healthPotionButton.gameObject.SetActive(false);
            manaPotionButton.gameObject.SetActive(false);
            jumpButton.gameObject.SetActive(false);
        }

        public void EnablePlayerInput()
        {
            joystick.gameObject.SetActive(true);
            skill1Button.gameObject.SetActive(true);
            skill2Button.gameObject.SetActive(Skill2Unlocked);
            skill3Button.gameObject.SetActive(Skill3Unlocked);
            skill4Button.gameObject.SetActive(Skill4Unlocked);
            healthPotionButton.gameObject.SetActive(true);
            manaPotionButton.gameObject.SetActive(true);
            jumpButton.gameObject.SetActive(true);
        }


        private void CheckGrounded()
        {
            float rayLength = 0.5f;
            Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y - playerCollider.bounds.extents.y);
            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                isGrounded = true;
                Debug.Log("Player is grounded by CheckGrounded.");
            }
            else
            {
                isGrounded = false;
                Debug.Log("Player is not grounded by CheckGrounded.");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"Player OnTriggerEnter2D called with {collision.name}");
            anim.SetBool("isJump", false);

            // Only interact with EnemySkill layer for applying damage to the player
            if (collision.gameObject.layer == LayerMask.NameToLayer("EnemySkill"))
            {
                var enemyBase = collision.gameObject.GetComponentInParent<EnemyBase>();
                if (enemyBase != null)
                {
                    Debug.Log($"Taking damage from enemy: {enemyBase.name}");
                    TakeDamage(enemyBase.damage); // Unified damage handling
                    anim.SetTrigger("hurt");
                    DisableSkillCollider();
                    currentSkill?.SetActive(false);
                }
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skill1Damage, currentSkill);
                    DisableSkillCollider();
                    currentSkill.SetActive(false);
                }
            }
            else if (collision.CompareTag("Barrel"))
            {
                Debug.Log($"Player collided with a Barrel: {collision.name}");
                if (BaseGameManager.Instance is GameManager2 gm2)
                {
                    gm2.HandleBarrelCollision(collision, gm2.barrelColliders);
                }
                else
                {
                    Debug.LogWarning("BaseGameManager is not GameManager2. HandleBarrelCollision ignored.");
                }
            }
        }

        private void DebugSkillButtonStates()
        {
            Debug.Log($"Skill 2 Unlocked: {Skill2Unlocked}, Button Active: {skill2Button.gameObject.activeSelf}");
            Debug.Log($"Skill 3 Unlocked: {Skill3Unlocked}, Button Active: {skill3Button.gameObject.activeSelf}");
            Debug.Log($"Skill 4 Unlocked: {Skill4Unlocked}, Button Active: {skill4Button.gameObject.activeSelf}");
        }

        public void GainExp(int amount)
        {
            if (playerStats != null)
            {
                playerStats.AddExp(amount);
            }
            else
            {
                Debug.LogError("PlayerStats is not assigned.");
            }
        }
        public void DisableCurrentSkill()
        {
            if (currentSkill != null)
            {
                DisableSkillCollider();
                currentSkill.SetActive(false);
            }
        }
        private void SavePlayerPosition()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SavePlayerTransform(transform);
                Debug.Log("Player position saved through SimplePlayerController.");
            }
        }

        private void LoadPlayerPosition()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.LoadPlayerTransform(transform);
                Debug.Log("Player position loaded through SimplePlayerController.");
            }
        }
        private void SavePlayerData()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SavePlayerStats();
                GameDataManager.Instance.SavePlayerTransform(transform);
            }
        }

        private void LoadPlayerData()
        {
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.LoadPlayerTransform(transform); // Load player position
                if (PlatformBoundaryController.Instance != null)
                {
                    PlatformBoundaryController.Instance.ValidatePlayerPositionAfterLoad(); // Validate boundaries
                }
                Debug.Log("Player data loaded and position validated.");
            }
            else
            {
                Debug.LogError("GameDataManager instance is null.");
            }
        }
        public void LoadUnlockedSkills()
        {
            Skill2Unlocked = GameDataManager.Instance.IsSkillUnlocked(2);
            Skill3Unlocked = GameDataManager.Instance.IsSkillUnlocked(3);
            Skill4Unlocked = GameDataManager.Instance.IsSkillUnlocked(4);

            // Ensure buttons are active based on unlocked state
            skill2Button.gameObject.SetActive(Skill2Unlocked);
            skill3Button.gameObject.SetActive(Skill3Unlocked);
            skill4Button.gameObject.SetActive(Skill4Unlocked);

            UpdateSkillButtons(); // Update interactable state
        }

        //POTIONS:
        public void UseHealthPotion()
        {
            if (healthPotionCount > 0 && playerStats != null)
            {
                healthPotionCount--;
                playerStats.ModifyHealth(50); // Add health points
                GameDataManager.Instance.SetHealthPotionCount(healthPotionCount); // Sync with GameDataManager
                UpdatePotionUI();

                Debug.Log("Used a health potion.");
            }
            else
            {
                Debug.LogWarning("No health potions left or playerStats is null.");
            }
        }

        public void UseManaPotion()
        {
            if (manaPotionCount > 0 && playerStats != null)
            {
                manaPotionCount--;
                playerStats.ModifyMana(50); // Add mana points
                GameDataManager.Instance.SetManaPotionCount(manaPotionCount); // Sync with GameDataManager
                UpdatePotionUI();

                Debug.Log("Used a mana potion.");
            }
            else
            {
                Debug.LogWarning("No mana potions left or playerStats is null.");
            }
        }

        public void UpdatePotionUI()
        {
            // Fetch values directly from GameDataManager
            healthPotionCount = GameDataManager.Instance.GetHealthPotionCount();
            manaPotionCount = GameDataManager.Instance.GetManaPotionCount();

            if (healthPotionCountText != null)
            {
                healthPotionCountText.text = healthPotionCount.ToString();
            }

            if (manaPotionCountText != null)
            {
                manaPotionCountText.text = manaPotionCount.ToString();
            }

        }


    }
}