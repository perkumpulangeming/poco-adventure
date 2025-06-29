using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Dialogue dialogueScript;
    
    [Header("UI Button Settings")]
    [SerializeField] private string fButtonName = "FbuttonImg";
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Click Effect Settings")]
    [SerializeField] private float pressScaleFactor = 0.8f;
    [SerializeField] private float clickAnimationDuration = 0.1f;
    [SerializeField] private Color pressColor = Color.gray;
    [SerializeField] private AudioClip keyPressSound;
    
    private bool playerDetected;
    private Image fButtonImage;
    private Vector3 originalFButtonScale;
    private Color originalFButtonColor;
    private Coroutine buttonAnimationCoroutine;
    private Coroutine clickEffectCoroutine;
    private bool isButtonVisible = false;
    private AudioSource audioSource;

    private void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Find and setup F button
        SetupFButton();
    }

    private void SetupFButton()
    {
        // Find FbuttonImg GameObject
        GameObject fButtonObj = GameObject.Find(fButtonName);
        
        if (fButtonObj != null)
        {
            fButtonImage = fButtonObj.GetComponent<Image>();
            if (fButtonImage != null)
            {
                // Store original properties
                originalFButtonScale = fButtonImage.transform.localScale;
                originalFButtonColor = fButtonImage.color;
                
                // Hide button initially
                HideFButtonImmediate();
                
                Debug.Log($"F Button setup complete: {fButtonName}");
            }
            else
            {
                Debug.LogWarning($"FbuttonImg found but no Image component!");
            }
        }
        else
        {
            Debug.LogWarning($"FbuttonImg not found! Make sure the name matches exactly: {fButtonName}");
        }
    }

    private void HideFButtonImmediate()
    {
        if (fButtonImage != null)
        {
            fButtonImage.transform.localScale = Vector3.zero;
            Color hiddenColor = originalFButtonColor;
            hiddenColor.a = 0f;
            fButtonImage.color = hiddenColor;
            isButtonVisible = false;
        }
    }

    //Detect trigger with player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If we triggerd the player enable playerdeteced and show indicator
        if(collision.gameObject.layer == Global.Layers.PlayerID)
        {
            playerDetected = true;
            dialogueScript.ToggleIndicator(playerDetected);
            
            // Show F button with smooth animation
            ShowFButton();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //If we lost trigger  with the player disable playerdeteced and hide indicator
        if (collision.gameObject.layer == Global.Layers.PlayerID)
        {
            playerDetected = false;
            dialogueScript.ToggleIndicator(playerDetected);
            dialogueScript.EndDialogue();
            
            // Hide F button with smooth animation - only if currently visible
            if (isButtonVisible)
            {
                HideFButton();
            }
        }
    }

    private void ShowFButton()
    {
        if (fButtonImage == null || isButtonVisible) return;
        
        // Stop any existing animation
        if (buttonAnimationCoroutine != null)
        {
            StopCoroutine(buttonAnimationCoroutine);
        }
        
        isButtonVisible = true;
        buttonAnimationCoroutine = StartCoroutine(AnimateFButton(true));
    }

    private void HideFButton()
    {
        if (fButtonImage == null || !isButtonVisible) return;
        
        // Stop any existing animation
        if (buttonAnimationCoroutine != null)
        {
            StopCoroutine(buttonAnimationCoroutine);
        }
        
        isButtonVisible = false;
        buttonAnimationCoroutine = StartCoroutine(AnimateFButton(false));
    }

    private IEnumerator AnimateFButton(bool show)
    {
        float elapsedTime = 0f;
        
        // Starting values
        Vector3 startScale = fButtonImage.transform.localScale;
        Color startColor = fButtonImage.color;
        
        // Target values
        Vector3 targetScale = show ? originalFButtonScale : Vector3.zero;
        Color targetColor = originalFButtonColor;
        targetColor.a = show ? originalFButtonColor.a : 0f;
        
        // Animation curve to use
        AnimationCurve curveToUse = show ? showCurve : hideCurve;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Apply curve
            float curveValue = curveToUse.Evaluate(progress);
            
            // Lerp scale and color
            fButtonImage.transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            fButtonImage.color = Color.Lerp(startColor, targetColor, curveValue);
            
            yield return null;
        }
        
        // Ensure final state
        fButtonImage.transform.localScale = targetScale;
        fButtonImage.color = targetColor;
        
        buttonAnimationCoroutine = null;
    }

    private void PlayClickEffect()
    {
        if (fButtonImage == null || !isButtonVisible) return;
        
        // Play sound effect
        if (keyPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(keyPressSound);
        }
        
        // Stop any existing click effect
        if (clickEffectCoroutine != null)
        {
            StopCoroutine(clickEffectCoroutine);
        }
        
        clickEffectCoroutine = StartCoroutine(ClickEffectAnimation());
    }

    private IEnumerator ClickEffectAnimation()
    {
        // Press effect
        Vector3 pressedScale = originalFButtonScale * pressScaleFactor;
        Color pressedColor = pressColor;
        
        // Store current values
        Vector3 currentScale = fButtonImage.transform.localScale;
        Color currentColor = fButtonImage.color;
        
        // Quick press animation
        float elapsedTime = 0f;
        while (elapsedTime < clickAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / clickAnimationDuration;
            
            fButtonImage.transform.localScale = Vector3.Lerp(currentScale, pressedScale, progress);
            fButtonImage.color = Color.Lerp(currentColor, pressedColor, progress);
            
            yield return null;
        }
        
        // Release animation
        elapsedTime = 0f;
        while (elapsedTime < clickAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / clickAnimationDuration;
            
            fButtonImage.transform.localScale = Vector3.Lerp(pressedScale, originalFButtonScale, progress);
            fButtonImage.color = Color.Lerp(pressedColor, originalFButtonColor, progress);
            
            yield return null;
        }
        
        // Ensure final state
        fButtonImage.transform.localScale = originalFButtonScale;
        fButtonImage.color = originalFButtonColor;
        
        clickEffectCoroutine = null;
    }

    //While detected if we interact start the dialogue
    private void Update()
    {
        if(playerDetected && Input.GetKeyDown(KeyCode.F))
        {
            // Play click effect
            PlayClickEffect();
            
            // Start dialogue
            dialogueScript.StartDialogue();
        }
    }
}
