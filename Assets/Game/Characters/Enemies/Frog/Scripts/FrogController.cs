using Game.Characters.Components;
using Game.Helpers;
using UnityEngine;

namespace Game.Characters.Enemies.Frog
{
    public sealed class FrogController : EnemyController
    {
        [SerializeField, Range(0.0f, float.MaxValue)]
        private float jumpForce = 5.0f;

        private void Start()
        {
            GroundCheck = GetComponentInChildren<GroundCheck>();

            Rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

            var frogTimerName = $"Timer_{name}";
            Timer.Create(Jump, 4.0f, true, $"{frogTimerName}_Jump");
        }

        private void Jump()
        {
            checkForEndOfPlatform = false;
            Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Rb.linearVelocity = new Vector2(AxisValue * moveSpeed, jumpForce);

            Timer.Create(() =>
            {
                if (!Health.IsAlive) return;

                checkForEndOfPlatform = true;
                Rb.linearVelocity = Vector2.zero;
                Rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            }, 1.0f, false, $"Timer_{name}_Fall");
        }
    }
}