using UnityEngine;
using UnityEngine.SceneManagement;

public class NextButtonManager : MonoBehaviour
{
    public void LoadNextLevel()
    {
        string currentScene = PlayerPrefs.GetString("CurrentLevel", "Level_01");

        if (currentScene == "Level_01")
        {
            SceneManager.LoadScene("Level_02");
            PlayerPrefs.SetString("CurrentLevel", "Level_02");
        }
        else if (currentScene == "Level_02")
        {
            SceneManager.LoadScene("Level_03");
            PlayerPrefs.SetString("CurrentLevel", "Level_03");
        }
        else if (currentScene == "Level_03")
        {
            // Jika level terakhir, bisa kembali ke menu atau buat ucapan tamat
            SceneManager.LoadScene("MainMenu"); 
        }
    }
}
