using System;
using Game.Characters.Player;
using UnityEngine;

namespace Game.Items
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class ItemController : MonoBehaviour
    {
        protected static PlayerController PlayerController;
        private float DeferredDestroyTime { get; set; } = 0;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID) return;
            PlayerController = col.GetComponent<PlayerController>();

            OnPickUp();
            Destroy(gameObject, DeferredDestroyTime);
        }

        protected virtual void OnPickUp()
        {
            throw new NotImplementedException();
        }
    }
}