using System.Collections;
using Core;
using NnUtils.Scripts;
using TimeScale;
using UnityEngine;
using PSM = Core.PlaySceneManager;

namespace KatanaMovement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementScript : MonoBehaviour
    {
        private static Camera Cam => PSM.CameraManager.Camera;
        private static string CamHandler => PSM.PlayerCameraHandler;
        
        /// Whether the player is currently using the strike ability
        [ReadOnly] public bool IsPerformingStrike;
        
        // Whether the player is performing the strike impact
        [ReadOnly] public bool IsPerformingStrikeImpact;
        
        [Header("Components")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;

        [Header("Camera")]
        [SerializeField] private float _cameraSwitchDuration = 1;
        [SerializeField] private Easings.Type _cameraSwitchEasing = Easings.Type.ExpoOut;
        
        [Header("Tilt")]
        [SerializeField] private float _tiltSpeed = 1;
       
        [Header("Flip")]
        [SerializeField] private Vector2 _flipForce = new(20, 25);
        [SerializeField] private float _flipRotation = 5;
        [SerializeField] private TimeScaleKeys _flipTimeScale;
        
        [Header("Dash")]
        [SerializeField] private float _dashForce = 100;
        [SerializeField] private TimeScaleKeys _dashTimeScale;
        
        [Header("Dodge")]
        [SerializeField] private float _dodgeForce = 30;
        [SerializeField] private TimeScaleKeys _dodgeTimeScale;

        [Header("Strike Hover")]
        [SerializeField] private float _strikeTransitionTime = 3;
        [SerializeField] private AnimationCurve _strikeTransitionCurve;
        [SerializeField] private float _strikeHoverHeight = 100;
        [SerializeField] private float _strikeHoverDuration = 5;
        [SerializeField] private float _strikeHoverTimeScale = 0.1f;

        [Header("Strike Impact")]
        [SerializeField] private float _camReturnDuration = 0.5f;
        [SerializeField] private Easings.Type _camReturnEasing = Easings.Type.ExpoOut;
        [SerializeField] private LayerMask _strikeLayerMask;
        [SerializeField] private float _strikeImpactForce = 200;
        [SerializeField] private TimeScaleKeys _strikeImpactTimeScale;
        [SerializeField] private TimeScaleKeys _postStrikeImpactTimeScale;

        private void Reset()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (IsPerformingStrike || IsPerformingStrikeImpact) return;
            
            Tilt();
            if (Input.GetKeyDown(KeyCode.Q)) ToggleCamera();
            if (Input.GetKeyDown(KeyCode.Space)) Flip();
            if (Input.GetKeyDown(KeyCode.Mouse0)) Dash();
            if (Input.GetKeyDown(KeyCode.LeftShift)) Dodge();
            if (Input.GetKeyDown(KeyCode.S)) Strike();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsPerformingStrikeImpact)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
                { StrikeImpact(); return; }
            }
            
        }

        private void Tilt()
        {
            // Store mouse input
            var x = Input.GetAxisRaw("Mouse X") * _tiltSpeed;
            var y = Input.GetAxisRaw("Mouse Y") * _tiltSpeed;

            // Return if there is no input
            if (x == 0 && y == 0) return;
            
            // Apply the rotation
            var deltaRotX = Quaternion.AngleAxis(x, Vector3.back);
            var deltaRotY = Quaternion.AngleAxis(y, Vector3.left);
            var deltaRot = deltaRotX * deltaRotY;
            _rb.MoveRotation(_rb.rotation * deltaRot);
        }
        
        private void ToggleCamera()
        {
            PSM.PlayerCameraHandler = CamHandler switch
            {
                PSM.FPCameraHandler => PSM.RightCameraHandler,
                PSM.RightCameraHandler => PSM.TPCameraHandler,
                PSM.TPCameraHandler => PSM.LeftCameraHandler,
                _ => PSM.FPCameraHandler
            };
            
            PSM.CameraManager.SwitchCameraHandler(CamHandler, _cameraSwitchDuration, _cameraSwitchEasing, unscaled: true);
        }

        private void Flip()
        {
            // Store forward
            var forward = transform.up;
            
            // Calculate forces
            var zForce = _flipForce.x * forward;
            var force = new Vector3(0, _flipForce.y) + zForce;
            
            // Add force and rotation
            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(transform.right * _flipRotation, ForceMode.Impulse);
            
            // Apply timescale changes
            GameManager.TimeScaleManager.UpdateTimeScale(_flipTimeScale);
        }

        private void Dash()
        {
            // Reset velocity
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            // Add force
            _rb.AddForce(transform.up * _dashForce, ForceMode.Impulse);
            
            // Apply timescale changes
            GameManager.TimeScaleManager.UpdateTimeScale(_dashTimeScale);
        }

        private void Dodge()
        {
            // Reset velocity
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            // Add force
            _rb.AddForce(-transform.up * _dodgeForce, ForceMode.Impulse);
            
            // Apply timescale changes
            GameManager.TimeScaleManager.UpdateTimeScale(_dodgeTimeScale);
        }

        private void Strike()
        {
            if (_strikeRoutine != null) return;
            this.RestartRoutine(ref _strikeRoutine, StrikeRoutine());
        }

        private Coroutine _strikeRoutine;

        private IEnumerator StrikeRoutine()
        {
            IsPerformingStrike = true;
            
            // Disable gravity
            _rb.useGravity = false;
            
            // Disable collider
            _collider.enabled = false;
            
            // Reset velocity
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            // Store transform
            var startPos = transform.localPosition;
            var targetPos = Vector3.up * _strikeHoverHeight;
            var startRot = transform.localRotation;
            var targetRot = Quaternion.Euler(180, 90, 0);

            // Store the timescale and lerp position
            var startTimeScale = Time.timeScale;
            float lerpPos = 0;
            
            // Start the camera switch and animation
            PSM.CameraManager.SwitchCameraHandler(PSM.StrikeCameraHandler, _strikeTransitionTime, _strikeTransitionCurve, true);
            while (lerpPos < 1)
            {
                var t = _strikeTransitionCurve.Evaluate(Misc.Tween(ref lerpPos, _strikeTransitionTime, unscaled: true));
                transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, t);
                transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, t);
                GameManager.TimeScaleManager.UpdateTimeScale(Mathf.LerpUnclamped(startTimeScale, _strikeHoverTimeScale, t));
                yield return null;
            }

            // Perform hover
            float timer = 0;
            while (timer < _strikeHoverDuration)
            {
                timer += Time.unscaledDeltaTime;
                PerformStrikeHover();
                if (Input.GetKeyDown(KeyCode.Mouse0)) break;
                yield return null;
            }

            // Return camera to the initial handler
            PSM.CameraManager.SwitchCameraHandler(PSM.PlayerCameraHandler, _camReturnDuration, _camReturnEasing, true);
            yield return new WaitForSecondsRealtime(_camReturnDuration);
            
            PerformStrikeImpact();
            _strikeRoutine = null;
        }

        private void PerformStrikeHover()
        {
            // Perform the raycast
            var ray = Cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, _strikeLayerMask))
                return;

            // Rotate player towards the hit point
            transform.LookAt(hit.point, Vector3.up);
            transform.Rotate(90, 0, 0);
        }

        private void PerformStrikeImpact()
        {
            // Update values
            IsPerformingStrike       = false;
            IsPerformingStrikeImpact = true;
            _rb.useGravity           = true;
            _collider.enabled        = true;

            // Add strike to where the blade is pointing
            _rb.AddForce(transform.up * _strikeImpactForce, ForceMode.Impulse);
            GameManager.TimeScaleManager.UpdateTimeScale(_strikeImpactTimeScale);
        }

        private void StrikeImpact()
        {
            IsPerformingStrikeImpact = false;
            GameManager.TimeScaleManager.UpdateTimeScale(_postStrikeImpactTimeScale);
        }
    }
}