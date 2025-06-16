using System;
using System.Linq;
using Game.Characters.Components;
using Game.Characters.Interfaces;
using Game.Characters.Player;
using Game.Helpers;
using UnityEngine;

namespace Game.Characters.Enemies
{
    public abstract class EnemyController : EntityController, IDamageable
    {
        [SerializeField] protected bool checkForEndOfPlatform = true;
        [SerializeField] private Transform endOfPlatformCheck;

        protected float PreviousMoveSpeed;
        private const float RayCastRange = 0.5f;

        private void OnValidate()
        {
            if (endOfPlatformCheck is null)
                throw new NullReferenceException(
                    $"Isn't given a reference to the transform child {nameof(endOfPlatformCheck)}.");
        }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Health = GetComponent<Health>();

            PreviousMoveSpeed = moveSpeed;

            Timer.Create(() =>
            {
                if (!checkForEndOfPlatform) return;

                var hitResult = Physics2D.Raycast(endOfPlatformCheck.position, Vector2.down, RayCastRange);

                if (hitResult.collider) return;

                AxisValue = -AxisValue;

                transform.eulerAngles = AxisValue > 0 ? Vector3.zero : new Vector3(0.0f, 180.0f, 0.0f);
            }, 0.1f, true, $"Timer_{name}");
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            Attack(col);
        }

        protected virtual void Attack(Collision2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID ||
                !col.gameObject.TryGetComponent<PlayerController>(out var player)) return;
            if (!player.enabled)
                return;

            player.enabled = false;

            var enemyPosition = transform.position;
            var dir = (col.contacts.First().point - new Vector2(enemyPosition.x, enemyPosition.y))
                .normalized;

            col.rigidbody.linearVelocity =
                new Vector2(dir.x * player.moveSpeed * 2.0f, 2.5f);

            Timer.Create(() =>
            {
                col.rigidbody.linearVelocity = Vector2.zero;
                player.enabled = true;
            }, 0.5f);

            player.TakeDamage(attackDamage);
        }
    }
}