using Game.Characters.Components;
using Game.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters
{
    public delegate float DeathNotifier();

    [RequireComponent(typeof(Health))]
    public abstract class EntityController : MonoBehaviour
    {
        public static event DeathNotifier OnEntityDeath;
        public event DeathNotifier OnDeath;

        [field: Range(0.0f, float.MaxValue)] public float moveSpeed = 5.0f;

        [SerializeField] protected uint attackDamage = 1;

        // Components
        public GroundCheck GroundCheck { get; protected set; }
        public Health Health { get; protected set; }
        protected Rigidbody2D Rb;

        // Internal
        public bool Running { get; private set; }
        public bool Falling => Rb.linearVelocity.y < -Mathf.Epsilon;

        protected float AxisValue = 1.0f;
        private bool _isDead;

        protected virtual void Move()
        {
            Rb.linearVelocity = new Vector2(AxisValue * moveSpeed, Rb.linearVelocity.y);
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (Rb.bodyType == RigidbodyType2D.Static) return;

            if (!enabled)
            {
                Running = false;

                return;
            }

            AxisValue = context.ReadValue<float>();

            if (context.started)
            {
                transform.localScale = new Vector3(AxisValue, 1.0f, 1.0f);
                Running = true;
            }

            if (context.canceled)
                Running = false;

            Rb.linearVelocity = new Vector2(AxisValue * moveSpeed, Rb.linearVelocity.y);
        }

        public virtual void TakeDamage(uint damageAmount)
        {
            Health.TakeDamage(damageAmount);

            if (!Health.IsAlive && !_isDead)
            {
                _isDead = true;

                OnEntityDeath?.Invoke();
                var deathAnimationLength = (float)OnDeath?.Invoke();

                enabled = false;
                Rb.simulated = false;
                Rb.bodyType = RigidbodyType2D.Static;
                var colliders = GetComponents<BoxCollider2D>();
                foreach (var collider in colliders)
                {
                    collider.enabled = false;
                }

                Timer.Create(() =>
                {
                    if (this != null)
                        Destroy(gameObject);
                }, deathAnimationLength);
            }
        }
    }
}