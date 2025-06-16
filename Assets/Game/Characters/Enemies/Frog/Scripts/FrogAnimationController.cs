using System;
using UnityEngine;

namespace Game.Characters.Enemies.Frog
{
    public sealed class FrogAnimationController : EntityAnimationController
    {
        // Params name
        private const string IsGroundedName = "IsGrounded";
        private const string IsFallingName = "IsFalling";

        // Params ID
        private static readonly int IsGroundedId = Animator.StringToHash(IsGroundedName);
        private static readonly int IsFallingID = Animator.StringToHash(IsFallingName);

        private FrogController _frogController;

        private void Start()
        {
            _frogController = GetComponent<FrogController>();

            Entity = GetComponent<FrogController>();
            Entity.OnDeath += OnDeath;
        }

        private void Update()
        {
            Anim.SetBool(IsGroundedId, _frogController.GroundCheck.Grounded);
            Anim.SetBool(IsFallingID, _frogController.Falling);
        }
    }
}