using UnityEngine;
using Game.Characters.Components;
using Game.Characters.Player;
using Game.Characters.Player.UI;
using Game;
using System.Collections;

public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool autoFindPlayer = true;
    
    [Header("Upgrade Settings")]
    [SerializeField] private uint baseHealth = 3;
    [SerializeField] private float baseAttackRange = 1f;
    [SerializeField] private uint healthPerUpgrade = 1;
    [SerializeField] private float rangePerUpgrade = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Player references
    private GameObject playerObject;
    private Health playerHealth;
    private PlayerController playerController;
    private PlayerUIController playerUIController;
    
    // Current applied upgrades
    private uint appliedHealthUpgrades = 0;
    private uint appliedRangeUpgrades = 0;
    
    // Singleton pattern
    public static PlayerUpgradeManager Instance { get; private set; }
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Subscribe to shop events
        ShopPurchaseManager.OnHealthUpgraded += OnHealthUpgraded;
        ShopPurchaseManager.OnRangeUpgraded += OnRangeUpgraded;
        
        // Find player and apply existing upgrades
        StartCoroutine(InitializePlayerUpgrades());
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        ShopPurchaseManager.OnHealthUpgraded -= OnHealthUpgraded;
        ShopPurchaseManager.OnRangeUpgraded -= OnRangeUpgraded;
        
        // Clear singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #endregion
    
    #region Player Detection & Setup
    
    private IEnumerator InitializePlayerUpgrades()
    {
        // Wait a frame for everything to initialize
        yield return null;
        
        if (autoFindPlayer)
        {
            FindPlayer();
        }
        
        if (playerObject != null)
        {
            GetPlayerComponents();
            ApplyStoredUpgrades();
        }
        else
        {
            LogDebug("Player not found! Make sure player exists in scene.");
        }
    }
    
    private void FindPlayer()
    {
        // Try to find by tag first
        if (!string.IsNullOrEmpty(playerTag))
        {
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
        }
        
        // If not found by tag, try to find PlayerController
        if (playerObject == null)
        {
            PlayerController controller = FindObjectOfType<PlayerController>();
            if (controller != null)
            {
                playerObject = controller.gameObject;
            }
        }
        
        // If still not found, try to find by name
        if (playerObject == null)
        {
            playerObject = GameObject.Find("Player");
        }
        
        LogDebug($"Player detection: {(playerObject != null ? "Found - " + playerObject.name : "Not found")}");
    }
    
    private void GetPlayerComponents()
    {
        if (playerObject == null) return;
        
        // Get Health component
        playerHealth = playerObject.GetComponent<Health>();
        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponentInChildren<Health>();
        }
        
        // Get PlayerController component
        playerController = playerObject.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = playerObject.GetComponentInChildren<PlayerController>();
        }
        
        // Get PlayerUIController component
        playerUIController = FindObjectOfType<PlayerUIController>();
        
        LogDebug($"Components found - Health: {playerHealth != null}, PlayerController: {playerController != null}, PlayerUIController: {playerUIController != null}");
    }
    
    #endregion
    
    #region Upgrade Application
    
    private void ApplyStoredUpgrades()
    {
        // Get stored upgrade counts from ShopPurchaseManager or PlayerPrefs
        uint storedHealthUpgrades = GetStoredHealthUpgrades();
        uint storedRangeUpgrades = GetStoredRangeUpgrades();
        
        LogDebug($"Applying stored upgrades - Health: {storedHealthUpgrades}, Range: {storedRangeUpgrades}");
        
        // Apply health upgrades
        if (storedHealthUpgrades > 0)
        {
            uint newMaxHealth = baseHealth + (storedHealthUpgrades * healthPerUpgrade);
            ApplyHealthUpgrade(newMaxHealth);
            appliedHealthUpgrades = storedHealthUpgrades;
        }
        
        // Apply range upgrades
        if (storedRangeUpgrades > 0)
        {
            float newAttackRange = baseAttackRange + (storedRangeUpgrades * rangePerUpgrade);
            ApplyRangeUpgrade(newAttackRange);
            appliedRangeUpgrades = storedRangeUpgrades;
        }
    }
    
    private void ApplyHealthUpgrade(uint newMaxHealth)
    {
        if (playerHealth == null)
        {
            LogDebug("Cannot apply health upgrade - Health component not found!");
            return;
        }
        
        // Use reflection to set the private _totalHealth field
        var totalHealthField = typeof(Health).GetField("_totalHealth", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (totalHealthField != null)
        {
            totalHealthField.SetValue(playerHealth, newMaxHealth);
            LogDebug($"Health upgrade applied - New max health: {newMaxHealth}");
        }
        
        // Also try to set current health to max if needed
        var healthAmountField = typeof(Health).GetField("healthAmount", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (healthAmountField != null)
        {
            uint currentHealth = playerHealth.GetHealthAmount();
            if (currentHealth < newMaxHealth)
            {
                healthAmountField.SetValue(playerHealth, newMaxHealth);
                LogDebug($"Current health restored to max: {newMaxHealth}");
            }
        }
        
        // Refresh heart visual system if UI controller exists
        if (playerUIController != null)
        {
            playerUIController.ForceRefreshHeartSystem();
            LogDebug("Heart visual system refreshed!");
        }
    }
    
    private void ApplyRangeUpgrade(float newAttackRange)
    {
        if (playerController == null)
        {
            LogDebug("Cannot apply range upgrade - PlayerController not found!");
            return;
        }
        
        // Try to find ShootingSystem component specifically
        var shootingSystem = playerController.GetComponent<ShootingSystem>();
        if (shootingSystem != null)
        {
            var shootRangeField = typeof(ShootingSystem).GetField("shootRange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (shootRangeField != null)
            {
                shootRangeField.SetValue(shootingSystem, newAttackRange);
                LogDebug($"Shooting range upgrade applied - New range: {newAttackRange}");
                return;
            }
        }
        
        // Try to find and update attack range in player controller or weapon system
        // This depends on your specific implementation
        
        // Method 1: Direct field access (if PlayerController has attackRange field)
        var attackRangeField = typeof(PlayerController).GetField("attackRange", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (attackRangeField != null)
        {
            attackRangeField.SetValue(playerController, newAttackRange);
            LogDebug($"Attack range upgrade applied - New range: {newAttackRange}");
            return;
        }
        
        // Method 2: Try to find weapon component
        var weaponComponent = playerController.GetComponentInChildren<MonoBehaviour>();
        if (weaponComponent != null)
        {
            var weaponRangeField = weaponComponent.GetType().GetField("attackRange", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (weaponRangeField != null)
            {
                weaponRangeField.SetValue(weaponComponent, newAttackRange);
                LogDebug($"Weapon range upgrade applied - New range: {newAttackRange}");
                return;
            }
        }
        
        LogDebug($"Range upgrade stored but not applied - No compatible field found. New range would be: {newAttackRange}");
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnHealthUpgraded(uint newMaxHealth)
    {
        LogDebug($"Shop health upgrade received - New max health: {newMaxHealth}");
        
        if (playerObject == null)
        {
            FindPlayer();
            GetPlayerComponents();
        }
        
        ApplyHealthUpgrade(newMaxHealth);
        appliedHealthUpgrades++;
    }
    
    private void OnRangeUpgraded(float newAttackRange)
    {
        LogDebug($"Shop range upgrade received - New attack range: {newAttackRange}");
        
        if (playerObject == null)
        {
            FindPlayer();
            GetPlayerComponents();
        }
        
        ApplyRangeUpgrade(newAttackRange);
        appliedRangeUpgrades++;
    }
    
    #endregion
    
    #region Data Management
    
    private uint GetStoredHealthUpgrades()
    {
        return (uint)PlayerPrefs.GetInt("HealthUpgrades", 0);
    }
    
    private uint GetStoredRangeUpgrades()
    {
        return (uint)PlayerPrefs.GetInt("RangeUpgrades", 0);
    }
    
    #endregion
    
    #region Public Methods
    
    public void ForceRefreshPlayer()
    {
        FindPlayer();
        GetPlayerComponents();
        ApplyStoredUpgrades();
    }
    
    public uint GetAppliedHealthUpgrades() => appliedHealthUpgrades;
    public uint GetAppliedRangeUpgrades() => appliedRangeUpgrades;
    
    public bool IsPlayerFound() => playerObject != null;
    public bool HasHealthComponent() => playerHealth != null;
    public bool HasPlayerController() => playerController != null;
    public bool HasPlayerUIController() => playerUIController != null;
    
    #endregion
    
    #region Utility
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerUpgradeManager] {message}");
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Debug: Find Player")]
    public void DebugFindPlayer()
    {
        FindPlayer();
        GetPlayerComponents();
        LogDebug($"Debug find player - Found: {playerObject != null}");
    }
    
    [ContextMenu("Debug: Apply Test Health Upgrade")]
    public void DebugTestHealthUpgrade()
    {
        uint testHealth = baseHealth + 2;
        ApplyHealthUpgrade(testHealth);
        LogDebug($"Debug applied health upgrade to: {testHealth}");
    }
    
    [ContextMenu("Debug: Apply Test Range Upgrade")]
    public void DebugTestRangeUpgrade()
    {
        float testRange = baseAttackRange + 1f;
        ApplyRangeUpgrade(testRange);
        LogDebug($"Debug applied range upgrade to: {testRange}");
    }
    
    [ContextMenu("Debug: Show Current Status")]
    public void DebugShowStatus()
    {
        LogDebug($"=== Player Upgrade Manager Status ===\n" +
                $"Player Found: {IsPlayerFound()}\n" +
                $"Health Component: {HasHealthComponent()}\n" +
                $"PlayerController: {HasPlayerController()}\n" +
                $"PlayerUIController: {HasPlayerUIController()}\n" +
                $"Applied Health Upgrades: {appliedHealthUpgrades}\n" +
                $"Applied Range Upgrades: {appliedRangeUpgrades}\n" +
                $"Current Health: {(playerHealth != null ? playerHealth.GetHealthAmount().ToString() : "N/A")}");
    }
    
    #endregion
} 