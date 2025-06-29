using UnityEngine;
using Game.Characters.Interfaces;
using Game.Helpers;

namespace Game.Characters.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private uint damage = 1;
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D rb;
        private Vector2 direction;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // Destroy peluru setelah lifetime habis
            Timer.Create(() =>
            {
                if (this != null)
                    Destroy(gameObject);
            }, lifetime);
        }

        public void Initialize(Vector2 shootDirection, uint projectileDamage = 1)
        {
            direction = shootDirection.normalized;
            damage = projectileDamage;

            // Set velocity peluru
            rb.linearVelocity = direction * speed;

            // Rotate peluru sesuai arah tembakan
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Jangan bertabrakan dengan player yang menembak
            if (other.gameObject.layer == Global.Layers.PlayerID)
                return;

            // Cek apakah mengenai enemy
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                DestroyProjectile();
                return;
            }

            // Hancurkan peluru jika mengenai ground/wall
            if (other.gameObject.layer == Global.Layers.GroundID)
            {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile()
        {
            // Bisa tambahkan efek ledakan/partikel di sini
            Destroy(gameObject);
        }
    }
}
