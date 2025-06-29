using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMusicManager : MonoBehaviour
{
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;

    private AudioSource audioSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Tetap aktif antar scene
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Level_01":
                PlayMusic(level1Music);
                break;
            case "Level_02":
                PlayMusic(level2Music);
                break;
            case "Level_03":
                PlayMusic(level3Music);
                break;
            default:
                break;
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
