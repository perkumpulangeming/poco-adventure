using UnityEngine;
using UnityEngine.SceneManagement;

public class NextButtonManager : MonoBehaviour
{
    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level_01")
        {
            SceneManager.LoadScene("Level_02");
        }
        else if (currentScene == "Level_02")
        {
            SceneManager.LoadScene("Level_03");
        }
        else if (currentScene == "Level_03")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
