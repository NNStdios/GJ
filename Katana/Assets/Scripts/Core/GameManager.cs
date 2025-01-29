using Assets.Scripts.Items;
using Assets.Scripts.TimeScale;
using NnUtils.Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    [RequireComponent(typeof(TimeScaleManager))]
    [RequireComponent(typeof(ItemManager))]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get => _instance;
            private set
            {
                if (_instance != null && _instance != value)
                {
                    Destroy(value.gameObject);
                    return;
                }

                _instance = value;
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        [ReadOnly] [SerializeField] private TimeScaleManager _timeScaleManager;
        public static TimeScaleManager TimeScaleManager => Instance._timeScaleManager;

<<<<<<< HEAD
        [HideInInspector]public Transform Player;

=======
        [ReadOnly] [SerializeField] private ItemManager _itemManager;
        public static ItemManager ItemManager => Instance._itemManager;
        
>>>>>>> 8d598c5b5405a10b39194f726c3cae599fb9bba6
        private void Reset()
        {
            _timeScaleManager = this.GetOrAddComponent<TimeScaleManager>();
            _itemManager = this.GetOrAddComponent<ItemManager>();
        }

        private void Awake()
        {
            if (_instance == null) Instance = this;
            else if (_instance != this) Destroy(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
<<<<<<< HEAD
            Player = GameObject.FindGameObjectWithTag("Player").transform;
=======
            TimeScaleManager.UpdateTimeScale(1, -100);
>>>>>>> 8d598c5b5405a10b39194f726c3cae599fb9bba6
        }
    }
}