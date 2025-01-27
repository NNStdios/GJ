using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NnUtils.Scripts;
using PlayerCamera;
using UnityEngine;

namespace Assets.Scripts.PlayerCamera
{
    public class CameraManager : MonoBehaviour
    {
        // Handlers should take care of adding/removing
        public readonly HashSet<CameraHandlerScript> Handlers = new();

        public Transform CameraHolder;
        public Camera Camera;
        public Camera PlayerCamera;

        public void SwitchCameraHandler(string handlerName, float duration = 0, Easings.Type easing = Easings.Type.None, bool unscaled = false)
            => SwitchCameraHandler(handlerName, duration, easing, null, unscaled);

        public void SwitchCameraHandler(string handlerName, float duration = 0, AnimationCurve curve = null, bool unscaled = false)
            => SwitchCameraHandler(handlerName, duration, Easings.Type.None, curve, unscaled);

        private void SwitchCameraHandler(string handlerName, float duration, Easings.Type easing, AnimationCurve curve, bool unscaled)
        {
            // Get the handler and check if it exists
            var handler = Handlers.First(x => x.name == handlerName);
            if (handler == null)
            {
                Debug.LogError($"Camera not found: {handlerName}, returning");
                return;
            }

            // Change the cam parent and switch
            CameraHolder.parent = handler.transform;
            this.RestartRoutine(ref _switchCameraRoutine, SwitchCameraRoutine(duration, easing, curve, unscaled));
        }

        private Coroutine _switchCameraRoutine;
        private IEnumerator SwitchCameraRoutine(float duration, Easings.Type easing, AnimationCurve curve, bool unscaled)
        {
            // Store transform
            var startPos = CameraHolder.localPosition;
            var startRot = CameraHolder.transform.localRotation;

            float lerpPos = 0;
            while (lerpPos < 1)
            {
                // Get T
                var t = Misc.Tween(ref lerpPos, duration, easing, unscaled);
                if (curve != null) t = curve.Evaluate(t);

                // Update transform
                CameraHolder.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
                CameraHolder.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);

                // Wait for the next frame
                yield return null;
            }

            _switchCameraRoutine = null;
        }
    }
}