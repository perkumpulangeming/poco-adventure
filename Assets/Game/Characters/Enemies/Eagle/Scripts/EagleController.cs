using System.Linq;
using Game.Characters.Player;
using Game.Helpers;
using UnityEngine;

namespace Game.Characters.Enemies.Eagle
{
    public sealed class EagleController : EnemyController
    {
        private PlayerController Player { get; set; }
        private Timer StopChaseTimer { get; set; }
        private Timer _startMoveTimer;
        private Timer _stopMoveTimer;

        private bool _playerEscaped;

        private Vector3 StartPosition { get; set; } = Vector3.zero;

        private void Start()
        {
            StartPosition = transform.position;
            enabled = false;
        }

        private void Update()
        {
            Move();
        }

        protected override void Move()
        {
            var currentTransform = transform;
            var position = currentTransform.position;

            if (_playerEscaped)
            {
                position = Vector2.MoveTowards(position, StartPosition,
                    moveSpeed * Time.deltaTime);

                currentTransform.position = position;
                currentTransform.localScale =
                    new Vector3(StartPosition.x - position.x > 0.0f ? -1.0f : 1.0f, 1.0f, 1.0f);

                return;
            }

            var playerPosition = Player.transform.position;

            position = Vector2.MoveTowards(position, playerPosition,
                moveSpeed * Time.deltaTime);

            currentTransform.position = position;
            currentTransform.localScale = new Vector3(playerPosition.x - position.x > 0.0f ? -1.0f : 1.0f, 1.0f, 1.0f);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID || col.isTrigger ||
                !col.TryGetComponent(out PlayerController player)) return;
            if (StopChaseTimer is not null)
            {
                StopChaseTimer.ResetTime();
                StopChaseTimer.DisableTimer();
            }

            _playerEscaped = false;
            enabled = true;
            Player = player;

            if (_startMoveTimer == null || !_startMoveTimer.IsValid())
            {
                _startMoveTimer = Timer.Create(() => enabled = true, 0.05f, true);
                _stopMoveTimer = Timer.Create(() => enabled = false, 0.5f, true);
            }
            else
            {
                _startMoveTimer.EnableTimer();
                _stopMoveTimer.EnableTimer();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.isTrigger || other.gameObject.layer != Global.Layers.PlayerID) return;

            if (StopChaseTimer is null)
            {
                StopChaseTimer = Timer.Create(() =>
                {
                    _playerEscaped = true;
                    Timer.RemoveTimer(StopChaseTimer);
                    StopChaseTimer = null;
                }, 2.0f);
            }
            else
            {
                StopChaseTimer.EnableTimer();
            }

            _startMoveTimer.DisableTimer();
            _stopMoveTimer.DisableTimer();
        }

        public override void TakeDamage(uint damageAmount)
        {
            base.TakeDamage(damageAmount);
            TemporaryStop();
        }

        protected override void Attack(Collision2D col)
        {
            if (col.gameObject.layer != Global.Layers.PlayerID ||
                !col.gameObject.TryGetComponent<PlayerController>(out var player)) return;
            if (player.enabled == false)
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

            if (!player.Health.IsAlive)
                _playerEscaped = true;

            TemporaryStop();
        }

        private void TemporaryStop()
        {
            moveSpeed = 0;
            Timer.Create(() => moveSpeed = PreviousMoveSpeed, 0.5f);
        }
    }
}