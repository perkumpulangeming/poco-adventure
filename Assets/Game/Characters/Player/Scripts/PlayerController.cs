using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Characters.Components;
using Game.Characters.Interfaces;

namespace Game.Characters.Player
{
    public delegate void StatsChangeNotifier(uint value);

    public delegate void HealthChangeNotifier();

    public delegate void InputEventNotifier();


    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(Animator))]
    public sealed class PlayerController : EntityController, IDamageable, IHealable
    {
        public event StatsChangeNotifier OnHealthChange;
        public event HealthChangeNotifier OnTakingDamage;
        public event InputEventNotifier OnEscapePressed;

        [SerializeField, Range(0.0f, float.MaxValue)]
        private float jumpForce = 5.0f;

        [SerializeField, Range(0.0f, float.MaxValue)]
        private float climbingForce = 2.0f;

        // Shooting System
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float shootCooldown = 0.3f;
        [SerializeField] private uint projectileDamage = 1;

        private float lastShootTime;

        private CapsuleCollider2D Capsule { get; set; }

        public bool IsCrouching { get; private set; }
        public bool IsOnLadder { get; private set; }
        private uint IsOnLadderCount { get; set; }

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Capsule = GetComponent<CapsuleCollider2D>();
            Health = GetComponent<Health>();
            GroundCheck = GetComponentInChildren<GroundCheck>();

            if (GroundCheck is null)
                throw new NullReferenceException(typeof(GroundCheck).ToString());
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.layer == Global.Layers.LadderID)
                IsOnLadderCount++;

            if (!col.isTrigger || col is not BoxCollider2D || !col.TryGetComponent(out IDamageable damageable))
                return;

            damageable.TakeDamage(attackDamage);

            if (((EntityController)damageable).Health.IsAlive)
                GameStats.KilledEnemies++;

            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, jumpForce);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer != Global.Layers.LadderID) return;

            IsOnLadderCount = Math.Clamp(IsOnLadderCount - 1, uint.MinValue, uint.MaxValue);

            if (IsOnLadderCount != 0) return;

            IsOnLadder = false;
            Rb.gravityScale = 1.0f;
        }

        public override void TakeDamage(uint damageAmount)
        {
            base.TakeDamage(damageAmount);
            OnHealthChange?.Invoke(Health.GetHealthAmount());

            if (Health.IsAlive)
                OnTakingDamage?.Invoke();
        }

        public void Jump(InputAction.CallbackContext context)
        {
            if (Rb.bodyType == RigidbodyType2D.Static) return;

            if (context.started && IsOnLadderCount > 0)
                IsOnLadder = true;

            if (IsOnLadder)
            {
                Rb.gravityScale = 0.0f;

                if (context.started)
                    Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, climbingForce);
                if (context.canceled)
                    Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, 0.0f);

                return;
            }

            if (context.started && GroundCheck.Grounded && !IsCrouching)
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, jumpForce);
        }

        public void Crouch(InputAction.CallbackContext context)
        {
            if (Rb.bodyType == RigidbodyType2D.Static) return;

            if (IsOnLadder)
            {
                Rb.gravityScale = 0.0f;

                if (context.started)
                    Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -climbingForce);
                if (context.canceled)
                    Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, 0.0f);

                return;
            }

            if (context.started)
            {
                if (IsOnLadderCount > 0)
                    IsOnLadder = true;

                if (!IsOnLadder)
                {
                    IsCrouching = true;

                    Capsule.offset = new Vector2(0.0f, -0.5f);
                    Capsule.size = new Vector2(0.96f, 0.96f);
                }
                else
                {
                    if (IsRoofAbove()) return;

                    IsCrouching = false;

                    Capsule.offset = new Vector2(0.0f, -0.3f);
                    Capsule.size = new Vector2(0.96f, 1.37f);
                }
            }

            if (!context.canceled || IsRoofAbove()) return;

            IsCrouching = false;

            Capsule.offset = new Vector2(0.0f, -0.3f);
            Capsule.size = new Vector2(0.96f, 1.37f);
        }

        public void EscapeMenu(InputAction.CallbackContext context)
        {
            if (context.started)
                OnEscapePressed?.Invoke();
        }

        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
            Rb.linearVelocity = new Vector2(AxisValue * newSpeed, Rb.linearVelocity.y);
        }

        private bool IsRoofAbove()
        {
            var hitResult = Physics2D.Raycast(Rb.position, Vector2.up, 2.0f, Global.Layers.GroundID);

            return hitResult.collider is not null &&
                   !hitResult.collider.IsTouchingLayers(Global.Layers.GroundID);
        }

        public void HealUp(uint healAmount)
        {
            Health.HealUp(healAmount);
            OnHealthChange?.Invoke(Health.GetHealthAmount());
        }

        public void EnableMovement(bool enable)
        {
            Rb.bodyType = enable ? RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
        }

        // Shooting System Methods
        public void Shoot(InputAction.CallbackContext context)
        {
            if (!context.started || Rb.bodyType == RigidbodyType2D.Static)
                return;

            // Check cooldown
            if (Time.time - lastShootTime < shootCooldown)
                return;

            // Check if we have projectile prefab and fire point
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning("Projectile prefab or fire point not assigned!");
                return;
            }

            // Tentukan arah tembakan (sesuai arah player menghadap)
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // Instantiate projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Initialize projectile
            if (projectile.TryGetComponent<Projectile>(out var projectileScript))
            {
                projectileScript.Initialize(shootDirection, projectileDamage);
            }

            lastShootTime = Time.time;
        }

        public void ShootAtMouse(InputAction.CallbackContext context)
        {
            Debug.Log("ShootAtMouse called! Context: " + context.phase);

            if (!context.started || Rb.bodyType == RigidbodyType2D.Static)
                return;

            // Check cooldown
            if (Time.time - lastShootTime < shootCooldown)
            {
                Debug.Log("Still in cooldown!");
                return;
            }

            // Check if we have projectile prefab and fire point
            if (projectilePrefab == null || firePoint == null)
            {
                Debug.LogWarning("Projectile prefab or fire point not assigned!");
                return;
            }

            Debug.Log("Creating projectile...");

            // Get mouse position in world space
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            // Calculate direction from fire point to mouse
            Vector2 shootDirection = (mousePosition - firePoint.position).normalized;

            // Instantiate projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Initialize projectile
            if (projectile.TryGetComponent<Projectile>(out var projectileScript))
            {
                projectileScript.Initialize(shootDirection, projectileDamage);
                Debug.Log("Projectile created and initialized!");
            }

            lastShootTime = Time.time;
        }
    }
}