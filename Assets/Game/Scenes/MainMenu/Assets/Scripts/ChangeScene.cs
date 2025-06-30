using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class ChangeScene : MonoBehaviour
{
    public void GoToCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void GoToMainMenu()
    {
        // Reset session stats when going to main menu
        GameStats.ResetSessionStats();
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToLevel1()
    {
        // Reset session stats when starting level
        GameStats.ResetSessionStats();
        SceneManager.LoadScene("Level_01");
    }

    public void GoToShop()
    {
        SceneManager.LoadScene("ShopMenu");
    }

    public void GoToHelp()
    {
        SceneManager.LoadScene("Help");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed"); // agar terlihat saat test di editor
    }

    void Update()
    {
        // ESC hanya aktif di scene Credits agar tidak mengganggu scene lain
        if (SceneManager.GetActiveScene().name == "Credits")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoToMainMenu();
            }
        }
    }
}
