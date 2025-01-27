using System.Collections;
using Assets.Scripts.Core;
using NnUtils.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.PlayerCamera
{
    [ExecuteAlways]
    [RequireComponent(typeof(Volume))]
    public class DynamicDOF : MonoBehaviour
    {
        private static Camera Camera => PlaySceneManager.CameraManager.Camera;

        private float _distance, _previousDistance;

        [SerializeField] private Transform _dofPoint;
        [SerializeField] private DepthOfField _dof;
        [SerializeField] private float _gaussianSize = 20;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _transitionTime = 1;
        [SerializeField] private Easings.Type _transitionEasing = Easings.Type.ExpoOut;

        private void Reset()
        {
            // Get volume and add dof if it doesn't exist
            var volume = GetComponent<Volume>();
            if (!volume.sharedProfile.TryGet(out _dof)) _dof = volume.profile.Add<DepthOfField>();
        }

        private void Update()
        {
            // Return if dof doesn't exist
            if (_dof == null) return;

            // Set the default distance to 10000(basically no dof)
            float distance = 1000;

            // Create a ray from the camera center, check for hits and update the distance accordingly
            Ray ray = new(_dofPoint.position, _dofPoint.forward);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _layerMask)) distance = hit.distance;

            // Check if object distance is approx the same and return
            if (Mathf.Approximately(distance, _distance)) return;

            // Update distance values and restart the transition routine
            _previousDistance = _distance;
            _distance = distance;
            this.RestartRoutine(ref _updateDOFRoutine, UpdateDOFRoutine());
        }

        private Coroutine _updateDOFRoutine;
        private IEnumerator UpdateDOFRoutine()
        {
            float lerpPos = 0;
            while (lerpPos < 1)
            {
                // Update T
                var t = Misc.Tween(ref lerpPos, _transitionTime, _transitionEasing, true);
                var distance = Mathf.Lerp(_previousDistance, _distance, t);

                // Update the dof component
                _dof.focusDistance.value = distance;
                _dof.gaussianStart.value = distance - _gaussianSize;
                _dof.gaussianEnd.value = distance + _gaussianSize;

                // Wait for the next frame
                yield return null;
            }

            _updateDOFRoutine = null;
        }
    }
}