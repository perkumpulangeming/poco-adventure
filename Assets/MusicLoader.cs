using UnityEngine;

public class MusicLoader : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectOfType<MusicManager>() == null)
        {
            GameObject musicPrefab = Resources.Load<GameObject>("MusicManager");
            Instantiate(musicPrefab);
        }
    }
}
