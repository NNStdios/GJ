using UnityEngine;
using NnUtils.Scripts;
using Assets.Scripts.PlayerCamera;
using Assets.Scripts.KatanaMovement;

namespace Assets.Scripts.Core
{
    [RequireComponent(typeof(CameraManager))]
    public class PlaySceneManager : MonoBehaviour
    {
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

        public bool IsDead;

        [SerializeField] private GameObject _deathScreen;

        private void Reset()
        {
            _cameraManager = GetComponent<CameraManager>();
            _player = GameObject.FindWithTag("Player");
            _playerMovement = Player.GetComponent<PlayerMovementScript>();
        }

        private void Awake()
        {
            if (_instance != null) Destroy(gameObject);
            _instance = this;
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Die()
        {
            _deathScreen.SetActive(true);
            IsDead = true;
        }
    }
}