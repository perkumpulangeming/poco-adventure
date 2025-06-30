using Game.Buildings.House;
using Game.Helpers;
using Game.Items.Gem;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Game.Characters.Player.UI
{
    public enum MenuState
    {
        HUD,
        Pause,
        Death,
        Win
    }

    public sealed class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gemsCountText;
        [SerializeField] private TextMeshProUGUI killsCountText;
        [SerializeField] private TextMeshProUGUI heartsCountText;

        [SerializeField] private Image[] winScreenImages;

        [SerializeField] private Canvas hud;
        [SerializeField] private Canvas escape;

        // Menu System
        [Header("Menu System")]
        [SerializeField] private bool isEndLevel = false; // If true, hide Next Level button on win
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject deathMenuPanel;
        [SerializeField] private GameObject winMenuPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMainMenuButton;
        [SerializeField] private Button deathRestartButton;
        [SerializeField] private Button deathMainMenuButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button winMainMenuButton;

        // Death Menu UI Elements
        [Header("Death Menu UI")]
        [SerializeField] private TextMeshProUGUI deathGemsCountText;
        [SerializeField] private TextMeshProUGUI deathKillsCountText;

        // Win Menu UI Elements  
        [Header("Win Menu UI")]
        [SerializeField] private TextMeshProUGUI winGemsCountText;
        [SerializeField] private TextMeshProUGUI winKillsCountText;

        // Simple Heart Visual System
        [Header("Heart Visual System")]
        [SerializeField] private bool useHeartVisual = false; // Toggle between text and heart visual
        [SerializeField] private Image referenceHeartImage; // Reference heart image to copy
        [SerializeField] private Transform heartContainer; // Where to put copied hearts
        [SerializeField] private float heartSpacing = 60f; // Distance between hearts

        // Credits System
        [Header("Credits System")]
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private RectTransform creditsContainer;
        [SerializeField] private float creditsScrollSpeed = 100f; // Speed of scrolling
        [SerializeField] private float creditsStartDelay = 0.5f; // Delay before starting
        [SerializeField] private float creditsStartY = -500f; // Starting position (below screen)
        [SerializeField] private float creditsEndY = 600f; // Ending position (above screen)
        [SerializeField] private Button skipCreditsButton;

        private PlayerController _playerController;
        private Camera _playerCamera;
        private MenuState currentMenuState = MenuState.HUD;

        // Credits animation variables
        private Coroutine creditsScrollCoroutine;
        private bool isCreditsPlaying = false;

        // Credits content
        private string creditsContent = @"YAH, SELESAI JUGA...

Game ini akhirnya kelar juga, walau banyak drama dan begadang gak jelas.

SPECIAL THANKS:

DOSEN PEMBIMBING  
Makasih udah ngasih arahan, walau kadang bikin pusing.

TIM PENGEMBANG  
Ya, kita juga. Ngetik kode sampe lupa tidur.

TEMAN-TEMAN KELAS  
Sering ngasih semangat... atau malah ngajak mabar pas lagi ngoding.

ASET & TOOLS  
Unity  
Sunnyland Asset Pack  
Pixel Art Community  
(gak mungkin bikin semuanya sendiri sih)

TESTER & MASUKAN  
Thanks udah nemuin bug yang kita pura-pura gak liat.

KELUARGA & TEMAN  
Dukungannya... ya, kita hargai banget kok.

YANG MAININ  
Lu keren sih udah sampe sini. Terima kasih udah mainin, semoga gak nyesel.

Dibuat pake Unity  
© 2025  

Udah segitu aja.  
DAH, MAKASIH.";

        // Heart visual system
        private List<Image> heartImages = new List<Image>();
        private Color fullHeartColor = Color.white;
        private Color emptyHeartColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray
        private uint maxHealth;
        private bool heartSystemInitialized = false;

        private void Awake()
        {
            Cursor.visible = false;

            // Ensure GameManager is initialized
            _ = GameManager.Instance;

            _playerController = FindObjectOfType<PlayerController>();
            _playerCamera = _playerController.GetComponentInChildren<Camera>();

            EntityController.OnEntityDeath += OnKill;
            FindObjectOfType<HouseController>().OnWin += OnWin;
            _playerController.OnHealthChange += OnHealthChange;
            _playerController.OnDeath += OnDeath;
            _playerController.OnEscapePressed += OnEscapePressed;

            GemController.OnItemCollect += OnItemCollect;

            // Setup menu buttons
            SetupMenuButtons();
        }

        private void SetupMenuButtons()
        {
            // Pause Menu Buttons
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(RestartLevel);
            if (pauseMainMenuButton != null)
                pauseMainMenuButton.onClick.AddListener(GoToMainMenu);

            // Death Menu Buttons
            if (deathRestartButton != null)
                deathRestartButton.onClick.AddListener(RestartLevel);
            if (deathMainMenuButton != null)
                deathMainMenuButton.onClick.AddListener(GoToMainMenu);

            // Win Menu Buttons
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(GoToNextLevel);
            if (winMainMenuButton != null)
                winMainMenuButton.onClick.AddListener(GoToMainMenu);

            // Credits Button
            if (skipCreditsButton != null)
                skipCreditsButton.onClick.AddListener(SkipCredits);
        }

        private void Start()
        {
            // Setup health display based on mode
            SetupHealthDisplay();

            heartsCountText.text = _playerController.Health.GetHealthAmount().ToString();

            // Initialize gems count from persistent storage
            gemsCountText.text = GameStats.CollectedGems.ToString();

            // Load and display persisted gems
            gemsCountText.text = GameStats.CollectedGems.ToString();

            InputSystem.EnableDevice(Keyboard.current);
        }

        private void SetupHealthDisplay()
        {
            if (useHeartVisual)
            {
                // Hide text, show heart visual
                if (heartsCountText != null)
                    heartsCountText.gameObject.SetActive(false);

                // Setup heart visual system
                SetupHeartVisualSystem();

                // Subscribe to taking damage event for visual effects
                _playerController.OnTakingDamage += OnTakingDamageVisual;
            }
            else
            {
                // Show text, hide heart visual
                if (heartsCountText != null)
                    heartsCountText.gameObject.SetActive(true);

                // Hide heart visual
                HideHeartVisual();

                // Unsubscribe from taking damage event
                _playerController.OnTakingDamage -= OnTakingDamageVisual;
            }
        }

        private void SetupHeartVisualSystem()
        {
            if (referenceHeartImage == null)
            {
                Debug.LogWarning("Reference Heart Image not assigned!");
                return;
            }

            // Get max health from player (refresh every time)
            maxHealth = _playerController.Health.GetHealthAmount();
            fullHeartColor = referenceHeartImage.color;

            // Auto-create container if not assigned
            if (heartContainer == null)
            {
                GameObject containerObj = new GameObject("HeartContainer");
                containerObj.transform.SetParent(referenceHeartImage.transform.parent);
                containerObj.transform.localScale = Vector3.one;

                // Position container at reference heart position
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                RectTransform refRect = referenceHeartImage.rectTransform;
                containerRect.anchoredPosition = refRect.anchoredPosition;
                containerRect.sizeDelta = new Vector2(maxHealth * heartSpacing, refRect.sizeDelta.y);

                heartContainer = containerObj.transform;
            }
            else
            {
                // Update container size if max health changed
                RectTransform containerRect = heartContainer.GetComponent<RectTransform>();
                if (containerRect != null)
                {
                    containerRect.sizeDelta = new Vector2(maxHealth * heartSpacing, containerRect.sizeDelta.y);
                }
            }

            // Hide original reference heart
            referenceHeartImage.gameObject.SetActive(false);

            CreateHeartCopies();
            heartSystemInitialized = true;
        }

        private void CreateHeartCopies()
        {
            // Clear existing hearts
            ClearHeartImages();

            // Create heart copies based on max health
            for (int i = 0; i < maxHealth; i++)
            {
                // Create heart copy
                GameObject heartCopy = new GameObject($"Heart_{i + 1}");
                heartCopy.transform.SetParent(heartContainer);
                heartCopy.transform.localScale = Vector3.one;

                // Copy image component
                Image heartImage = heartCopy.AddComponent<Image>();
                heartImage.sprite = referenceHeartImage.sprite;
                heartImage.color = fullHeartColor;
                heartImage.preserveAspect = referenceHeartImage.preserveAspect;
                heartImage.type = referenceHeartImage.type;

                // Setup position
                RectTransform heartRect = heartCopy.GetComponent<RectTransform>();
                heartRect.sizeDelta = referenceHeartImage.rectTransform.sizeDelta;
                heartRect.anchoredPosition = new Vector2(i * heartSpacing, 0);

                heartImages.Add(heartImage);
            }

            // Update display for current health
            UpdateHeartVisual(_playerController.Health.GetHealthAmount());
        }

        private void ClearHeartImages()
        {
            foreach (Image heart in heartImages)
            {
                if (heart != null)
                    DestroyImmediate(heart.gameObject);
            }
            heartImages.Clear();
        }

        private void HideHeartVisual()
        {
            // Show original reference heart
            if (referenceHeartImage != null)
                referenceHeartImage.gameObject.SetActive(true);

            // Clear heart copies
            ClearHeartImages();

            heartSystemInitialized = false;
        }

        private void UpdateHeartVisual(uint currentHealth)
        {
            if (!heartSystemInitialized) return;

            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] == null) continue;

                bool shouldBeFull = i < currentHealth;
                Color targetColor = shouldBeFull ? fullHeartColor : emptyHeartColor;

                // Smooth color transition
                StartCoroutine(SmoothColorChange(heartImages[i], targetColor));
            }
        }

        private IEnumerator SmoothColorChange(Image heartImage, Color targetColor)
        {
            if (heartImage == null) yield break;

            Color startColor = heartImage.color;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration && heartImage != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                heartImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            if (heartImage != null)
                heartImage.color = targetColor;
        }

        private void OnTakingDamageVisual()
        {
            // Trigger shake effect for heart visual
            if (useHeartVisual && heartSystemInitialized && heartContainer != null)
            {
                StartCoroutine(HeartShakeEffect());
            }
        }

        private IEnumerator HeartShakeEffect()
        {
            if (heartContainer == null) yield break;

            Vector3 originalPos = heartContainer.localPosition;
            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration && heartContainer != null)
            {
                float shake = 8f * (1f - elapsed / duration);
                Vector3 randomOffset = new Vector3(
                    Random.Range(-shake, shake),
                    Random.Range(-shake, shake),
                    0
                );

                if (heartContainer != null)
                    heartContainer.localPosition = originalPos + randomOffset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (heartContainer != null)
                heartContainer.localPosition = originalPos;
        }

        private float OnKill()
        {
            killsCountText.text = (int.Parse(killsCountText.text) + 1).ToString();

            return float.NaN;
        }

        private void OnHealthChange(uint healthAmount)
        {
            // Update text (keep for compatibility)
            if (heartsCountText != null && heartsCountText.gameObject.activeSelf)
            {
                heartsCountText.text = healthAmount.ToString();
            }

            // Update heart visual
            if (useHeartVisual && heartSystemInitialized)
            {
                UpdateHeartVisual(healthAmount);
            }
        }

        private float OnDeath()
        {
            var deathAnimationLength = _playerController.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length;

            Timer.Create(() =>
            {
                ShowDeathMenu();
            }, deathAnimationLength);

            return float.NaN;
        }

        private void ShowDeathMenu()
        {
            currentMenuState = MenuState.Death;
            hud.gameObject.SetActive(false);
            escape.gameObject.SetActive(true);
            _playerCamera.gameObject.SetActive(false);

            // Hide all existing escape canvas elements first
            HideEscapeCanvasElements();

            // Hide all menu panels first
            HideAllMenuPanels();

            // Show death menu panel
            if (deathMenuPanel != null)
            {
                deathMenuPanel.SetActive(true);

                // Update death menu stats
                if (deathGemsCountText != null)
                    deathGemsCountText.text = gemsCountText.text;
                if (deathKillsCountText != null)
                    deathKillsCountText.text = killsCountText.text;
            }
            else
            {
                Debug.LogWarning("[PlayerUI] Death menu panel not assigned! Please assign DeathMenuPanel in inspector.");
            }

            Cursor.visible = true;
            _playerController.EnableMovement(false);
        }

        private void OnEscapePressed()
        {
            if (currentMenuState == MenuState.HUD)
            {
                ShowPauseMenu();
            }
            else if (currentMenuState == MenuState.Pause)
            {
                ResumeGame();
            }
        }

        private void ShowPauseMenu()
        {
            currentMenuState = MenuState.Pause;
            hud.gameObject.SetActive(false);
            escape.gameObject.SetActive(true);

            // Hide ALL existing escape canvas elements first
            HideEscapeCanvasElements();

            // Hide all menu panels 
            HideAllMenuPanels();

            // Show ONLY pause menu panel
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }

            _playerCamera.gameObject.SetActive(false);
            Cursor.visible = true;
            _playerController.EnableMovement(false);

            // Pause the game
            Time.timeScale = 0f;
        }

        private void HideEscapeCanvasElements()
        {
            // Hide win screen images
            if (winScreenImages != null)
            {
                foreach (var image in winScreenImages)
                {
                    if (image != null)
                    {
                        image.gameObject.SetActive(false);
                        Debug.Log($"[PlayerUI] Hidden winScreenImage: {image.name}");
                    }
                }
            }

            Debug.Log($"[PlayerUI] PauseMenuPanel assigned: {pauseMenuPanel != null}");
        }

        private void HideAllMenuPanels()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (deathMenuPanel != null) deathMenuPanel.SetActive(false);
            if (winMenuPanel != null) winMenuPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        private void ResumeGame()
        {
            currentMenuState = MenuState.HUD;
            hud.gameObject.SetActive(true);
            escape.gameObject.SetActive(false);

            _playerCamera.gameObject.SetActive(true);
            Cursor.visible = false;
            _playerController.EnableMovement(true);

            // Resume the game
            Time.timeScale = 1f;
        }

        private void RestartLevel()
        {
            // Resume time before restarting
            Time.timeScale = 1f;

            // Get current scene name and reload it
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }

        private void GoToMainMenu()
        {
            // Resume time before going to main menu
            Time.timeScale = 1f;

            // Load main menu scene (adjust scene name as needed)
            SceneManager.LoadScene("MainMenu");
        }

        private void GoToNextLevel()
        {
            // Resume time before going to next level
            Time.timeScale = 1f;

            // Get current scene name and determine next level
            string currentSceneName = SceneManager.GetActiveScene().name;
            string nextLevelName = GetNextLevelName(currentSceneName);

            if (!string.IsNullOrEmpty(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // If no next level, go to main menu
                GoToMainMenu();
            }
        }

        private string GetNextLevelName(string currentLevel)
        {
            // If this is marked as end level, no next level
            if (isEndLevel)
            {
                Debug.Log($"[PlayerUI] End level reached: {currentLevel}");
                return null;
            }

            // Define level progression
            switch (currentLevel)
            {
                case "Level_01":
                    return "Level_02";
                case "Level_02":
                    return "Level_03";
                case "Level_03":
                    return "Level_Bonus";
                case "Level_Bonus":
                    return null; // Last level by default
                default:
                    Debug.LogWarning($"[PlayerUI] Unknown level: {currentLevel}");
                    return null; // Unknown level
            }
        }

        private void OnItemCollect()
        {
            // Update UI to reflect current gem count from GameStats
            gemsCountText.text = GameStats.CollectedGems.ToString();
        }

        private void OnWin()
        {
            ShowWinMenu();
        }

        private void ShowWinMenu()
        {
            currentMenuState = MenuState.Win;
            hud.gameObject.SetActive(false);
            escape.gameObject.SetActive(true);

            _playerCamera.gameObject.SetActive(false);
            _playerController.gameObject.SetActive(false);

            Cursor.visible = true;
            InputSystem.DisableDevice(Keyboard.current);

            // Hide all existing escape canvas elements first
            HideEscapeCanvasElements();

            // Hide all menu panels first  
            HideAllMenuPanels();

            // Show win menu panel if available, otherwise use existing system
            if (winMenuPanel != null)
            {
                winMenuPanel.SetActive(true);

                // Update win menu stats
                if (winGemsCountText != null)
                    winGemsCountText.text = gemsCountText.text;
                if (winKillsCountText != null)
                    winKillsCountText.text = killsCountText.text;

                // Control Next Level button visibility based on isEndLevel
                if (nextLevelButton != null)
                {
                    nextLevelButton.gameObject.SetActive(!isEndLevel);
                    Debug.Log($"[PlayerUI] Next Level button visible: {!isEndLevel} (isEndLevel: {isEndLevel})");
                }

                // If this is end level, transition to credits after delay
                if (isEndLevel)
                {
                    StartCoroutine(TransitionToCredits());
                }
            }
            else
            {
                // Fallback to existing win system
                foreach (var image in winScreenImages)
                    image.gameObject.SetActive(true);

                // If this is end level, transition to credits after delay
                if (isEndLevel)
                {
                    StartCoroutine(TransitionToCredits());
                }
            }
        }

        private IEnumerator TransitionToCredits()
        {
            yield return new WaitForSeconds(1.5f); // Reduced from 2f to 1.5f
            ShowCredits();
        }

        private void ShowCredits()
        {
            // Hide win menu panel
            HideAllMenuPanels();
            
            // Show credits panel
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
            
            // Setup credits text
            if (creditsText != null)
            {
                creditsText.text = creditsContent;
            }
            
            // Setup initial position using custom setting
            if (creditsContainer != null)
            {
                // Use custom starting position that can be adjusted in inspector
                creditsContainer.anchoredPosition = new Vector2(0, creditsStartY);
            }
            
            isCreditsPlaying = true;
            
            // Start scrolling credits after delay
            if (creditsScrollCoroutine == null)
            {
                creditsScrollCoroutine = StartCoroutine(ScrollCredits());
            }
        }

        private IEnumerator ScrollCredits()
        {
            // Wait for start delay
            yield return new WaitForSeconds(creditsStartDelay);
            
            if (creditsContainer == null)
            {
                Debug.LogWarning("[PlayerUI] Credits container not assigned!");
                yield break;
            }
            
            // Use custom start and end positions
            float startY = creditsStartY;
            float endY = creditsEndY;
            float totalDistance = endY - startY;
            float scrollDuration = totalDistance / creditsScrollSpeed;
            
            Debug.Log($"[PlayerUI] Credits scroll: Start={startY}, End={endY}, Duration={scrollDuration:F2}s");
            
            float elapsed = 0f;
            
            while (elapsed < scrollDuration && isCreditsPlaying)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
                float t = elapsed / scrollDuration;
                
                // Calculate current position
                float currentY = Mathf.Lerp(startY, endY, t);
                creditsContainer.anchoredPosition = new Vector2(0, currentY);
                
                yield return null;
            }
            
            // Credits finished, go to main menu
            if (isCreditsPlaying)
            {
                yield return new WaitForSeconds(1f);
                GoToMainMenu();
            }
            
            creditsScrollCoroutine = null;
        }

        private void SkipCredits()
        {
            if (isCreditsPlaying)
            {
                isCreditsPlaying = false;

                if (creditsScrollCoroutine != null)
                {
                    StopCoroutine(creditsScrollCoroutine);
                    creditsScrollCoroutine = null;
                }

                GoToMainMenu();
            }
        }

        // Public method to toggle between text and heart visual
        [ContextMenu("Toggle Heart Visual")]
        public void ToggleHeartVisual()
        {
            useHeartVisual = !useHeartVisual;
            SetupHealthDisplay();
        }

        // Public method to set heart visual mode
        public void SetHeartVisualMode(bool enable)
        {
            useHeartVisual = enable;
            SetupHealthDisplay();
        }

        // Public method to refresh heart system when max health changes
        public void RefreshHeartSystem()
        {
            if (useHeartVisual && _playerController != null)
            {
                uint newMaxHealth = _playerController.Health.GetHealthAmount();
                if (newMaxHealth != maxHealth)
                {
                    Debug.Log($"[PlayerUIController] Refreshing heart system - Old max: {maxHealth}, New max: {newMaxHealth}");
                    SetupHeartVisualSystem();
                }
            }
        }

        // Public method to force refresh heart system (for external calls)
        public void ForceRefreshHeartSystem()
        {
            if (useHeartVisual && _playerController != null)
            {
                Debug.Log("[PlayerUIController] Force refreshing heart system");
                SetupHeartVisualSystem();
            }
        }

        private void OnDestroy()
        {
            // Cleanup event subscriptions
            if (_playerController != null)
            {
                _playerController.OnTakingDamage -= OnTakingDamageVisual;
            }

            // Cleanup credits coroutine
            if (creditsScrollCoroutine != null)
            {
                StopCoroutine(creditsScrollCoroutine);
                creditsScrollCoroutine = null;
            }

            isCreditsPlaying = false;
        }
    }
}