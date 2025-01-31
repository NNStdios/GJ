using System.Collections;
using Assets.Scripts.Core;
using Assets.Scripts.Fruits;
using Assets.Scripts.PlayerCamera;
using Assets.Scripts.TimeScale;
using NnUtils.Scripts;
using UnityEngine;
using PSM = Assets.Scripts.Core.PlaySceneManager;

namespace Assets.Scripts.KatanaMovement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementScript : MonoBehaviour
    {
        private static Camera Cam => PSM.CameraManager.Camera;
        private static SettingsManager Settings => GameManager.SettingsManager;

        /// Whether the player is currently stuck
        [ReadOnly] public bool IsStuck;

        /// Whether the player is currently using the strike ability
        [ReadOnly] public bool IsPerformingStrike;

        /// Whether the player is performing the strike impact
        [ReadOnly] public bool IsPerformingStrikeImpact;

        [Header("Components")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _decalPrefab;
        [SerializeField] private Transform _decalPoint;

        [Header("Camera")]
        [SerializeField] private float _cameraSwitchDuration = 1;
        [SerializeField] private Easings.Type _cameraSwitchEasing = Easings.Type.ExpoOut;

        [Header("Tilt")]
        [SerializeField] private float _tiltSpeed = 1;

        [Header("Stick")]
        [SerializeField] private LayerMask _stickMask;
        [SerializeField] private float _minStickVelocity = 5;
        [SerializeField] private float _maxStickAngle = 45;

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

        private void Start()
        {
            _renderer.sharedMaterial = GameManager.ItemManager.SelectedItem.Material;
            PSM.CameraManager.SwitchCameraHandler(Settings.Perspective, _cameraSwitchDuration, _cameraSwitchEasing, unscaled: true);
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

        private void OnCollisionEnter(Collision col)
        {
            // Store the col layer
            var layer = col.gameObject.layer;

            // If performing strike, check if something's hit
            if (IsPerformingStrikeImpact)
            {
                // If there is no hit, do an early return
                if ((_strikeLayerMask & 1 << layer) == 0) return;

                // Handle the strike impact
                StrikeImpact(col);

                // Return
                return;
            }

            // Try to stick
            if ((_stickMask & 1 << layer) != 0) Stick(col);
        }

        private void Stick(Collision col)
        {
            // Return if already stuck
            if (IsStuck) return;

            // Return if striking
            if (IsPerformingStrike || IsPerformingStrikeImpact) return;

            // Return if not moving forward fast enough
            if (col.relativeVelocity.magnitude < _minStickVelocity) return;
            //if (transform.InverseTransformDirection(_rb.linearVelocity).y < _minStickVelocity) return;

            // Get forward and threshold
            var fw = transform.up;
            var threshold = -Mathf.Cos(_maxStickAngle * Mathf.Deg2Rad);

            // Loop through all normals and see if they are at a low enough angle relative to player
            foreach (var contact in col.contacts)
            {
                // Get the normal and dot product
                var contactNormal = contact.normal;
                var dp = Vector3.Dot(fw, contactNormal);

                // Return if angle is not low enough
                if (dp > threshold) continue;

                // Get stuck and return
                GetStuck(col.transform);
                SpawnDecal();
                return;
            }
        }

        private void GetStuck(Transform parent)
        {
            // Freeze rb
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            // Set parent to follow that object
            transform.SetParent(parent);

            // Set IsStuck to true
            IsStuck = true;
        }

        private void SpawnDecal()
        {
            var decal = Instantiate(_decalPrefab, _decalPoint);
            decal.SetParent(null);
        }

        private void GetUnstuck()
        {
            // Return if not stuck
            if (!IsStuck) return;

            // Unfreeze the rb
            _rb.isKinematic = false;

            // Reset parent
            transform.SetParent(null);

            // Set IsStuck to false
            IsStuck = false;
        }

        private void Tilt()
        {
            // Return if stuck
            if (IsStuck) return;

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
            Settings.Perspective = Settings.Perspective switch
            {
                Perspective.First => Perspective.Right,
                Perspective.Right => Perspective.Third,
                Perspective.Third => Perspective.Left,
                _ => Perspective.First
            };

            PSM.CameraManager.SwitchCameraHandler(Settings.Perspective, _cameraSwitchDuration, _cameraSwitchEasing, unscaled: true);
        }

        private void Flip()
        {
            GetUnstuck();

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
            GetUnstuck();

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
            GetUnstuck();

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
            if (IsPerformingStrike || IsPerformingStrikeImpact) return;
            this.RestartRoutine(ref _strikeRoutine, StrikeRoutine());
        }

        private Coroutine _strikeRoutine;

        private IEnumerator StrikeRoutine()
        {
            IsPerformingStrike = true;

            // Unstick
            GetUnstuck();

            // Freeze rb
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;

            // Disable collider
            _collider.enabled = false;

            // Store transform
            var startPos = transform.localPosition;
            var targetPos = Vector3.up * _strikeHoverHeight;
            var startRot = transform.localRotation;
            var targetRot = Quaternion.Euler(180, 90, 0);

            // Store the timescale and lerp position
            var startTimeScale = Time.timeScale;
            float lerpPos = 0;

            // Start the camera switch and animation
            PSM.CameraManager.SwitchCameraHandler(Perspective.Strike, _strikeTransitionTime, _strikeTransitionCurve, true);
            while (lerpPos < 1)
            {
                var t = _strikeTransitionCurve.Evaluate(Misc.Tween(ref lerpPos, _strikeTransitionTime, unscaled: true));
                transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, t);
                transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, t);
                GameManager.TimeScaleManager.UpdateTimeScale(Mathf.LerpUnclamped(startTimeScale, _strikeHoverTimeScale, t));
                yield return null;
            }

            // TODO: Replace with a custom cursor
            // Show and unlock the cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Perform hover
            float timer = 0;
            while (timer < _strikeHoverDuration)
            {
                timer += Time.unscaledDeltaTime;
                PerformStrikeHover();
                if (Input.GetKeyDown(KeyCode.Mouse0)) break;
                yield return null;
            }

            // Hide and lock the cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Return camera to the initial handler
            PSM.CameraManager.SwitchCameraHandler(Settings.Perspective, _camReturnDuration, _camReturnEasing, true);
            yield return new WaitForSecondsRealtime(_camReturnDuration);

            // Perform the impact
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
            IsPerformingStrike = false;
            IsPerformingStrikeImpact = true;
            _rb.isKinematic = false;
            _collider.enabled = true;

            // Add strike to where the blade is pointing
            _rb.AddForce(transform.up * _strikeImpactForce, ForceMode.Impulse);
            GameManager.TimeScaleManager.UpdateTimeScale(_strikeImpactTimeScale);
        }

        private void StrikeImpact(Collision col)
        {
            IsPerformingStrikeImpact = false;
            Stick(col);
            GameManager.TimeScaleManager.UpdateTimeScale(_postStrikeImpactTimeScale);
        }
    }
}