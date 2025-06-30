using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Game;

public delegate void Win();

namespace Game.Buildings.House
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class HouseController : MonoBehaviour
    {
        public event Win OnWin;

        [Header("Level Progression")]
        [SerializeField] private bool autoProgressToNextLevel = true;
        [SerializeField] private float delayBeforeSceneChange = 2.0f;
        [SerializeField] private bool showWinMessage = true;

        private void Awake()
        {
            // Tidak perlu mencari ChangeScene component, kita akan menggunakan SceneManager langsung
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID) return;

            OnWin?.Invoke();

            if (showWinMessage)
            {
                Debug.Log("🎉 Level Complete! Moving to next level...");
            }

            if (autoProgressToNextLevel)
            {
                // Delay sebelum pindah scene untuk memberikan feedback
                StartCoroutine(DelayedProgressToNextLevel());
            }
        }

        private IEnumerator DelayedProgressToNextLevel()
        {
            yield return new WaitForSeconds(delayBeforeSceneChange);
            ProgressToNextLevel();
        }

        private void ProgressToNextLevel()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            switch (currentScene)
            {
                case SceneNames.Level_01:
                    Debug.Log("Progressing from Level 1 to Level 2");
                    LoadScene(SceneNames.Level_02);
                    break;

                case SceneNames.Level_02:
                    Debug.Log("Progressing from Level 2 to Level 3");
                    LoadScene(SceneNames.Level_03);
                    break;

                case SceneNames.Level_03:
                    Debug.Log("Level 3 completed! Going to Credits");
                    LoadScene(SceneNames.Credits);
                    break;

                case SceneNames.Level_Bonus:
                    Debug.Log("Bonus Level completed! Going to Credits");
                    LoadScene(SceneNames.Credits);
                    break;

                default:
                    Debug.LogWarning($"No next level defined for scene: {currentScene}. Going to Main Menu.");
                    LoadScene(SceneNames.MainMenu);
                    break;
            }
        }

        private void LoadScene(string sceneName)
        {
            // Reset session stats saat pindah level
            GameStats.ResetSessionStats();

            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        // Method untuk manual progression (bisa dipanggil dari UI button)
        public void ManualProgressToNextLevel()
        {
            ProgressToNextLevel();
        }

        // Method untuk kembali ke main menu
        public void GoToMainMenu()
        {
            LoadScene(SceneNames.MainMenu);
        }

        // Method untuk restart level
        public void RestartLevel()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
    }
}