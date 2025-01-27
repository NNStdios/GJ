using Assets.Scripts.TimeScale;
using NnUtils.Scripts;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [RequireComponent(typeof(TimeScaleManager))]
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

        [ReadOnly][SerializeField] private TimeScaleManager _timeScaleManager;
        public static TimeScaleManager TimeScaleManager => Instance._timeScaleManager;

        private void Reset()
        {
            _timeScaleManager = GetComponent<TimeScaleManager>();
        }

        private void Awake()
        {
            if (_instance == null) Instance = this;
            else if (_instance != this) Destroy(gameObject);
        }

        private void Start()
        {

        }
    }
}