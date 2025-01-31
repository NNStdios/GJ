using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.PlayerCamera
{
    public class CameraHandlerScript : MonoBehaviour
    {
        private static Transform Player => PlaySceneManager.Player.transform;
        
        [Header("")]
        public Perspective Perspective;
        [SerializeField] private LayerMask _cameraCollisionMask;

        [Header("Position")]
        [Tooltip("Offset from the player")]
        [SerializeField] private Vector3 _offset;

        [Tooltip("Whether offset is applied locally or globally")]
        [SerializeField] private bool _localOffset;

        [Tooltip("If enabled, offset will be applied according to the rotation")]
        [SerializeField] private bool _translateOffset;

        [Header("Rotation")]
        [Tooltip("Whether the camera will follow player's rotation")]
        [SerializeField] private bool _rotateCamera;

        [Tooltip("Delta between the player rotation and camera rotation")]
        [SerializeField] private Vector3 _rotationOffset;

        private void Start()
        {
            PlaySceneManager.CameraManager.Handlers.Add(this);
        }

        private void Update()
        {
            // Move camera to the player's position to allow for proper offset after rotation
            transform.position = Player.position;
            if (_rotateCamera) Rotate();
            Offset();
        }

        private void OnDestroy()
        {
            if (PlaySceneManager.CameraManager == null) return;
            PlaySceneManager.CameraManager.Handlers.Remove(this);
        }

        private void Offset()
        {
            // Store parent
            var parent = transform.parent ? transform.parent : transform;

            // Calculate offset
            var offset = _localOffset ? parent.TransformPoint(_offset) - Player.position : _offset;

            var desiredPos = transform.position + (_translateOffset ? transform.TransformVector(offset) : offset);
            var dir = desiredPos - Player.position;
            var distance = dir.magnitude;

            // Set the camera handler position
            if (Physics.Raycast(Player.position, dir.normalized, out var hit, distance, _cameraCollisionMask))
            {
                var targetPos = hit.point - dir.normalized * 0.1f;
                if (_localOffset) transform.localPosition = parent.InverseTransformPoint(targetPos);
                else transform.position = targetPos;
            }
            else
            {
                if (_localOffset) transform.localPosition = parent.InverseTransformPoint(desiredPos);
                else transform.position = desiredPos;
            }
        }

        private void Rotate()
        {
            transform.localRotation = Player.localRotation * Quaternion.Euler(_rotationOffset);
        }
    }
}