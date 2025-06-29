using UnityEngine;

namespace Game.Characters.Player
{
    public sealed class PlayerDeathZoneController : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.TryGetComponent(out PlayerController player)) return;
            player.TakeDamage(player.Health.GetHealthAmount());
        }
    }
}