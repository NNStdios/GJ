using System.Collections;
using NnUtils.Scripts;
using UnityEngine;

namespace Assets.Scripts.TimeScale
{
    public class TimeScaleManager : MonoBehaviour
    {
        private float _fixedDeltaTime;

        private float _timeScale = 1;
        public float TimeScale
        {
            get => _timeScale;
            private set
            {
                if (Mathf.Approximately(_timeScale, value)) return;
                if (value < 0) _timeScale = 0;
                _timeScale = value;
                Time.timeScale = TimeScale;
                Time.fixedDeltaTime = _fixedDeltaTime * TimeScale;
            }
        }

        private int? _currentPriority;

        private void Start()
        {
            TimeScale = Time.timeScale;
            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        public void UpdateTimeScale(float timeScale, int priority = 0)
        {
            if (_currentPriority < priority) return;
            this.StopRoutine(ref _updateTimeScaleRoutine);
            TimeScale = timeScale;
            _currentPriority = null;
        }

        public void UpdateTimeScale(TimeScaleKey timeScaleKey, int priority = 0) => UpdateTimeScale(new TimeScaleKeys(timeScaleKey), priority);

        public void UpdateTimeScale(TimeScaleKeys timeScales, int priority = 0)
        {
            if (_currentPriority < priority) return;
            _currentPriority = priority;
            this.RestartRoutine(ref _updateTimeScaleRoutine, UpdateTimeScaleRoutine(timeScales));
        }

        private Coroutine _updateTimeScaleRoutine;
        private IEnumerator UpdateTimeScaleRoutine(TimeScaleKeys timeScales)
        {
            foreach (var timeScale in timeScales.Keys)
            {
                var startTimeScale = TimeScale;
                float lerpPos = 0;

                while (lerpPos < 1)
                {
                    var t = Misc.Tween(ref lerpPos, timeScale.Time, timeScale.Easing, true);
                    TimeScale = Mathf.Lerp(startTimeScale, timeScale.TimeScale, t);
                    yield return null;
                }
            }

            _currentPriority = null;
        }
    }
}