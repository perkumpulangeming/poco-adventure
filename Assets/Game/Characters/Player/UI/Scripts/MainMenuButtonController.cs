using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Characters.Player.UI
{
    public sealed class MainMenuButtonController : MonoBehaviour
    {
        private void Awake()
        {
            Cursor.visible = true;
        }

        public void OnClick()
        {
            SceneManager.LoadScene(Global.Levels.MainMenu);
        }
    }
}