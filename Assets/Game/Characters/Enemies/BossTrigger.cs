using UnityEngine;

namespace Game.Characters.Enemies
{
    public class BossTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossHealthBarController healthBarController;
        
        [Header("Settings")]
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private bool useSimpleMode = true; // Use simplified detection
        
        private bool hasTriggered = false;
        
        private void Start()
        {
            // Auto-find health bar controller if not assigned
            if (healthBarController == null)
            {
                healthBarController = FindObjectOfType<BossHealthBarController>();
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if player entered
            if (other.CompareTag("Player"))
            {
                // Check if should trigger only once
                if (triggerOnce && hasTriggered) return;
                
                // Show health bar
                if (healthBarController != null)
                {
                    if (useSimpleMode)
                    {
                        healthBarController.ShowHealthBarSimple();
                    }
                    else
                    {
                        healthBarController.ShowHealthBar();
                    }
                    
                    hasTriggered = true;
                }
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            // Optional: Hide health bar when player leaves
            // Uncomment if needed
            /*
            if (other.CompareTag("Player") && healthBarController != null)
            {
                healthBarController.HideHealthBar();
            }
            */
        }

    }
} 