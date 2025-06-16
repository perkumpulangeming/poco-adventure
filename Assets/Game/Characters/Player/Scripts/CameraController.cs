using UnityEngine;

namespace Game.Characters.Player
{
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;

        private void Update()
        {
            var position = playerTransform.position;
            var cameraTransform = transform;

            cameraTransform.position = new Vector3(position.x, position.y, cameraTransform.position.z);
        }
    }
}