using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NnUtils.Scripts;
using UnityEngine;

namespace PlayerCamera
{
    public class CameraManager : MonoBehaviour
    {
        // Handlers should take care of adding/removing
        public readonly HashSet<CameraHandlerScript> Handlers = new();
        public Camera Camera { get; private set; }
        public FPCameraScript FPCamera { get; private set; }

        private void Awake()
        {
            Camera       = Camera.main;
            FPCamera = Camera?.GetComponent<FPCameraScript>();
        }
        
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
            Camera.transform.parent = handler.transform;
            this.RestartRoutine(ref _switchCameraRoutine, SwitchCameraRoutine(duration, easing, curve, unscaled));
        }

        private Coroutine _switchCameraRoutine;
        private IEnumerator SwitchCameraRoutine(float duration, Easings.Type easing, AnimationCurve curve, bool unscaled)
        {
            // Store transform
            var startPos = Camera.transform.localPosition;
            var startRot = Camera.transform.localRotation;

            float lerpPos = 0;
            while (lerpPos < 1)
            {
                // Get T
                var t = Misc.Tween(ref lerpPos, duration, easing, unscaled);
                if (curve != null) t = curve.Evaluate(t);
                
                // Update transform
                Camera.transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
                Camera.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
                
                // Wait for the next frame
                yield return null;
            }

            _switchCameraRoutine = null;
        }
    }
}