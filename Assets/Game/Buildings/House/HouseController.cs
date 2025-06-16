using UnityEngine;

public delegate void Win();

namespace Game.Buildings.House
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class HouseController : MonoBehaviour
    {
        public event Win OnWin;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID) return;

            OnWin?.Invoke();
        }
    }
}