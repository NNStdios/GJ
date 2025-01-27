using System.Collections;
using NnUtils.Scripts;
using UnityEngine;

namespace Assets.Scripts.MainMenu
{
    public class MainMenuButton : MonoBehaviour
    {
        private Vector3 _originalPos;
        [SerializeField] private Transform _mesh;
        [SerializeField] private float _animationTime = 1;
        [SerializeField] private Easings.Type _animationEasing = Easings.Type.ExpoOut;

        private void Reset()
        {
            if (transform.childCount > 0)
                _mesh = transform.GetChild(0);
        }

        private void Awake()
        {
            _originalPos = _mesh.localPosition;
        }

        public void Select()
        {
            this.RestartRoutine(ref _moveRoutine, MoveRoutine(Vector3.zero));
        }

        public void Deselect()
        {
            this.RestartRoutine(ref _moveRoutine, MoveRoutine(_originalPos));
        }

        private Coroutine _moveRoutine;
        private IEnumerator MoveRoutine(Vector3 targetPos)
        {
            var startPos = _mesh.localPosition;
            float lerpPos = 0;

            while (lerpPos < 1)
            {
                var t = Misc.Tween(ref lerpPos, _animationTime, _animationEasing, true);
                _mesh.transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, t);
                yield return null;
            }
        }
    }
}