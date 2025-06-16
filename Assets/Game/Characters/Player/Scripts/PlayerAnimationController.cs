using UnityEngine;

namespace Game.Characters.Player
{
    public sealed class PlayerAnimationController : EntityAnimationController
    {
        // Animations ID
        private static readonly int HurtAnimationID = Animator.StringToHash("Hurt");

        // Params ID
        private static readonly int IsRunningID = Animator.StringToHash("IsRunning");
        private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");
        private static readonly int IsFallingID = Animator.StringToHash("IsFalling");
        private static readonly int IsCrouchingID = Animator.StringToHash("IsCrouching");
        private static readonly int IsClimbingID = Animator.StringToHash("IsClimbing");

        private PlayerController _playerController;

        private void Start()
        {
            _playerController = (PlayerController)Entity;

            _playerController.OnTakingDamage += OnTakingDamage;
        }

        private void Update()
        {
            Anim.SetBool(IsRunningID, Entity.Running);

            Anim.SetBool(IsGroundedId, _playerController.GroundCheck.Grounded);
            Anim.SetBool(IsFallingID, _playerController.Falling);
            Anim.SetBool(IsCrouchingID, _playerController.IsCrouching);
            Anim.SetBool(IsClimbingID, _playerController.IsOnLadder);
        }

        private void OnTakingDamage()
        {
            Anim.Play(HurtAnimationID);
        }
    }
}