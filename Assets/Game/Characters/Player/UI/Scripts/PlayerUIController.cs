using Game.Buildings.House;
using Game.Helpers;
using Game.Items.Gem;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Game.Characters.Player.UI
{
    public sealed class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gemsCountText;
        [SerializeField] private TextMeshProUGUI killsCountText;
        [SerializeField] private TextMeshProUGUI heartsCountText;

        [SerializeField] private Image[] winScreenImages;

        [SerializeField] private Canvas hud;
        [SerializeField] private Canvas escape;

        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private TextMeshProUGUI escapeGemsCountText;
        [SerializeField] private TextMeshProUGUI escapeKillsCountText;

        // Simple Heart Visual System
        [Header("Heart Visual System")]
        [SerializeField] private bool useHeartVisual = false; // Toggle between text and heart visual
        [SerializeField] private Image referenceHeartImage; // Reference heart image to copy
        [SerializeField] private Transform heartContainer; // Where to put copied hearts
        [SerializeField] private float heartSpacing = 60f; // Distance between hearts

        private const string PlayerDeathText = "YOU ARE DEAD! GO HOME, NERD!";

        private PlayerController _playerController;
        private Camera _playerCamera;
        
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
                hud.gameObject.SetActive(false);
                escape.gameObject.SetActive(true);
                _playerCamera.gameObject.SetActive(false);

                winText.gameObject.SetActive(true);
                winText.text = PlayerDeathText;
            }, deathAnimationLength);

            return float.NaN;
        }

        private void OnEscapePressed()
        {
            hud.gameObject.SetActive(!hud.gameObject.activeSelf);
            escape.gameObject.SetActive(!escape.gameObject.activeSelf);

            _playerCamera.gameObject.SetActive(hud.gameObject.activeSelf);

            Cursor.visible = escape.gameObject.activeSelf;
            _playerController.EnableMovement(hud.gameObject.activeSelf);
        }

        private void OnItemCollect()
        {
            // Update UI to reflect current gem count from GameStats
            gemsCountText.text = GameStats.CollectedGems.ToString();
        }

        private void OnWin()
        {
            hud.gameObject.SetActive(false);
            escape.gameObject.SetActive(true);

            _playerCamera.gameObject.SetActive(false);
            _playerController.gameObject.SetActive(false);

            Cursor.visible = true;
            InputSystem.DisableDevice(Keyboard.current);

            foreach (var image in winScreenImages)
                image.gameObject.SetActive(true);

            escapeGemsCountText.text = gemsCountText.text;
            escapeKillsCountText.text = killsCountText.text;

            winText.gameObject.SetActive(true);
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
        }
    }
}