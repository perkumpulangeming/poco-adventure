using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void GoToCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToLevel1()
    {
        SceneManager.LoadScene("Level_01");
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
