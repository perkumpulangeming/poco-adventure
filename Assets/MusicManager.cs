using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;
    public AudioClip soundBackSound;

    private AudioSource audioSource;
    private static MusicManager instance;

    void Awake()
    {
        // Singleton: pastikan hanya satu MusicManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Cek dan ambil AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("❌ AudioSource tidak ditemukan di MusicManager!");
            return;
        }

        Debug.Log("✅ MusicManager aktif di scene: " + SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        // Pastikan musik dimainkan saat scene pertama dibuka
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ AudioSource belum tersedia.");
            return;
        }

        AudioClip clipToPlay = soundBackSound;

        if (sceneName == "Level_01")
            clipToPlay = sound1;
        else if (sceneName == "Level_02")
            clipToPlay = sound2;
        else if (sceneName == "Level_03")
            clipToPlay = sound3;

        Debug.Log($"🎵 Scene aktif: {sceneName}, Clip yang dipilih: {(clipToPlay ? clipToPlay.name : "null")}");

        if (audioSource.clip != clipToPlay && clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("▶️ Musik diputar: " + clipToPlay.name);
        }
        else
        {
            Debug.Log("ℹ️ Musik sudah sesuai atau clip null, tidak memutar ulang.");
        }
    }
}
