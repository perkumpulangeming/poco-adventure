using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class ChangeScene : MonoBehaviour
{
    [Header("Scene Navigation")]
    [SerializeField] private bool resetStatsOnSceneChange = true;
    [SerializeField] private bool enableDebugLogging = true;

    #region Public Scene Change Methods

    // Main Menu Navigation
    public void GoToMainMenu()
    {
        LoadScene(GameScene.MainMenu);
    }

    public void GoToCredits()
    {
        LoadScene(GameScene.Credits);
    }

    public void GoToExitMenu()
    {
        LoadScene(GameScene.ExitMenu);
    }

    public void GoToShopMenu()
    {
        LoadScene(GameScene.ShopMenu);
    }

    // Level Navigation
    public void GoToLevel1()
    {
        LoadScene(GameScene.Level_01);
    }

    public void GoToLevel2()
    {
        LoadScene(GameScene.Level_02);
    }

    public void GoToLevel3()
    {
        LoadScene(GameScene.Level_03);
    }

    public void GoToBonusLevel()
    {
        LoadScene(GameScene.Level_Bonus);
    }

    // Navigation by String (untuk backward compatibility)
    public void GoToScene(string sceneName)
    {
        LoadSceneByName(sceneName);
    }

    // Navigation by Enum
    public void GoToScene(GameScene scene)
    {
        LoadScene(scene);
    }

    // Level progression (next/previous)
    public void GoToNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case SceneNames.Level_01:
                LoadScene(GameScene.Level_02);
                break;
            case SceneNames.Level_02:
                LoadScene(GameScene.Level_03);
                break;
            case SceneNames.Level_03:
                LoadScene(GameScene.Level_Bonus);
                break;
            case SceneNames.Level_Bonus:
                LoadScene(GameScene.MainMenu); // Kembali ke menu setelah bonus level
                break;
            default:
                LogDebug($"No next level defined for scene: {currentScene}");
                break;
        }
    }

    public void GoToPreviousLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        switch (currentScene)
        {
            case SceneNames.Level_02:
                LoadScene(GameScene.Level_01);
                break;
            case SceneNames.Level_03:
                LoadScene(GameScene.Level_02);
                break;
            case SceneNames.Level_Bonus:
                LoadScene(GameScene.Level_03);
                break;
            default:
                LoadScene(GameScene.MainMenu); // Default kembali ke menu
                break;
        }
    }

    // Restart current level
    public void RestartCurrentLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadSceneByName(currentScene);
    }

    #endregion

    #region Core Scene Loading Methods

    private void LoadScene(GameScene scene)
    {
        string sceneName = GetSceneName(scene);
        LoadSceneByName(sceneName);
    }

    private void LoadSceneByName(string sceneName)
    {
        LogDebug($"Loading scene: {sceneName}");

        // Reset stats jika diaktifkan dan scene adalah level atau menu utama
        if (resetStatsOnSceneChange && ShouldResetStats(sceneName))
        {
            GameStats.ResetSessionStats();
            LogDebug("Session stats reset");
        }

        // Load scene
        SceneManager.LoadScene(sceneName);
    }

    private string GetSceneName(GameScene scene)
    {
        return scene switch
        {
            GameScene.MainMenu => SceneNames.MainMenu,
            GameScene.Credits => SceneNames.Credits,
            GameScene.ExitMenu => SceneNames.ExitMenu,
            GameScene.ShopMenu => SceneNames.ShopMenu,
            GameScene.Level_01 => SceneNames.Level_01,
            GameScene.Level_02 => SceneNames.Level_02,
            GameScene.Level_03 => SceneNames.Level_03,
            GameScene.Level_Bonus => SceneNames.Level_Bonus,
            _ => SceneNames.MainMenu
        };
    }

    private bool ShouldResetStats(string sceneName)
    {
        // Reset stats saat masuk ke main menu atau mulai level baru
        return sceneName == SceneNames.MainMenu ||
               sceneName == SceneNames.Level_01 ||
               sceneName == SceneNames.Level_02 ||
               sceneName == SceneNames.Level_03 ||
               sceneName == SceneNames.Level_Bonus;
    }

    #endregion

    #region Utility Methods

    public void ExitGame()
    {
        LogDebug("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public bool IsInLevel()
    {
        string currentScene = GetCurrentSceneName();
        return currentScene.Contains("Level_");
    }

    public bool IsInMenu()
    {
        string currentScene = GetCurrentSceneName();
        return currentScene == SceneNames.MainMenu ||
               currentScene == SceneNames.Credits ||
               currentScene == SceneNames.ExitMenu ||
               currentScene == SceneNames.ShopMenu;
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[ChangeScene] {message}");
        }
    }

    #endregion

    #region Input Handling

    void Update()
    {
        HandleEscapeInput();
    }

    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        string currentScene = GetCurrentSceneName();

        switch (currentScene)
        {
            case SceneNames.Credits:
                GoToMainMenu();
                break;
            case SceneNames.ExitMenu:
                GoToMainMenu();
                break;
            case SceneNames.ShopMenu:
                GoToMainMenu();
                break;
            // Untuk level, bisa kembali ke menu atau pause game
            case SceneNames.Level_01:
            case SceneNames.Level_02:
            case SceneNames.Level_03:
            case SceneNames.Level_Bonus:
                // Implementasi pause menu bisa ditambahkan di sini
                LogDebug("ESC pressed in level - implement pause menu here");
                break;
        }
    }

    #endregion
}
