using UnityEngine;
using System;

namespace ClearSky.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public int Gold { get; private set; }
        public int Exp { get; set; }
        public float health = 100f;
        public float maxHealth = 100f;
        public float mana = 100f;
        public float maxMana = 100f;
        public float maxExp = 1500f;
        public float manaRegenRate = 1f; // Mana regenerated per second
        public float CurrentHealth { get; set; }
        public float CurrentMana { get; set; }

        public event Action OnGoldChanged;
        public event Action OnExpChanged;
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;

        public PlayerStatsBarBehaviour statsBar;

        private void Start()
        {
            statsBar.SetHealth(health, maxHealth);
            statsBar.SetMana(mana, maxMana);
            statsBar.SetExp(Exp, maxExp);
        }

        private void Update()
        {
            RegenerateMana(Time.deltaTime);
        }

        public void ModifyHealth(float amount)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);
            Debug.Log($"Health modified: {health}/{maxHealth}");
            statsBar.SetHealth(health, maxHealth);
            OnHealthChanged?.Invoke(health, maxHealth);
            if (health <= 0) Die();
        }

        public void ModifyMana(float amount)
        {
            mana = Mathf.Clamp(mana + amount, 0, maxMana);
            statsBar.SetMana(mana, maxMana);
            OnManaChanged?.Invoke(mana, maxMana);
        }

        private void RegenerateMana(float deltaTime)
        {
            ModifyMana(deltaTime * manaRegenRate);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke();
        }

        public void AddExp(int amount)
        {
            Exp += amount;
            statsBar.SetExp(Exp, maxExp);
            OnExpChanged?.Invoke();

            if (Exp >= maxExp) LevelUp();
        }

        private void LevelUp()
        {
            Exp = 0;
            maxExp += 100;
            health = maxHealth; // Full heal on level-up
            mana = maxMana; // Full mana on level-up
        }

        private void Die()
        {
            Debug.Log("Player has died!");
            // Additional death logic here
        }
    }
}
