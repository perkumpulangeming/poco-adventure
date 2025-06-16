using UnityEngine;

namespace Game.Characters.Components
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class GroundCheck : MonoBehaviour
    {
        private BoxCollider2D _col;

        public bool Grounded { get; private set; }

        private void Awake()
        {
            _col = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            Grounded = _col.IsTouchingLayers(Global.Layers.GroundID);
        }
    }
}