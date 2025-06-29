using UnityEngine;
using Game.Characters.Interfaces;

namespace Game.Characters.Player
{
    public class ShootingSystem : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private float shootRange = 8f;
        [SerializeField] private float projectileSize = 0.8f;
        [SerializeField] private uint projectileDamage = 1;
        
        [Header("Shooting Settings")]
        [SerializeField] private float fireRate = 0.6f;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private KeyCode shootKey = KeyCode.E;
        
        private float lastShootTime;
        private PlayerController playerController;
        private bool isFacingRight = true;
        private float projectileSpeed = 6f; // Fixed reasonable speed
        private float rotationSpeed = 360f; // Fixed rotation speed
        private float calculatedTravelTime;
        
        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            
            // Calculate travel time based on range and speed
            calculatedTravelTime = shootRange / projectileSpeed;
            
            // Auto create shoot point if not assigned
            if (shootPoint == null)
            {
                GameObject shootPointObj = new GameObject("ShootPoint");
                shootPointObj.transform.SetParent(transform);
                shootPointObj.transform.localPosition = new Vector3(0.8f, 0f, 0f);
                shootPoint = shootPointObj.transform;
            }
        }
        
        private void Update()
        {
            // Recalculate travel time if range changed
            calculatedTravelTime = shootRange / projectileSpeed;
            
            UpdatePlayerDirection();
            HandleShooting();
        }
        
        private void UpdatePlayerDirection()
        {
            // Check player facing direction from scale
            if (transform.localScale.x > 0)
                isFacingRight = true;
            else if (transform.localScale.x < 0)
                isFacingRight = false;
        }
        
        private void HandleShooting()
        {
            // Check if can shoot (fire rate cooldown)
            if (Time.time - lastShootTime < fireRate) return;
            
            // Check E key press
            if (Input.GetKeyDown(shootKey))
            {
                Shoot();
            }
        }
        
        private void Shoot()
        {
            if (projectileSprite == null) return;
            
            // Determine shoot direction based on player facing
            Vector3 shootDirection = isFacingRight ? Vector3.right : Vector3.left;
            
            // Calculate target position
            Vector3 targetPos = shootPoint.position + (shootDirection * shootRange);
            
            // Create projectile
            CreateProjectile(shootDirection, targetPos);
            
            // Update last shoot time
            lastShootTime = Time.time;
        }
        
        private void CreateProjectile(Vector3 direction, Vector3 targetPos)
        {
            // Create projectile GameObject
            GameObject projectile = new GameObject("Projectile");
            projectile.transform.position = shootPoint.position;
            
            // Add SpriteRenderer
            SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
            sr.sprite = projectileSprite;
            sr.sortingLayerName = "Background";
            sr.sortingOrder = 10;
            
            // Set custom size - stable, no changes
            projectile.transform.localScale = Vector3.one * projectileSize;
            
            // Add Rigidbody2D for smooth physics
            Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * projectileSpeed;
            rb.angularVelocity = rotationSpeed * (isFacingRight ? 1 : -1);
            
            // Add collider for collision detection
            CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.1f;
            
            // Add projectile component for behavior
            ProjectileBehavior behavior = projectile.AddComponent<ProjectileBehavior>();
            behavior.Initialize(shootRange, calculatedTravelTime, gameObject, projectileDamage, isFacingRight, projectileSize);
        }
        
        // Visual debug for shoot range
        private void OnDrawGizmosSelected()
        {
            if (shootPoint != null)
            {
                // Draw shoot range in both directions
                Gizmos.color = Color.red;
                Gizmos.DrawLine(shootPoint.position, shootPoint.position + Vector3.right * shootRange);
                Gizmos.DrawLine(shootPoint.position, shootPoint.position + Vector3.left * shootRange);
                
                // Current facing direction
                Gizmos.color = isFacingRight ? Color.green : Color.blue;
                Vector3 faceDirection = isFacingRight ? Vector3.right : Vector3.left;
                Gizmos.DrawRay(shootPoint.position, faceDirection * shootRange);
                
                // Shoot point
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            }
        }
    }
    
    // Separate component for projectile behavior
    public class ProjectileBehavior : MonoBehaviour
    {
        private Vector3 startPos;
        private float maxRange;
        private float maxTravelTime;
        private uint damage;
        private GameObject shooter;
        private SpriteRenderer spriteRenderer;
        private float lifeTime = 0f;
        private bool hasStartedMoving = false;
        private bool facingRight = true;
        private float stableSize = 0.8f;
        
        public void Initialize(float range, float travelTime, GameObject shooterObject, uint projectileDamage, bool shootingRight, float size)
        {
            startPos = transform.position;
            maxRange = range;
            maxTravelTime = travelTime;
            damage = projectileDamage;
            shooter = shooterObject;
            facingRight = shootingRight;
            stableSize = size;
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // Auto destroy after travel time + buffer
            Destroy(gameObject, travelTime + 3f);
        }
        
        private void Update()
        {
            lifeTime += Time.deltaTime;
            
            // Smooth rotation animation - fix direction based on facing
            float rotationDirection = facingRight ? 1f : -1f;
            transform.Rotate(0, 0, 360f * rotationDirection * Time.deltaTime);
            
            // Keep size stable - no pulse effect
            transform.localScale = Vector3.one * stableSize;
            
            // Check if we've moved away from shooter before enabling collision
            if (!hasStartedMoving)
            {
                float distanceFromShooter = Vector3.Distance(transform.position, shooter.transform.position);
                if (distanceFromShooter > 1.5f) // Increased safe distance
                {
                    hasStartedMoving = true;
                }
            }
            
            // Check if traveled max distance
            float distanceTraveled = Vector3.Distance(startPos, transform.position);
            if (distanceTraveled >= maxRange)
            {
                DestroyProjectile();
                return;
            }
            
            // Check if exceeded travel time
            if (lifeTime >= maxTravelTime)
            {
                DestroyProjectile();
                return;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only process collision if we've moved away from shooter
            if (!hasStartedMoving) return;
            
            // Don't hit the shooter
            if (other.gameObject == shooter || other.transform.IsChildOf(shooter.transform)) return;
            
            // Ignore certain layers that shouldn't stop projectile
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) return;
            
            // Check if it's an enemy and deal damage
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }
            
            // Hit something solid, destroy projectile
            if (!other.isTrigger) // Only destroy on solid objects, not triggers
            {
                DestroyProjectile();
            }
        }
        
        private void DestroyProjectile()
        {
            // Smooth fade out effect
            StartCoroutine(FadeOut());
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            float fadeTime = 0.3f;
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            Vector3 originalScale = transform.localScale;
            
            // Stop movement
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                
                // Smooth fade out
                float alpha = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                // Smooth shrink with bounce effect
                float bounceScale = Mathf.Lerp(1f, 0f, Mathf.Sin(progress * Mathf.PI));
                transform.localScale = originalScale * bounceScale;
                
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
} 