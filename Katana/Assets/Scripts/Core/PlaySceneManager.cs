using KatanaMovement;
using UnityEngine;
using NnUtils.Scripts;
using PlayerCamera;

namespace Core
{
    [RequireComponent(typeof(CameraManager))]
    public class PlaySceneManager : MonoBehaviour
    {
        public const string TPCameraHandler = "ThirdPersonCameraHandler";
        public const string FPCameraHandler = "FirstPersonCameraHandler";
        public const string RightCameraHandler = "RightCameraHandler";
        public const string LeftCameraHandler = "LeftCameraHandler";
        public const string StrikeCameraHandler = "StrikeCameraHandler";
        
        private static PlaySceneManager _instance;
        public static PlaySceneManager Instance => _instance = FindFirstObjectByType<PlaySceneManager>();

        [SerializeField] private GameObject _player;
        public static GameObject Player
        {
            get
            {
                if (!Instance) return null;
                return Instance._player ??= GameObject.FindWithTag("Player");
            }
        }

        [SerializeField] private PlayerMovementScript _playerMovement;
        public static PlayerMovementScript PlayerMovement
        {
            get
            {
                if (!Instance) return null;
                return Instance._playerMovement ??= Player?.GetComponent<PlayerMovementScript>();
            }
        }

        [SerializeField] private CameraManager _cameraManager;
        public static CameraManager CameraManager
        {
            get
            {
                if (!Instance) return null;
                return Instance._cameraManager ??= Instance.gameObject.GetOrAddComponent<CameraManager>();
            }
        }

        [SerializeField] private string _playerCameraHandler = FPCameraHandler;
        public static string PlayerCameraHandler
        {
            get => Instance?._playerCameraHandler;
            set => Instance._playerCameraHandler = value;
        }

        private void Reset()
        {
            _cameraManager  = GetComponent<CameraManager>();
            _player         = GameObject.FindWithTag("Player");
            _playerMovement = Player.GetComponent<PlayerMovementScript>();
        }

        private void Awake()
        {
            if (_instance != null) Destroy(gameObject);
            _instance = this;
        }
    }
}