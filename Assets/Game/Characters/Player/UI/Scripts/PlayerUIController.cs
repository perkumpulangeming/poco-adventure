using Game.Buildings.House;
using Game.Helpers;
using Game.Items.Gem;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

        private const string PlayerDeathText = "YOU ARE DEAD! GO HOME, NERD!";

        private PlayerController _playerController;
        private Camera _playerCamera;

        private void Awake()
        {
            Cursor.visible = false;

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
            heartsCountText.text = _playerController.Health.GetHealthAmount().ToString();
            InputSystem.EnableDevice(Keyboard.current);
        }

        private float OnKill()
        {
            killsCountText.text = (int.Parse(killsCountText.text) + 1).ToString();

            return float.NaN;
        }

        private void OnHealthChange(uint healthAmount)
        {
            heartsCountText.text = healthAmount.ToString();
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

        private void OnItemCollect() => gemsCountText.text = (int.Parse(gemsCountText.text) + 1).ToString();

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
    }
}