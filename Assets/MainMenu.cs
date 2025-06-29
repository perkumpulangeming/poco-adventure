using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Fungsi dipanggil saat tombol "No" diklik
    public void MainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}