using System.Collections;
using Core;
using KatanaMovement;
using NnUtils.Scripts;
using UnityEngine;

namespace PlayerCamera
{
    public class FPCameraScript : MonoBehaviour
    {
        private static PlayerMovementScript PlayerMovement => PlaySceneManager.PlayerMovement;
        
        // Current rotation has to be stored and can't be taken from the origin rot because quaternions are between -180 and 180
        private Vector2 _currentRot;
        private Vector2 _totalRot;
        
        [Header("Panning")]
        [SerializeField] private float _sensitivity = 2f;
        [SerializeField] private float _panTime = 1;
        [SerializeField] private Easings.Type _panEasing = Easings.Type.ExpoOut;
        [SerializeField] private Vector2 _xPanLimit = new(-90, 90);
        [SerializeField] private Vector2 _yPanLimit = new(-90, 90);
        
        private void Update()
        {
            if (PlayerMovement.IsPerformingStrike || PlayerMovement.IsPerformingStrikeImpact
                || PlaySceneManager.PlayerCameraHandler != PlaySceneManager.FPCameraHandler)
            {
                StopPanning();
                return;
            }
            
            Pan();
        }

        private void Pan()
        {
            // Store mouse movement and return if there is none
            Vector2 positionDelta = new(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

            // Update total rotation
            _totalRot += positionDelta * _sensitivity;
            _totalRot.x = Mathf.Clamp(_totalRot.x, _xPanLimit.x, _xPanLimit.y);
            _totalRot.y = Mathf.Clamp(_totalRot.y, _yPanLimit.x, _yPanLimit.y);
            
            // Start panning
            this.RestartRoutine(ref _panRoutine, PanRoutine());
        }
        
        private Coroutine _panRoutine;

        private IEnumerator PanRoutine()
        {
            var startRot = _currentRot;
            float lerpPos = 0;

            while (lerpPos < 1)
            {
                var t = Misc.Tween(ref lerpPos, _panTime, _panEasing);
                _currentRot                = Vector2.LerpUnclamped(startRot, _totalRot, t);
                transform.localEulerAngles = _currentRot;
                yield return null;
            }

            _panRoutine = null;
        }

        private void StopPanning()
        {
            this.StopRoutine(ref _panRoutine);
            _totalRot = _currentRot = Vector3.zero;
        }
    }
}