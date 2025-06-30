using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class ExitMenuController : MonoBehaviour
{
    private void Start()
    {
        // Tampilkan cursor
        Cursor.visible = true;

        // Log untuk debug
        string lastLevel = PlayerPrefs.GetString("LastPlayedLevel", SceneNames.MainMenu);
        Debug.Log($"Exit Menu opened from level: {lastLevel}");
    }

    private void Update()
    {
        // ESC untuk langsung kembali ke game (sama seperti tombol No)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnNoClicked();
        }
    }

    // Method untuk tombol "Yes" - Kembali ke Main Menu
    public void OnYesClicked()
    {
        Debug.Log("Yes clicked - Going to Main Menu");

        // Hapus data level yang disimpan
        PlayerPrefs.DeleteKey("LastPlayedLevel");

        // Reset session stats
        GameStats.ResetSessionStats();

        // Kembali ke Main Menu
        SceneManager.LoadScene(SceneNames.MainMenu);
    }

    // Method untuk tombol "No" - Lanjut main game
    public void OnNoClicked()
    {
        Debug.Log("No clicked - Returning to game");

        // Ambil level yang disimpan
        string lastLevel = PlayerPrefs.GetString("LastPlayedLevel", SceneNames.MainMenu);

        // Validasi apakah level valid
        if (IsValidGameLevel(lastLevel))
        {
            Debug.Log($"Returning to level: {lastLevel}");
            SceneManager.LoadScene(lastLevel);
        }
        else
        {
            Debug.LogWarning($"Invalid level: {lastLevel}. Going to Main Menu instead.");
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }

    // Method untuk tombol "Exit Game" - Keluar dari aplikasi
    public void OnExitGameClicked()
    {
        Debug.Log("Exit Game clicked - Quitting application");

        // Hapus data level yang disimpan
        PlayerPrefs.DeleteKey("LastPlayedLevel");

        // Keluar dari aplikasi
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private bool IsValidGameLevel(string levelName)
    {
        return levelName == SceneNames.Level_01 ||
               levelName == SceneNames.Level_02 ||
               levelName == SceneNames.Level_03 ||
               levelName == SceneNames.Level_Bonus;
    }
}