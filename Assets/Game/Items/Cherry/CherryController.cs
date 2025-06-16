using Game.Helpers;
using UnityEngine;

namespace Game.Items.Cherry
{
    public sealed class CherryController : ItemController
    {
        [SerializeField] private uint healAmount = 25;
        [SerializeField] private float speedModifier = 1.1f;
        [SerializeField] private float speedBoostDuration = 2.0f;

        protected override void OnPickUp()
        {
            PlayerController.HealUp(healAmount);

            PlayerController.SetMoveSpeed(PlayerController.moveSpeed * speedModifier);
            Timer.Create(() =>
                {
                    if (PlayerController != null)
                        PlayerController.SetMoveSpeed(PlayerController.moveSpeed / speedModifier);
                },
                speedBoostDuration);
        }
    }
}