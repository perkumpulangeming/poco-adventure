using UnityEngine;

namespace Game.Characters.Components
{
    public class Health : MonoBehaviour
    {
        public int maxHealth = 3;
        public int currentHealth = 3;

        public bool IsAlive => currentHealth > 0;

        public void TakeDamage(uint damage)
        {
            currentHealth -= (int)damage;
            currentHealth = Mathf.Max(currentHealth, 0);
        }

        public void Heal(uint amount)
        {
            currentHealth += (int)amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public void HealUp(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public int GetHealthAmount()
        {
            return currentHealth;
        }
    }
}
