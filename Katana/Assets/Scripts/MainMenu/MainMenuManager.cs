using System.Collections;
using NnUtils.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        private static MainMenuManager _instance;
        public static MainMenuManager Instance => _instance = FindFirstObjectByType<MainMenuManager>();

        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private float _menuTransitionTime;
        [SerializeField] private AnimationCurve _menuTransitionCurve;
        [SerializeField] private Vector3 _mainMenuRotation;
        [SerializeField] private Vector3 _settingsRotation = new(0, 180, 0);
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) MainMenu();
            if (Input.GetKeyDown(KeyCode.Alpha1)) MainMenu();
            if (Input.GetKeyDown(KeyCode.Alpha2)) Settings();
        }

        public void Play() => SceneManager.LoadScene(1);

        public void MainMenu()
        {
            this.RestartRoutine(ref _rotateCameraRoutine, RotateCameraRoutine(_mainMenuRotation));
        }
        
        public void Settings()
        {
            this.RestartRoutine(ref _rotateCameraRoutine, RotateCameraRoutine(_settingsRotation));
        }
        
        public void Quit() => Application.Quit();

        private Coroutine _rotateCameraRoutine;

        private IEnumerator RotateCameraRoutine(Vector3 targetRot)
        {
            var startRot = _cameraHolder.localEulerAngles;

            float lerpPos = 0;
            while (lerpPos < 1)
            {
                var t = _menuTransitionCurve.Evaluate(Misc.Tween(ref lerpPos, _menuTransitionTime, unscaled: true));
                _cameraHolder.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(startRot, targetRot, t));
                yield return null;
            }
        }
    }
}