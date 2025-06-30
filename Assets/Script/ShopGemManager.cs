using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Game;
using Game.Items.Gem;

public class ShopGemManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gemCountText;
    [SerializeField] private TextMeshProUGUI[] additionalGemTexts; // For multiple gem displays in shop
    
    [Header("Shop Settings")]
    [SerializeField] private bool autoUpdateUI = true;
    [SerializeField] private string gemCountFormat = "{0}"; // Format untuk display gem count
    
    // Events untuk notifikasi perubahan gem
    public static event Action<uint> OnGemCountChanged;
    public static event Action<uint, uint> OnGemSpent; // spent amount, remaining gems
    public static event Action OnInsufficientGems;
    
    // Singleton pattern untuk akses mudah
    public static ShopGemManager Instance { get; private set; }
    
    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ShopGemManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Subscribe to gem collect events untuk real-time updates
        if (autoUpdateUI)
        {
            GemController.OnItemCollect += OnGemCollected;
        }
        
        // Initial gem count display
        UpdateGemDisplay();
    }
    
    private void OnDestroy()
    {
        // Cleanup event subscriptions
        if (autoUpdateUI)
        {
            GemController.OnItemCollect -= OnGemCollected;
        }
        
        // Clear singleton reference
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #region Public Methods - Gem Operations
    
    /// <summary>
    /// Mendapatkan jumlah gem yang dimiliki player saat ini
    /// </summary>
    /// <returns>Current gem count</returns>
    public uint GetCurrentGemCount()
    {
        return GameStats.CollectedGems;
    }
    
    /// <summary>
    /// Cek apakah player memiliki cukup gem untuk pembelian
    /// </summary>
    /// <param name="requiredAmount">Jumlah gem yang dibutuhkan</param>
    /// <returns>True jika cukup, false jika tidak</returns>
    public bool HasEnoughGems(uint requiredAmount)
    {
        return GameStats.CollectedGems >= requiredAmount;
    }
    
    /// <summary>
    /// Melakukan pembelian dengan gem (mengurangi gem count)
    /// </summary>
    /// <param name="cost">Harga dalam gem</param>
    /// <returns>True jika berhasil, false jika gem tidak cukup</returns>
    public bool SpendGems(uint cost)
    {
        if (!HasEnoughGems(cost))
        {
            Debug.LogWarning($"Insufficient gems! Required: {cost}, Available: {GameStats.CollectedGems}");
            OnInsufficientGems?.Invoke();
            return false;
        }
        
        uint oldGemCount = GameStats.CollectedGems;
        GameStats.CollectedGems -= cost;
        
        Debug.Log($"Gems spent: {cost}. Remaining: {GameStats.CollectedGems}");
        
        // Trigger events
        OnGemSpent?.Invoke(cost, GameStats.CollectedGems);
        OnGemCountChanged?.Invoke(GameStats.CollectedGems);
        
        // Update UI
        if (autoUpdateUI)
        {
            UpdateGemDisplay();
        }
        
        return true;
    }
    
    /// <summary>
    /// Menambah gem (untuk testing atau reward)
    /// </summary>
    /// <param name="amount">Jumlah gem yang ditambahkan</param>
    public void AddGems(uint amount)
    {
        GameStats.CollectedGems += amount;
        
        Debug.Log($"Gems added: {amount}. Total: {GameStats.CollectedGems}");
        
        // Trigger events
        OnGemCountChanged?.Invoke(GameStats.CollectedGems);
        
        // Update UI
        if (autoUpdateUI)
        {
            UpdateGemDisplay();
        }
    }
    
    /// <summary>
    /// Set gem count secara langsung (untuk testing)
    /// </summary>
    /// <param name="amount">Jumlah gem yang akan diset</param>
    public void SetGemCount(uint amount)
    {
        GameStats.CollectedGems = amount;
        
        Debug.Log($"Gem count set to: {GameStats.CollectedGems}");
        
        // Trigger events
        OnGemCountChanged?.Invoke(GameStats.CollectedGems);
        
        // Update UI
        if (autoUpdateUI)
        {
            UpdateGemDisplay();
        }
    }
    
    #endregion
    
    #region UI Methods
    
    /// <summary>
    /// Update semua UI text yang menampilkan gem count
    /// </summary>
    public void UpdateGemDisplay()
    {
        uint currentGems = GetCurrentGemCount();
        string displayText = string.Format(gemCountFormat, currentGems);
        
        // Update main gem count text
        if (gemCountText != null)
        {
            gemCountText.text = displayText;
            
            // Fix TextMeshPro display issues
            gemCountText.ForceMeshUpdate();
            
            // Ensure text fits properly
            if (gemCountText.isTextOverflowing)
            {
                // Try to enable auto-sizing if text overflows
                gemCountText.enableAutoSizing = true;
                gemCountText.fontSizeMin = 1f;
                gemCountText.fontSizeMax = gemCountText.fontSize;
            }
        }
        
        // Update additional gem texts
        if (additionalGemTexts != null)
        {
            foreach (var textComponent in additionalGemTexts)
            {
                if (textComponent != null)
                {
                    textComponent.text = displayText;
                    textComponent.ForceMeshUpdate();
                    
                    // Fix overflow for additional texts too
                    if (textComponent.isTextOverflowing)
                    {
                        textComponent.enableAutoSizing = true;
                        textComponent.fontSizeMin = 1f;
                        textComponent.fontSizeMax = textComponent.fontSize;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Set format untuk display gem count
    /// </summary>
    /// <param name="format">Format string (contoh: "Gems: {0}", "{0} 💎")</param>
    public void SetGemDisplayFormat(string format)
    {
        gemCountFormat = format;
        if (autoUpdateUI)
        {
            UpdateGemDisplay();
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnGemCollected()
    {
        // Dipanggil ketika player mengumpulkan gem
        if (autoUpdateUI)
        {
            UpdateGemDisplay();
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Format gem amount sebagai string dengan separator
    /// </summary>
    /// <param name="amount">Jumlah gem</param>
    /// <returns>Formatted string</returns>
    public static string FormatGemAmount(uint amount)
    {
        return amount.ToString("N0"); // Adds thousand separators
    }
    
    /// <summary>
    /// Cek apakah pembelian akan membuat gem menjadi minus
    /// </summary>
    /// <param name="cost">Harga item</param>
    /// <param name="remainingGems">Output: sisa gem setelah pembelian</param>
    /// <returns>True jika aman, false jika akan minus</returns>
    public bool CanAfford(uint cost, out uint remainingGems)
    {
        uint currentGems = GetCurrentGemCount();
        
        if (currentGems >= cost)
        {
            remainingGems = currentGems - cost;
            return true;
        }
        else
        {
            remainingGems = 0;
            return false;
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Debug: Add 100 Gems")]
    private void DebugAdd100Gems()
    {
        AddGems(100);
    }
    
    [ContextMenu("Debug: Spend 50 Gems")]
    private void DebugSpend50Gems()
    {
        SpendGems(50);
    }
    
    [ContextMenu("Debug: Show Current Gems")]
    private void DebugShowCurrentGems()
    {
        Debug.Log($"Current Gems: {GetCurrentGemCount()}");
    }
    
    [ContextMenu("Debug: Reset Gems to 0")]
    private void DebugResetGems()
    {
        SetGemCount(0);
    }
    
    #endregion
} 