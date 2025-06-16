using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.Scenes.MainMenu
{
    public sealed class PlayButtonController : MonoBehaviour
    {
        public void OnClick()
        {
            SceneManager.LoadScene(Global.Levels.Main);

            InputSystem.EnableDevice(Keyboard.current);
        }
    }
}