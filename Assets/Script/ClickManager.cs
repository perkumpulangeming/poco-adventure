using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ClickManager : MonoBehaviour
{
    [Header("Visual Effect Settings")]
    [SerializeField] private float pressScaleFactor = 0.8f;
    [SerializeField] private float animationDuration = 0.1f;
    [SerializeField] private Color pressColor = Color.gray;
    [SerializeField] private AudioClip keyPressSound;
    
    private AudioSource audioSource;
    
    // Dictionary untuk menyimpan mapping key -> image
    private Dictionary<KeyCode, Image> keyImageMap;
    private Dictionary<KeyCode, Vector3> originalScales;
    private Dictionary<KeyCode, Color> originalColors;
    private Dictionary<KeyCode, bool> keyPressed;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize dictionaries
        keyImageMap = new Dictionary<KeyCode, Image>();
        originalScales = new Dictionary<KeyCode, Vector3>();
        originalColors = new Dictionary<KeyCode, Color>();
        keyPressed = new Dictionary<KeyCode, bool>();
        
        // Auto-find and setup key images
        SetupKeyImages();
    }
    
    void Update()
    {
        // Track keyboard input dan trigger visual effects
        CheckKeyInput(KeyCode.W);
        CheckKeyInput(KeyCode.A);
        CheckKeyInput(KeyCode.S);
        CheckKeyInput(KeyCode.D);
        CheckKeyInput(KeyCode.Space);
        CheckKeyInput(KeyCode.E);
    }
    
    private void SetupKeyImages()
    {
        // Nama-nama GameObject yang akan dicari
        string[] imageNames = { "WbuttonImg", "AbuttonImg", "SbuttonImg", "DbuttonImg", "SpaceButtonImg", "EbuttonImg" };
        KeyCode[] keyCodes = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space, KeyCode.E };
        
        for (int i = 0; i < imageNames.Length; i++)
        {
            // Cari GameObject berdasarkan nama
            GameObject imageObj = GameObject.Find(imageNames[i]);
            
            if (imageObj != null)
            {
                Image img = imageObj.GetComponent<Image>();
                if (img != null)
                {
                    KeyCode key = keyCodes[i];
                    
                    // Simpan ke dictionary
                    keyImageMap[key] = img;
                    originalScales[key] = img.transform.localScale;
                    originalColors[key] = img.color;
                    keyPressed[key] = false;
                    
                    Debug.Log($"Found and setup key image: {imageNames[i]} for key {key}");
                }
                else
                {
                    Debug.LogWarning($"GameObject {imageNames[i]} found but no Image component!");
                }
            }
            else
            {
                Debug.LogWarning($"GameObject {imageNames[i]} not found! Make sure the name matches exactly.");
            }
        }
        
        Debug.Log($"Total key images found: {keyImageMap.Count}");
    }
    
    private void CheckKeyInput(KeyCode key)
    {
        // Cek apakah key tersedia di mapping
        if (!keyImageMap.ContainsKey(key)) return;
        
        bool isCurrentlyPressed = Input.GetKey(key);
        bool wasPressed = keyPressed[key];
        
        // Key baru ditekan
        if (isCurrentlyPressed && !wasPressed)
        {
            OnKeyPress(key);
            keyPressed[key] = true;
        }
        // Key baru dilepas
        else if (!isCurrentlyPressed && wasPressed)
        {
            OnKeyRelease(key);
            keyPressed[key] = false;
        }
    }
    
    private void OnKeyPress(KeyCode key)
    {
        if (!keyImageMap.ContainsKey(key)) return;
        
        Image keyImage = keyImageMap[key];
        
        // Play sound effect
        PlayKeySound();
        
        // Apply press effect
        keyImage.transform.localScale = originalScales[key] * pressScaleFactor;
        keyImage.color = pressColor;
        
        Debug.Log($"Key {key} pressed - Visual effect applied");
    }
    
    private void OnKeyRelease(KeyCode key)
    {
        if (!keyImageMap.ContainsKey(key)) return;
        
        Image keyImage = keyImageMap[key];
        
        // Start release animation
        StartCoroutine(ReleaseAnimation(key));
    }
    
    private IEnumerator ReleaseAnimation(KeyCode key)
    {
        if (!keyImageMap.ContainsKey(key)) yield break;
        
        Image keyImage = keyImageMap[key];
        
        Vector3 startScale = keyImage.transform.localScale;
        Color startColor = keyImage.color;
        
        Vector3 targetScale = originalScales[key];
        Color targetColor = originalColors[key];
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Smooth lerp back to original state
            keyImage.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            keyImage.color = Color.Lerp(startColor, targetColor, progress);
            
            yield return null;
        }
        
        // Ensure final state
        keyImage.transform.localScale = targetScale;
        keyImage.color = targetColor;
    }
    
    private void PlayKeySound()
    {
        if (keyPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(keyPressSound);
        }
    }
    
    // Public method untuk trigger effect secara manual jika diperlukan
    public void TriggerKeyEffect(KeyCode key)
    {
        if (keyImageMap.ContainsKey(key))
        {
            StartCoroutine(ManualKeyEffect(key));
        }
    }
    
    private IEnumerator ManualKeyEffect(KeyCode key)
    {
        OnKeyPress(key);
        yield return new WaitForSeconds(0.1f);
        OnKeyRelease(key);
    }
    
    // Debug method untuk melihat status
    [ContextMenu("Debug Key Status")]
    public void DebugKeyStatus()
    {
        Debug.Log("=== Key Image Status ===");
        foreach (var kvp in keyImageMap)
        {
            string status = kvp.Value != null ? "OK" : "NULL";
            Debug.Log($"Key {kvp.Key}: {status}");
        }
    }
} 