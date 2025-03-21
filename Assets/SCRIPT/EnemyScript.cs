using UnityEngine;

namespace ClearSky.TutorialEnemy // Different namespace for tutorial enemies
{
    public class TutorialEnemy : MonoBehaviour
    {
        public float health = 50f;
        private bool isDead = false;
        private Animator anim;
        public GameObject healthBar;  // Assign a health bar UI element to this enemy
        public bool isTutorialEnemy = true;

        void Start()
        {
            anim = GetComponent<Animator>();
        }

        void Update()
        {
            if (isDead)
            {
                return; // Stop any other behavior if the enemy is dead
            }
            // You can add other logic here if the enemy has any special behaviors.
        }


    }
}
