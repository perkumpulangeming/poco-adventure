using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Management
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;

        private void Awake()
        {
            // Singleton pattern
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
            if (showDebugInfo)
            {
                Debug.Log("💎 Gem System Active! 💎\n" +
                         "- Gems are now persistent across levels\n" +
                         "- Press F1 to see current stats\n" +
                         "- Use 'Play' to continue with current gems\n" +
                         "- Use 'New Game' to reset everything");
            }
        }

        private void Update()
        {
            // Debug shortcut
            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameStats.ShowStats();
            }
        }

        public void LoadLevel(int levelIndex)
        {
            // Only reset level-specific stats, keep gems
            GameStats.ResetLevelStats();
            SceneManager.LoadScene(levelIndex);
        }

        public void RestartCurrentLevel()
        {
            // Only reset level-specific stats, keep gems
            GameStats.ResetLevelStats();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(Global.Levels.MainMenu);
        }

        public void QuitGame()
        {
            GameStats.SaveData();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameStats.SaveData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                GameStats.SaveData();
            }
        }

        private void OnDestroy()
        {
            GameStats.SaveData();
        }
    }
}
