using UnityEngine;

namespace Game.Scenes.MainMenu
{
    public sealed class ExitButtonController : MonoBehaviour
    {
        public void OnClick()
        {
            Application.Quit();
        }
    }
}