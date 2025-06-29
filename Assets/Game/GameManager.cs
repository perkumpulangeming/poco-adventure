using UnityEngine;

namespace Game
{
    /// <summary>
    /// Singleton GameManager that handles global game state and persistent data.
    /// This ensures gems persist across scene changes and game sessions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing instance
                    _instance = FindObjectOfType<GameManager>();

                    // Create new instance if none exists
                    if (_instance == null)
                    {
                        GameObject gameManagerObject = new GameObject("GameManager");
                        _instance = gameManagerObject.AddComponent<GameManager>();
                        DontDestroyOnLoad(gameManagerObject);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            // Ensure only one GameManager exists
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            // Initialize persistent data when game starts
            // The GameStats.CollectedGems will automatically load from PlayerPrefs
            Debug.Log($"GameManager initialized. Current gems: {GameStats.CollectedGems}");
        }

        /// <summary>
        /// Add gems to the player's total. This will persist across sessions.
        /// </summary>
        /// <param name="amount">Number of gems to add</param>
        public static void AddGems(uint amount)
        {
            GameStats.CollectedGems += amount;
            Debug.Log($"Added {amount} gems. Total: {GameStats.CollectedGems}");
        }

        /// <summary>
        /// Get current gem count
        /// </summary>
        /// <returns>Total collected gems</returns>
        public static uint GetGems()
        {
            return GameStats.CollectedGems;
        }

        /// <summary>
        /// Reset all persistent data (use for new game/reset)
        /// </summary>
        public static void ResetAllProgress()
        {
            GameStats.ResetAllStats();
            Debug.Log("All game progress has been reset.");
        }

        /// <summary>
        /// Reset only session data (use when starting new level/returning to menu)
        /// </summary>
        public static void ResetSession()
        {
            GameStats.ResetSessionStats();
            Debug.Log("Session stats reset. Gems preserved.");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save data when app is paused
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Save data when app loses focus
                PlayerPrefs.Save();
            }
        }
    }
}
