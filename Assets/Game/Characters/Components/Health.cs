using System;
using UnityEngine;

namespace Game.Characters.Components
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private uint healthAmount = 1;

        public bool IsAlive { get; private set; } = true;
        private uint _totalHealth;

        private void Awake()
        {
            _totalHealth = healthAmount;
        }

        public void TakeDamage(uint damageAmount)
        {
            healthAmount = Math.Clamp(healthAmount - damageAmount, uint.MinValue, _totalHealth);

            if (healthAmount == 0)
                IsAlive = false;
        }

        public void HealUp(uint healAmount)
        {
            if (!IsAlive)
                return;

            healthAmount = Math.Clamp(healthAmount + healAmount, uint.MinValue, _totalHealth);
        }

        public uint GetHealthAmount()
        {
            return healthAmount;
        }
    }
}