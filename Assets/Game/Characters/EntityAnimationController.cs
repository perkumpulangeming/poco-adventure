using UnityEngine;

namespace Game.Characters
{
    public abstract class EntityAnimationController : MonoBehaviour
    {
        // Triggers
        private static readonly int DeathTrigger = Animator.StringToHash("Death");

        // Components
        protected Animator Anim { get; private set; }
        protected EntityController Entity { get; set; }

        private void Awake()
        {
            Anim = GetComponent<Animator>();
            Entity = GetComponent<EntityController>();

            Entity.OnDeath += OnDeath;
        }

        protected float OnDeath()
        {
            Anim.SetTrigger(DeathTrigger);

            return Anim.GetCurrentAnimatorClipInfo(0).Length;
        }
    }
}