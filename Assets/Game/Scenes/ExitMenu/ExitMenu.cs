using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitMenu : MonoBehaviour
{
    // Fungsi dipanggil saat tombol "No" diklik
    public void OnNoButtonClicked()
    {
        // Ganti ke scene "MainMenu"
        SceneManager.LoadScene("MainMenu");
    }

    // Fungsi dipanggil saat tombol "Yes" diklik
    public void OnYesButtonClicked()
    {
        // Keluar dari aplikasi
        Application.Quit();

        // Debug untuk editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
