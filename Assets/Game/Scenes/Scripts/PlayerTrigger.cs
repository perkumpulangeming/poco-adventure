using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("House"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Level_03")
            {
                SceneManager.LoadScene("LevelEnd");
            }
            else
            {
                SceneManager.LoadScene("NextLevel");
            }
        }
    }
}
