    using UnityEngine;

    public class SlimerDeath : MonoBehaviour
    {
        private Animator animator;
        private bool isDead = false;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            // Tekan tombol K pada keyboard untuk trigger animasi mati
            if (Input.GetKeyDown(KeyCode.K))
            {
                Kill();
            }
        }

        public void Kill()
        {
            if (!isDead)
            {
                isDead = true;
                animator.SetTrigger("Die");
            }
        }
    }
