using UnityEngine;

public class LevelTracker : MonoBehaviour
{
    [SerializeField] private string currentLevelName;

    void Start()
    {
        PlayerPrefs.SetString("CurrentLevel", currentLevelName);
    }
}
