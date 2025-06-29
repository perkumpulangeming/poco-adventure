using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Game.Characters.Interfaces;
using Game.Characters.Components;

namespace Game.Characters.Enemies
{
    public class BossHealthBarController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject healthBarPanel;
        [SerializeField] private Text bossNameText; // Optional
        
        [Header("Multi-Boss Settings")]
        [SerializeField] private List<GameObject> bossObjects = new List<GameObject>(); // Multiple bosses
        [SerializeField] private float detectionRadius = 15f;
        [SerializeField] private float lastHitPriorityTime = 3f; // How long last hit priority lasts
        [SerializeField] private float switchDelay = 0.5f; // Delay between switching bosses
        
        [Header("Animation Settings")]
        [SerializeField] private float slideSpeed = 500f;
        [SerializeField] private float slideDelay = 0.5f;
        
        private Transform player;
        private GameObject currentBoss;
        private Health currentBossHealth;
        private uint currentMaxHealth;
        private Image[] heartImages;
        
        private Dictionary<GameObject, float> lastHitTimes = new Dictionary<GameObject, float>();
        private Dictionary<GameObject, uint> bossMaxHealths = new Dictionary<GameObject, uint>();
        
        private bool isVisible = false;
        private bool isAnimating = false;
        private bool isSwitching = false;
        private Vector3 hiddenPosition;
        private Vector3 visiblePosition;
        private RectTransform healthBarRect;
        
        private void Awake()
        {
            SetupHealthBar();
            FindPlayer();
        }
        
        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        private void SetupHealthBar()
        {
            if (healthBarPanel == null) return;
            
            healthBarRect = healthBarPanel.GetComponent<RectTransform>();
            
            // Calculate positions for animation
            visiblePosition = healthBarRect.anchoredPosition;
            hiddenPosition = new Vector3(visiblePosition.x, visiblePosition.y + healthBarRect.rect.height + 50f, visiblePosition.z);
            
            // Start hidden
            healthBarRect.anchoredPosition = hiddenPosition;
            healthBarPanel.SetActive(false);
            
            // Initialize boss health tracking
            InitializeBosses();
            
            // Auto-find heart images
            AutoFindHeartImages();
        }
        
        private void InitializeBosses()
        {
            foreach (GameObject boss in bossObjects)
            {
                if (boss != null)
                {
                    Health bossHealth = boss.GetComponent<Health>();
                    if (bossHealth != null)
                    {
                        uint maxHealth = bossHealth.GetHealthAmount();
                        bossMaxHealths[boss] = maxHealth;
                        lastHitTimes[boss] = 0f;
                        
                        // Subscribe to damage events if possible
                        var damageable = boss.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            // Track when boss takes damage for priority
                            StartCoroutine(TrackBossDamage(boss, bossHealth));
                        }
                    }
                }
            }
        }
        
        private IEnumerator TrackBossDamage(GameObject boss, Health bossHealth)
        {
            uint previousHealth = bossHealth.GetHealthAmount();
            
            while (boss != null && bossHealth != null && bossHealth.IsAlive)
            {
                uint currentHealth = bossHealth.GetHealthAmount();
                
                // Check if boss took damage
                if (currentHealth < previousHealth)
                {
                    lastHitTimes[boss] = Time.time;
                }
                
                previousHealth = currentHealth;
                yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            }
        }
        
        private void AutoFindHeartImages()
        {
            if (healthBarPanel == null) return;
            
            // Find all child images that contain "Heart" in name
            Image[] allImages = healthBarPanel.GetComponentsInChildren<Image>();
            List<Image> hearts = new List<Image>();
            
            foreach (Image img in allImages)
            {
                if (img.name.ToLower().Contains("heart"))
                {
                    hearts.Add(img);
                }
            }
            
            // Sort hearts by name to ensure proper order
            hearts.Sort((a, b) => a.name.CompareTo(b.name));
            
            heartImages = hearts.ToArray();
        }
        
        private void Update()
        {
            if (isVisible && !isSwitching)
            {
                // Determine which boss should have priority
                GameObject priorityBoss = GetPriorityBoss();
                
                // Switch to priority boss if different from current
                if (priorityBoss != currentBoss && priorityBoss != null)
                {
                    StartCoroutine(SwitchToBoss(priorityBoss));
                }
                
                // Update health display for current boss
                if (currentBoss != null && currentBossHealth != null)
                {
                    UpdateHealthDisplay();
                    
                    // Check if current boss is dead
                    if (!currentBossHealth.IsAlive)
                    {
                        // Check if any boss is still alive
                        if (!AnyBossAlive())
                        {
                            StartCoroutine(HideHealthBarDelayed());
                        }
                        else
                        {
                            // Switch to another alive boss
                            GameObject aliveBoss = GetNextAliveBoss();
                            if (aliveBoss != null)
                            {
                                StartCoroutine(SwitchToBoss(aliveBoss));
                            }
                        }
                    }
                }
            }
        }
        
        private GameObject GetPriorityBoss()
        {
            if (player == null) return currentBoss;
            
            GameObject closestBoss = null;
            float closestDistance = float.MaxValue;
            
            foreach (GameObject boss in bossObjects)
            {
                if (boss == null) continue;
                
                Health bossHealth = boss.GetComponent<Health>();
                if (bossHealth == null) continue;
                
                if (!bossHealth.IsAlive) continue;
                
                float distance = Vector3.Distance(player.position, boss.transform.position);
                
                // Check if within detection range
                if (distance <= detectionRadius)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBoss = boss;
                    }
                }
            }
            
            return closestBoss;
        }
        
        private bool AnyBossAlive()
        {
            foreach (GameObject boss in bossObjects)
            {
                if (boss != null)
                {
                    Health bossHealth = boss.GetComponent<Health>();
                    if (bossHealth != null && bossHealth.IsAlive)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        private GameObject GetNextAliveBoss()
        {
            foreach (GameObject boss in bossObjects)
            {
                if (boss != null && boss != currentBoss)
                {
                    Health bossHealth = boss.GetComponent<Health>();
                    if (bossHealth != null && bossHealth.IsAlive)
                    {
                        return boss;
                    }
                }
            }
            return null;
        }
        
        private IEnumerator SwitchToBoss(GameObject newBoss)
        {
            if (isSwitching || newBoss == currentBoss) yield break;
            
            isSwitching = true;
            
            // Hide current health bar
            if (isVisible)
            {
                yield return StartCoroutine(HideHealthBarAnimation());
            }
            
            // Switch to new boss
            currentBoss = newBoss;
            if (currentBoss != null)
            {
                currentBossHealth = currentBoss.GetComponent<Health>();
                if (bossMaxHealths.ContainsKey(currentBoss))
                {
                    currentMaxHealth = bossMaxHealths[currentBoss];
                }
            }
            
            // Wait for switch delay
            yield return new WaitForSeconds(switchDelay);
            
            // Show health bar for new boss
            if (currentBoss != null && currentBossHealth != null && currentBossHealth.IsAlive)
            {
                yield return StartCoroutine(ShowHealthBarAnimation());
            }
            
            isSwitching = false;
        }
        
        private void UpdateHealthDisplay()
        {
            if (heartImages == null || heartImages.Length == 0) return;
            if (currentBossHealth == null) return;
            
            uint currentHealth = currentBossHealth.GetHealthAmount();
            
            // Update hearts based on current health
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    bool shouldBeFull = i < currentHealth;
                    
                    if (shouldBeFull)
                    {
                        // Full heart - white color
                        heartImages[i].color = Color.white;
                    }
                    else
                    {
                        // Empty heart - gray color
                        heartImages[i].color = Color.gray;
                    }
                    
                    heartImages[i].gameObject.SetActive(true);
                }
            }
        }
        
        // Call this method when dialog trigger activates
        public void ShowHealthBar()
        {
            if (isVisible || isAnimating) return;
            
            // Find initial priority boss
            GameObject priorityBoss = GetPriorityBoss();
            if (priorityBoss != null)
            {
                currentBoss = priorityBoss;
                currentBossHealth = currentBoss.GetComponent<Health>();
                if (bossMaxHealths.ContainsKey(currentBoss))
                {
                    currentMaxHealth = bossMaxHealths[currentBoss];
                }
                
                StartCoroutine(ShowHealthBarAnimation());
            }
        }
        
        public void HideHealthBar()
        {
            if (!isVisible || isAnimating) return;
            StartCoroutine(HideHealthBarAnimation());
        }
        
        private IEnumerator ShowHealthBarAnimation()
        {
            isAnimating = true;
            
            if (slideDelay > 0f)
                yield return new WaitForSeconds(slideDelay);
            
            healthBarPanel.SetActive(true);
            healthBarRect.anchoredPosition = hiddenPosition;
            
            if (currentBossHealth != null)
            {
                UpdateHealthDisplay();
                
                if (bossNameText != null)
                {
                    bossNameText.text = currentBoss != null ? currentBoss.name : "Boss";
                }
            }
            
            float elapsedTime = 0f;
            Vector3 startPos = hiddenPosition;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * (slideSpeed / 100f);
                float easedProgress = Mathf.SmoothStep(0f, 1f, elapsedTime);
                healthBarRect.anchoredPosition = Vector3.Lerp(startPos, visiblePosition, easedProgress);
                yield return null;
            }
            
            healthBarRect.anchoredPosition = visiblePosition;
            isVisible = true;
            isAnimating = false;
        }
        
        private IEnumerator HideHealthBarAnimation()
        {
            isAnimating = true;
            
            float elapsedTime = 0f;
            Vector3 startPos = visiblePosition;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * (slideSpeed / 100f);
                float easedProgress = Mathf.SmoothStep(0f, 1f, elapsedTime);
                healthBarRect.anchoredPosition = Vector3.Lerp(startPos, hiddenPosition, easedProgress);
                yield return null;
            }
            
            healthBarRect.anchoredPosition = hiddenPosition;
            healthBarPanel.SetActive(false);
            
            isVisible = false;
            isAnimating = false;
        }
        
        private IEnumerator HideHealthBarDelayed()
        {
            yield return new WaitForSeconds(1f);
            HideHealthBar();
        }
        
        // Public methods
        public void AddBoss(GameObject boss)
        {
            if (!bossObjects.Contains(boss))
            {
                bossObjects.Add(boss);
                Health bossHealth = boss.GetComponent<Health>();
                if (bossHealth != null)
                {
                    bossMaxHealths[boss] = bossHealth.GetHealthAmount();
                    lastHitTimes[boss] = 0f;
                }
            }
        }
        
        public void OnDialogTrigger()
        {
            ShowHealthBar();
        }
        
        public void OnBossDefeat()
        {
            if (!AnyBossAlive())
            {
                HideHealthBar();
            }
        }
        
        // Simplified method to just show health bar for any available boss
        public void ShowHealthBarSimple()
        {
            if (isVisible || isAnimating) return;
            
            // Try to find any alive boss first (ignore distance)
            GameObject anyAliveBoss = null;
            foreach (GameObject boss in bossObjects)
            {
                if (boss != null)
                {
                    Health bossHealth = boss.GetComponent<Health>();
                    if (bossHealth != null && bossHealth.IsAlive)
                    {
                        anyAliveBoss = boss;
                        break;
                    }
                }
            }
            
            if (anyAliveBoss != null)
            {
                currentBoss = anyAliveBoss;
                currentBossHealth = currentBoss.GetComponent<Health>();
                if (bossMaxHealths.ContainsKey(currentBoss))
                {
                    currentMaxHealth = bossMaxHealths[currentBoss];
                }
                
                StartCoroutine(ShowHealthBarAnimation());
            }
        }
    }
} 