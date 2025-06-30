using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Game;

public class ShopPurchaseManager : MonoBehaviour
{
    [Header("Shop Items UI")]
    [SerializeField] private Image healthUpgradeButton;
    [SerializeField] private Image rangeUpgradeButton;
    [SerializeField] private TextMeshProUGUI healthPriceText;
    [SerializeField] private TextMeshProUGUI rangePriceText;
    
    [Header("Auto-Detected Progress Bars")]
    [SerializeField] private List<Image> healthBars = new List<Image>();
    [SerializeField] private List<Image> rangeBars = new List<Image>();
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeBarColor = Color.white;
    [SerializeField] private Color inactiveBarColor = Color.black;
    [SerializeField] private Color enabledButtonColor = Color.white;
    [SerializeField] private Color disabledButtonColor = Color.gray;
    [SerializeField] private Color clickedButtonColor = Color.black; // Dark color when clicked
    [SerializeField] private float disabledButtonAlpha = 0.5f;
    [SerializeField] private float clickEffectDuration = 0.1f;
    
    [Header("Upgrade Settings")]
    [SerializeField] private uint basePrice = 50;
    [SerializeField] private float priceMultiplier = 1.5f;
    [SerializeField] private uint healthUpgradeAmount = 1;
    [SerializeField] private float rangeUpgradeAmount = 0.5f;
    [SerializeField] private uint maxUpgrades = 6;
    
    [Header("Auto-Detection Settings")]
    [SerializeField] private string healthBarPrefix = "HpBar";
    [SerializeField] private string rangeBarPrefix = "WeaponBar";
    
    [Header("PlayerPrefs Keys")]
    [SerializeField] private string healthUpgradeCountKey = "HealthUpgrades";
    [SerializeField] private string rangeUpgradeCountKey = "RangeUpgrades";
    [SerializeField] private string maxHealthKey = "MaxHealth";
    [SerializeField] private string attackRangeKey = "AttackRange";
    
    // Events
    public static event Action<uint> OnHealthUpgraded;
    public static event Action<float> OnRangeUpgraded;
    public static event Action<string, uint> OnItemPurchased;
    
    // Current upgrade counts
    private uint healthUpgradeCount = 0;
    private uint rangeUpgradeCount = 0;
    
    // Current player stats
    private uint currentMaxHealth = 3;
    private float currentAttackRange = 1f;
    
    // Button click state tracking
    private Dictionary<Image, bool> buttonPressStates = new Dictionary<Image, bool>();
    
    #region Unity Lifecycle
    
    private void Start()
    {
        AutoDetectProgressBars();
        LoadUpgradeData();
        SetupButtons();
        UpdatePriceDisplays();
        UpdateProgressBars();
    }
    
    private void Update()
    {
        UpdateButtonStates();
    }
    
    private void OnEnable()
    {
        if (ShopGemManager.Instance != null)
        {
            ShopGemManager.OnGemCountChanged += OnGemCountChanged;
        }
    }
    
    private void OnDisable()
    {
        if (ShopGemManager.Instance != null)
        {
            ShopGemManager.OnGemCountChanged -= OnGemCountChanged;
        }
    }
    
    #endregion
    
    #region Auto-Detection
    
    private void AutoDetectProgressBars()
    {
        healthBars.Clear();
        rangeBars.Clear();
        
        // Auto-detect health bars (HpBar1, HpBar2, etc.)
        for (int i = 1; i <= maxUpgrades; i++)
        {
            string healthBarName = healthBarPrefix + i;
            GameObject healthBarObj = GameObject.Find(healthBarName);
            
            if (healthBarObj != null)
            {
                Image healthBar = healthBarObj.GetComponent<Image>();
                if (healthBar != null)
                {
                    healthBars.Add(healthBar);
                    Debug.Log($"Auto-detected health bar: {healthBarName}");
                }
            }
        }
        
        // Auto-detect range bars (WeaponBar1, WeaponBar2, etc.)
        for (int i = 1; i <= maxUpgrades; i++)
        {
            string rangeBarName = rangeBarPrefix + i;
            GameObject rangeBarObj = GameObject.Find(rangeBarName);
            
            if (rangeBarObj != null)
            {
                Image rangeBar = rangeBarObj.GetComponent<Image>();
                if (rangeBar != null)
                {
                    rangeBars.Add(rangeBar);
                    Debug.Log($"Auto-detected range bar: {rangeBarName}");
                }
            }
        }
        
        Debug.Log($"Auto-detection complete: {healthBars.Count} health bars, {rangeBars.Count} range bars found");
    }
    
    #endregion
    
    #region Data Management
    
    private void LoadUpgradeData()
    {
        healthUpgradeCount = (uint)PlayerPrefs.GetInt(healthUpgradeCountKey, 0);
        rangeUpgradeCount = (uint)PlayerPrefs.GetInt(rangeUpgradeCountKey, 0);
        currentMaxHealth = (uint)PlayerPrefs.GetInt(maxHealthKey, 3);
        currentAttackRange = PlayerPrefs.GetFloat(attackRangeKey, 1f);
        
        Debug.Log($"Loaded upgrades - Health: {healthUpgradeCount}, Range: {rangeUpgradeCount}");
    }
    
    private void SaveUpgradeData()
    {
        PlayerPrefs.SetInt(healthUpgradeCountKey, (int)healthUpgradeCount);
        PlayerPrefs.SetInt(rangeUpgradeCountKey, (int)rangeUpgradeCount);
        PlayerPrefs.SetInt(maxHealthKey, (int)currentMaxHealth);
        PlayerPrefs.SetFloat(attackRangeKey, currentAttackRange);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Button Setup & Effects
    
    private void SetupButtons()
    {
        SetupButtonWithEffects(healthUpgradeButton, PurchaseHealthUpgrade);
        SetupButtonWithEffects(rangeUpgradeButton, PurchaseRangeUpgrade);
    }
    
    private void SetupButtonWithEffects(Image button, System.Action clickAction)
    {
        if (button == null) return;
        
        // Initialize button state
        buttonPressStates[button] = false;
        
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        trigger.triggers.Clear();
        
        // Pointer Down - Make darker
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { 
            if (CanInteractWithButton(button))
            {
                buttonPressStates[button] = true;
                ApplyClickEffect(button, true);
            }
        });
        trigger.triggers.Add(pointerDown);
        
        // Pointer Up - Return to normal and execute action
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { 
            if (buttonPressStates.ContainsKey(button) && buttonPressStates[button] && CanInteractWithButton(button))
            {
                ApplyClickEffect(button, false);
                buttonPressStates[button] = false;
                
                // Delay click action slightly for visual feedback
                StartCoroutine(DelayedAction(clickAction, clickEffectDuration));
            }
        });
        trigger.triggers.Add(pointerUp);
        
        // Pointer Exit - Cancel press if pointer leaves
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { 
            if (buttonPressStates.ContainsKey(button) && buttonPressStates[button])
            {
                ApplyClickEffect(button, false);
                buttonPressStates[button] = false;
            }
        });
        trigger.triggers.Add(pointerExit);
    }
    
    private System.Collections.IEnumerator DelayedAction(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    
    private void ApplyClickEffect(Image button, bool pressed)
    {
        if (button == null) return;
        
        Color targetColor = pressed ? clickedButtonColor : enabledButtonColor;
        button.color = targetColor;
    }
    
    private bool CanInteractWithButton(Image button)
    {
        return button != null && button.raycastTarget;
    }
    
    private bool IsButtonPressed(Image button)
    {
        return buttonPressStates.ContainsKey(button) && buttonPressStates[button];
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdatePriceDisplays()
    {
        uint healthPrice = CalculatePrice(healthUpgradeCount);
        uint rangePrice = CalculatePrice(rangeUpgradeCount);
        
        if (healthPriceText != null)
        {
            healthPriceText.text = healthPrice.ToString();
        }
        
        if (rangePriceText != null)
        {
            rangePriceText.text = rangePrice.ToString();
        }
        
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        if (ShopGemManager.Instance == null) return;
        
        uint currentGems = ShopGemManager.Instance.GetCurrentGemCount();
        uint healthPrice = CalculatePrice(healthUpgradeCount);
        uint rangePrice = CalculatePrice(rangeUpgradeCount);
        
        // Update health button
        if (healthUpgradeButton != null && !IsButtonPressed(healthUpgradeButton))
        {
            bool canBuyHealth = currentGems >= healthPrice && healthUpgradeCount < maxUpgrades;
            UpdateImageButtonState(healthUpgradeButton, canBuyHealth);
        }
        
        // Update range button
        if (rangeUpgradeButton != null && !IsButtonPressed(rangeUpgradeButton))
        {
            bool canBuyRange = currentGems >= rangePrice && rangeUpgradeCount < maxUpgrades;
            UpdateImageButtonState(rangeUpgradeButton, canBuyRange);
        }
    }
    
    private void UpdateImageButtonState(Image button, bool enabled)
    {
        if (button == null) return;
        
        Color color = enabled ? enabledButtonColor : disabledButtonColor;
        color.a = enabled ? 1f : disabledButtonAlpha;
        button.color = color;
        button.raycastTarget = enabled;
    }
    
    private void UpdateProgressBars()
    {
        // Update health bars
        for (int i = 0; i < healthBars.Count; i++)
        {
            if (healthBars[i] != null)
            {
                bool isActive = i < healthUpgradeCount;
                healthBars[i].color = isActive ? activeBarColor : inactiveBarColor;
            }
        }
        
        // Update range bars
        for (int i = 0; i < rangeBars.Count; i++)
        {
            if (rangeBars[i] != null)
            {
                bool isActive = i < rangeUpgradeCount;
                rangeBars[i].color = isActive ? activeBarColor : inactiveBarColor;
            }
        }
    }
    
    private void OnGemCountChanged(uint newGemCount)
    {
        UpdateButtonStates();
    }
    
    #endregion
    
    #region Purchase Logic
    
    public void PurchaseHealthUpgrade()
    {
        if (healthUpgradeCount >= maxUpgrades)
        {
            Debug.Log("Health upgrade maxed out!");
            return;
        }
        
        uint price = CalculatePrice(healthUpgradeCount);
        
        if (ShopGemManager.Instance == null)
        {
            Debug.LogWarning("ShopGemManager not found!");
            return;
        }
        
        if (ShopGemManager.Instance.SpendGems(price))
        {
            healthUpgradeCount++;
            currentMaxHealth += healthUpgradeAmount;
            
            SaveUpgradeData();
            UpdatePriceDisplays();
            UpdateProgressBars();
            
            OnHealthUpgraded?.Invoke(currentMaxHealth);
            OnItemPurchased?.Invoke("Health", CalculatePrice(healthUpgradeCount));
            
            Debug.Log($"Health upgraded! New max health: {currentMaxHealth}");
        }
        else
        {
            Debug.Log("Not enough gems for health upgrade!");
        }
    }
    
    public void PurchaseRangeUpgrade()
    {
        if (rangeUpgradeCount >= maxUpgrades)
        {
            Debug.Log("Range upgrade maxed out!");
            return;
        }
        
        uint price = CalculatePrice(rangeUpgradeCount);
        
        if (ShopGemManager.Instance == null)
        {
            Debug.LogWarning("ShopGemManager not found!");
            return;
        }
        
        if (ShopGemManager.Instance.SpendGems(price))
        {
            rangeUpgradeCount++;
            currentAttackRange += rangeUpgradeAmount;
            
            SaveUpgradeData();
            UpdatePriceDisplays();
            UpdateProgressBars();
            
            OnRangeUpgraded?.Invoke(currentAttackRange);
            OnItemPurchased?.Invoke("Range", CalculatePrice(rangeUpgradeCount));
            
            Debug.Log($"Range upgraded! New attack range: {currentAttackRange}");
        }
        else
        {
            Debug.Log("Not enough gems for range upgrade!");
        }
    }
    
    private uint CalculatePrice(uint upgradeCount)
    {
        if (upgradeCount == 0) return basePrice;
        
        float price = basePrice;
        for (int i = 0; i < upgradeCount; i++)
        {
            price *= priceMultiplier;
        }
        
        return (uint)Mathf.Round(price);
    }
    
    #endregion
    
    #region Public Getters
    
    public uint GetHealthUpgradeCount() => healthUpgradeCount;
    public uint GetRangeUpgradeCount() => rangeUpgradeCount;
    public uint GetCurrentMaxHealth() => currentMaxHealth;
    public float GetCurrentAttackRange() => currentAttackRange;
    public uint GetHealthUpgradePrice() => CalculatePrice(healthUpgradeCount);
    public uint GetRangeUpgradePrice() => CalculatePrice(rangeUpgradeCount);
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Debug: Re-detect Bars")]
    public void DebugRedetectBars()
    {
        AutoDetectProgressBars();
        UpdateProgressBars();
    }
    
    [ContextMenu("Debug: Show Current Stats")]
    public void DebugShowStats()
    {
        Debug.Log($"=== Shop Purchase Stats ===\n" +
                 $"Health Upgrades: {healthUpgradeCount}/{maxUpgrades} (Max HP: {currentMaxHealth})\n" +
                 $"Range Upgrades: {rangeUpgradeCount}/{maxUpgrades} (Range: {currentAttackRange})\n" +
                 $"Health Bars Found: {healthBars.Count}\n" +
                 $"Range Bars Found: {rangeBars.Count}\n" +
                 $"Next Health Price: {GetHealthUpgradePrice()}\n" +
                 $"Next Range Price: {GetRangeUpgradePrice()}");
    }
    
    [ContextMenu("Debug: Reset All Upgrades")]
    public void DebugResetUpgrades()
    {
        healthUpgradeCount = 0;
        rangeUpgradeCount = 0;
        currentMaxHealth = 3;
        currentAttackRange = 1f;
        
        SaveUpgradeData();
        UpdatePriceDisplays();
        UpdateProgressBars();
        
        Debug.Log("All upgrades reset!");
    }
    
    [ContextMenu("Debug: Add 1 Health Upgrade")]
    public void DebugAddHealthUpgrade()
    {
        if (healthUpgradeCount < maxUpgrades)
        {
            healthUpgradeCount++;
            currentMaxHealth += healthUpgradeAmount;
            SaveUpgradeData();
            UpdatePriceDisplays();
            UpdateProgressBars();
        }
    }
    
    [ContextMenu("Debug: Add 1 Range Upgrade")]
    public void DebugAddRangeUpgrade()
    {
        if (rangeUpgradeCount < maxUpgrades)
        {
            rangeUpgradeCount++;
            currentAttackRange += rangeUpgradeAmount;
            SaveUpgradeData();
            UpdatePriceDisplays();
            UpdateProgressBars();
        }
    }
    
    #endregion
} 