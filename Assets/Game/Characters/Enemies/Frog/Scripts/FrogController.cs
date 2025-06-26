using Game.Characters.Components;
using Game.Characters.Player;
using Game.Helpers;
using UnityEngine;

namespace Game.Characters.Enemies.Frog
{
    public sealed class FrogController : EnemyController
    {
        [SerializeField, Range(0.0f, float.MaxValue)]
        private float jumpForce = 5.0f;

        [SerializeField, Range(1f, 5f)]
        private float minMoveDistance = 2f;

        [SerializeField, Range(3f, 8f)]
        private float maxMoveDistance = 6f;

        [SerializeField, Range(2f, 10f)]
        private float chaseRange = 5f;

        private Vector3 startPosition;
        private float distanceTraveled = 0f;
        private float currentMoveTarget;
        private PlayerController player;
        private bool chasingPlayer = false;

        private void Start()
        {
            GroundCheck = GetComponentInChildren<GroundCheck>();
            Rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            checkForEndOfPlatform = false;

            startPosition = transform.position;
            SetRandomMoveTarget();

            Timer.Create(Jump, 4.0f, true, $"Timer_{name}_Jump");
        }

        private void SetRandomMoveTarget()
        {
            currentMoveTarget = Random.Range(minMoveDistance, maxMoveDistance);
        }

        private void Update()
        {
            // Auto cari player di scene
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
                return;
            }

            if (player != null && player.Health.IsAlive)
            {
                var distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                chasingPlayer = distanceToPlayer <= chaseRange;
            }
            else
            {
                chasingPlayer = false;
            }
        }

        private void Jump()
        {
            if (!Health.IsAlive) return;

            Rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (chasingPlayer && player != null && player.Health.IsAlive)
            {
                float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
                AxisValue = direction;
                transform.eulerAngles = direction > 0 ? Vector3.zero : new Vector3(0.0f, 180.0f, 0.0f);
            }

            Rb.linearVelocity = new Vector2(AxisValue * moveSpeed, jumpForce);

            Timer.Create(() =>
            {
                if (!Health.IsAlive) return;

                distanceTraveled += Mathf.Abs(transform.position.x - startPosition.x);

                if (!chasingPlayer && distanceTraveled >= currentMoveTarget)
                {
                    AxisValue = -AxisValue;
                    transform.eulerAngles = AxisValue > 0 ? Vector3.zero : new Vector3(0.0f, 180.0f, 0.0f);
                    distanceTraveled = 0f;
                    startPosition = transform.position;
                    SetRandomMoveTarget();
                }

                Rb.linearVelocity = Vector2.zero;
                Rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;

            }, 1.0f, false, $"Timer_{name}_Fall");
        }
    }
}
