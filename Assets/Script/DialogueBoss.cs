using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBoss : MonoBehaviour
{
    [Header("Cinematic Boss Dialogue System")]
    [SerializeField] private GameObject cinematicDialoguePanel; // Main fullscreen panel
    [SerializeField] private Image screenDimmer; // Background dimmer overlay
    [SerializeField] private float dimmerAlpha = 0.7f;
    
    [Header("Character Portraits")]
    [SerializeField] private GameObject playerPortraitContainer;
    [SerializeField] private GameObject bossPortraitContainer;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private Image bossPortraitImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text bossNameText;
    
    [Header("Dialogue Display")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText; // Who's currently speaking
    
    [Header("Boss Dialogue Data")]
    [SerializeField] private List<BossDialogueEntry> bossDialogueEntries;
    [SerializeField] private float cinematicWritingSpeed = 0.03f;
    
    [Header("Animation Settings")]
    [SerializeField] private float portraitSlideSpeed = 1f;
    [SerializeField] private float fadeInSpeed = 2f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip cinematicStartSound;
    [SerializeField] private AudioClip playerTypingSound;
    [SerializeField] private AudioClip bossTypingSound;
    [SerializeField] private AudioClip cinematicEndSound;
    [SerializeField] private float audioVolume = 0.5f;
    
    [Header("Player & Boss Names")]
    [SerializeField] private string playerDisplayName = "Hero";
    [SerializeField] private string bossDisplayName = "Frog Boss";
    
    // Private variables
    private int currentDialogueIndex = 0;
    private int currentCharIndex = 0;
    private bool isCinematicActive = false;
    private bool isWriting = false;
    private bool waitingForNext = false;
    private bool isFirstEncounter = true;
    
    // Components
    private AudioSource audioSource;
    private Canvas cinematicCanvas;
    private CanvasGroup cinematicCanvasGroup;
    
    // Animation positions
    private Vector3 playerPortraitStartPos;
    private Vector3 bossPortraitStartPos;
    private Vector3 playerPortraitEndPos;
    private Vector3 bossPortraitEndPos;

    [System.Serializable]
    public class BossDialogueEntry
    {
        public bool isPlayerSpeaking; // true = player, false = boss
        public string dialogueText;
        public float speakingDelay = 0f; // Delay before this line starts
        public bool useCustomWritingSpeed = false;
        public float customWritingSpeed = 0.03f;
    }

    private void Awake()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Setup canvas
        if (cinematicDialoguePanel != null)
        {
            cinematicCanvas = cinematicDialoguePanel.GetComponent<Canvas>();
            cinematicCanvasGroup = cinematicDialoguePanel.GetComponent<CanvasGroup>();
            
            if (cinematicCanvasGroup == null)
                cinematicCanvasGroup = cinematicDialoguePanel.AddComponent<CanvasGroup>();
        }
        
        // Store original portrait positions
        SetupPortraitPositions();
        
        // Initially hide cinematic panel
        HideCinematicPanel();
    }

    private void SetupPortraitPositions()
    {
        if (playerPortraitContainer != null)
        {
            playerPortraitEndPos = playerPortraitContainer.transform.localPosition;
            playerPortraitStartPos = playerPortraitEndPos + Vector3.left * 500f; // Start off-screen left
        }
        
        if (bossPortraitContainer != null)
        {
            bossPortraitEndPos = bossPortraitContainer.transform.localPosition;
            bossPortraitStartPos = bossPortraitEndPos + Vector3.right * 500f; // Start off-screen right
        }
    }

    private void HideCinematicPanel()
    {
        if (cinematicDialoguePanel != null)
        {
            cinematicDialoguePanel.SetActive(false);
        }
        
        // Reset portrait positions
        if (playerPortraitContainer != null)
            playerPortraitContainer.transform.localPosition = playerPortraitStartPos;
        if (bossPortraitContainer != null)
            bossPortraitContainer.transform.localPosition = bossPortraitStartPos;
    }

    public void StartCinematicBossDialogue()
    {
        if (isCinematicActive)
            return;
            
        Debug.Log("Starting Cinematic Boss Dialogue");
        
        // Pause game
        Time.timeScale = 0f;
        
        isCinematicActive = true;
        currentDialogueIndex = 0;
        currentCharIndex = 0;
        
        // Show cinematic panel
        cinematicDialoguePanel.SetActive(true);
        
        // Play cinematic start sound
        PlaySound(cinematicStartSound);
        
        // Start cinematic sequence
        StartCoroutine(CinematicIntroSequence());
    }

    private IEnumerator CinematicIntroSequence()
    {
        // Step 1: Fade in screen dimmer
        yield return StartCoroutine(FadeInScreenDimmer());
        
        // Step 2: Slide in character portraits
        yield return StartCoroutine(SlideInPortraits());
        
        // Step 3: Setup names
        SetupCharacterNames();
        
        // Step 4: Start first dialogue
        yield return new WaitForSecondsRealtime(0.5f);
        StartNextDialogue();
    }

    private IEnumerator FadeInScreenDimmer()
    {
        if (screenDimmer == null) yield break;
        
        float elapsedTime = 0f;
        Color startColor = screenDimmer.color;
        startColor.a = 0f;
        screenDimmer.color = startColor;
        
        Color targetColor = startColor;
        targetColor.a = dimmerAlpha;
        
        while (elapsedTime < (1f / fadeInSpeed))
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime * fadeInSpeed;
            
            screenDimmer.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        screenDimmer.color = targetColor;
    }

    private IEnumerator SlideInPortraits()
    {
        float elapsedTime = 0f;
        float slideTime = 1f / portraitSlideSpeed;
        
        while (elapsedTime < slideTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime * portraitSlideSpeed;
            float curveValue = slideInCurve.Evaluate(progress);
            
            // Slide player from left
            if (playerPortraitContainer != null)
            {
                playerPortraitContainer.transform.localPosition = 
                    Vector3.Lerp(playerPortraitStartPos, playerPortraitEndPos, curveValue);
            }
            
            // Slide boss from right
            if (bossPortraitContainer != null)
            {
                bossPortraitContainer.transform.localPosition = 
                    Vector3.Lerp(bossPortraitStartPos, bossPortraitEndPos, curveValue);
            }
            
            yield return null;
        }
        
        // Ensure final positions
        if (playerPortraitContainer != null)
            playerPortraitContainer.transform.localPosition = playerPortraitEndPos;
        if (bossPortraitContainer != null)
            bossPortraitContainer.transform.localPosition = bossPortraitEndPos;
    }

    private void SetupCharacterNames()
    {
        if (playerNameText != null)
            playerNameText.text = playerDisplayName;
        if (bossNameText != null)
            bossNameText.text = bossDisplayName;
    }

    private void StartNextDialogue()
    {
        if (currentDialogueIndex >= bossDialogueEntries.Count)
        {
            // End cinematic dialogue
            StartCoroutine(EndCinematicDialogue());
            return;
        }
        
        BossDialogueEntry currentEntry = bossDialogueEntries[currentDialogueIndex];
        
        // Set speaker name and highlight active portrait
        UpdateSpeakerDisplay(currentEntry.isPlayerSpeaking);
        
        // Clear dialogue text
        if (dialogueText != null)
            dialogueText.text = "";
        
        // Start writing with delay if specified
        StartCoroutine(StartDialogueWithDelay(currentEntry));
    }

    private IEnumerator StartDialogueWithDelay(BossDialogueEntry entry)
    {
        if (entry.speakingDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(entry.speakingDelay);
        }
        
        // Start typing animation
        currentCharIndex = 0;
        isWriting = true;
        waitingForNext = false;
        
        float writingSpeed = entry.useCustomWritingSpeed ? entry.customWritingSpeed : cinematicWritingSpeed;
        StartCoroutine(TypeDialogue(entry.dialogueText, writingSpeed, entry.isPlayerSpeaking));
    }

    private void UpdateSpeakerDisplay(bool isPlayerSpeaking)
    {
        // Set speaker name
        if (speakerNameText != null)
        {
            speakerNameText.text = isPlayerSpeaking ? playerDisplayName : bossDisplayName;
        }
        
        // Highlight active portrait (you can add highlight effects here)
        // For example, change opacity or add glow effect
        if (playerPortraitContainer != null)
        {
            CanvasGroup playerGroup = playerPortraitContainer.GetComponent<CanvasGroup>();
            if (playerGroup == null) playerGroup = playerPortraitContainer.AddComponent<CanvasGroup>();
            playerGroup.alpha = isPlayerSpeaking ? 1f : 0.7f;
        }
        
        if (bossPortraitContainer != null)
        {
            CanvasGroup bossGroup = bossPortraitContainer.GetComponent<CanvasGroup>();
            if (bossGroup == null) bossGroup = bossPortraitContainer.AddComponent<CanvasGroup>();
            bossGroup.alpha = isPlayerSpeaking ? 0.7f : 1f;
        }
    }

    private IEnumerator TypeDialogue(string text, float speed, bool isPlayerSpeaking)
    {
        currentCharIndex = 0;
        
        while (currentCharIndex < text.Length)
        {
            if (dialogueText != null)
                dialogueText.text = text.Substring(0, currentCharIndex + 1);
            
            // Play typing sound
            AudioClip soundToPlay = isPlayerSpeaking ? playerTypingSound : bossTypingSound;
            if (soundToPlay != null)
                PlaySound(soundToPlay, audioVolume * 0.3f);
            
            currentCharIndex++;
            yield return new WaitForSecondsRealtime(speed);
        }
        
        // Finish typing
        isWriting = false;
        waitingForNext = true;
    }

    private IEnumerator EndCinematicDialogue()
    {
        Debug.Log("Ending Cinematic Boss Dialogue");
        
        // Play end sound
        PlaySound(cinematicEndSound);
        
        // Slide out portraits
        yield return StartCoroutine(SlideOutPortraits());
        
        // Fade out screen dimmer
        yield return StartCoroutine(FadeOutScreenDimmer());
        
        // Hide cinematic panel
        HideCinematicPanel();
        
        // Resume game
        Time.timeScale = 1f;
        
        isCinematicActive = false;
        isFirstEncounter = false;
    }

    private IEnumerator SlideOutPortraits()
    {
        float elapsedTime = 0f;
        float slideTime = 1f / portraitSlideSpeed;
        
        while (elapsedTime < slideTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime * portraitSlideSpeed;
            float curveValue = slideInCurve.Evaluate(progress);
            
            // Slide player to left
            if (playerPortraitContainer != null)
            {
                playerPortraitContainer.transform.localPosition = 
                    Vector3.Lerp(playerPortraitEndPos, playerPortraitStartPos, curveValue);
            }
            
            // Slide boss to right
            if (bossPortraitContainer != null)
            {
                bossPortraitContainer.transform.localPosition = 
                    Vector3.Lerp(bossPortraitEndPos, bossPortraitStartPos, curveValue);
            }
            
            yield return null;
        }
    }

    private IEnumerator FadeOutScreenDimmer()
    {
        if (screenDimmer == null) yield break;
        
        float elapsedTime = 0f;
        Color startColor = screenDimmer.color;
        Color targetColor = startColor;
        targetColor.a = 0f;
        
        while (elapsedTime < (1f / fadeInSpeed))
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime * fadeInSpeed;
            
            screenDimmer.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        screenDimmer.color = targetColor;
    }

    private void PlaySound(AudioClip clip, float volume = -1f)
    {
        if (clip != null && audioSource != null)
        {
            float vol = volume >= 0f ? volume : audioVolume;
            audioSource.PlayOneShot(clip, vol);
        }
    }

    // Public methods
    public bool IsCinematicActive()
    {
        return isCinematicActive;
    }
    
    public bool IsFirstEncounter()
    {
        return isFirstEncounter;
    }
    
    public void SetFirstEncounter(bool firstEncounter)
    {
        isFirstEncounter = firstEncounter;
    }

    private void Update()
    {
        if (!isCinematicActive)
            return;
        
        // Handle input to proceed dialogue
        if (waitingForNext && Input.GetKeyDown(KeyCode.F))
        {
            waitingForNext = false;
            currentDialogueIndex++;
            StartNextDialogue();
        }
        
        // Skip typing animation
        if (isWriting && Input.GetKeyDown(KeyCode.Space))
        {
            // Complete current dialogue instantly
            StopAllCoroutines();
            if (currentDialogueIndex < bossDialogueEntries.Count)
            {
                dialogueText.text = bossDialogueEntries[currentDialogueIndex].dialogueText;
            }
            isWriting = false;
            waitingForNext = true;
        }
    }
} 