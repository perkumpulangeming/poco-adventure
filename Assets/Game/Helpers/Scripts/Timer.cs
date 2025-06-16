using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Game.Helpers
{
    internal sealed class TimerUpdater : MonoBehaviour
    {
        public Action OnUpdate { get; set; }

        private void Update()
        {
            OnUpdate();
        }
    }

    public sealed class Timer
    {
        private Timer(Action action, float secondsToWait, GameObject timerUpdater, string timerName = null)
        {
            Action = action;
            _secondsToWait = secondsToWait;
            _initialSecondsToWait = secondsToWait;
            _timerUpdater = timerUpdater;
            if (timerName is not null)
                _timerID = timerName.GetHashCode();
        }

        private const string DefaultTimerName = "Timer_";
        private const string CurrentSceneGameObjectName = "TimerManager_CurrentScene";

        private static List<Timer> _timerList;
        private static GameObject _currentSceneGameObject;

        private Action Action { get; }
        private readonly GameObject _timerUpdater;

        private readonly int? _timerID;
        private float _secondsToWait;
        private readonly float _initialSecondsToWait;
        private bool Stopped { get; set; }

        public static Timer Create([NotNull] Action action, float secondsToWait,
            bool loop = false, string timerName = null)
        {
            ResetIfNeeded();

            var timerUpdaterName =
                timerName ?? DefaultTimerName + Guid.NewGuid(); // FIXME: Not so efficient concatenation of strings.
            var timerUpdater = new GameObject(timerUpdaterName, typeof(TimerUpdater));

            var timer = new Timer(action, secondsToWait, timerUpdater, timerName);

            if (loop)
            {
                timerUpdater.GetComponent<TimerUpdater>().OnUpdate = timer.UpdateInfinite;
            }
            else
            {
                timerUpdater.GetComponent<TimerUpdater>().OnUpdate = timer.UpdateFixed;
            }

            _timerList.Add(timer);

            return timer;
        }

        public static void RemoveTimer(Timer timer)
        {
            _timerList.Remove(timer);
        }

        public static void RemoveTimer(string timerName)
        {
            var isFound = _timerList.Find(timer => timer._timerID == timerName.GetHashCode());
            if (isFound is null) return;

            UnityEngine.Object.Destroy(isFound._timerUpdater);
            _timerList.Remove(isFound);
        }

        public static void RemoveTimers()
        {
            if (_currentSceneGameObject is null)
            {
                Reset();
            }
            else
            {
                _timerList.ForEach(timer => UnityEngine.Object.Destroy(timer._timerUpdater));
                _timerList = new List<Timer>();
            }
        }

        public static void EnableTimer(Timer timer)
        {
            _timerList.Where(timerPredicate => timerPredicate._timerID == timer._timerID).ToList()
                .ForEach(timerToDisable => timerToDisable._timerUpdater.SetActive(true));
        }

        public static void EnableTimer(string timerName)
        {
            _timerList.Where(timer => timer._timerID == timerName.GetHashCode()).ToList()
                .ForEach(timer => timer._timerUpdater.SetActive(true));
        }

        public static void DisableTimer(Timer timer)
        {
            _timerList.Where(timerPredicate => timerPredicate._timerID == timer._timerID).ToList()
                .ForEach(timerToDisable => timerToDisable._timerUpdater.SetActive(false));
        }

        public void DisableTimer()
        {
            _timerUpdater.SetActive(false);
        }

        public void EnableTimer()
        {
            _timerUpdater.SetActive(true);
        }

        public void ResetTime()
        {
            _secondsToWait = _initialSecondsToWait;
        }

        public bool IsValid()
        {
            return _timerUpdater.gameObject != null;
        }

        public static void DisableTimer(string timerName)
        {
            _timerList.Where(timer => timer._timerID == timerName.GetHashCode()).ToList()
                .ForEach(timer => timer._timerUpdater.SetActive(false));
        }

        private static void Reset()
        {
            _currentSceneGameObject = new GameObject(CurrentSceneGameObjectName);

            _timerList = new List<Timer>();
        }

        private static void ResetIfNeeded()
        {
            if (_currentSceneGameObject is not null) return;

            Reset();
        }

        private void UpdateFixed()
        {
            if (Stopped) return;

            _secondsToWait -= Time.deltaTime;
            if (!(_secondsToWait <= 0.0f)) return;

            Action();
            Stopped = true;
            UnityEngine.Object.Destroy(_timerUpdater);
        }

        private void UpdateInfinite()
        {
            ResetIfNeeded();

            _secondsToWait -= Time.deltaTime;
            if (!(_secondsToWait <= 0.0f)) return;

            try
            {
                Action();
            }
            catch (Exception)
            {
                UnityEngine.Object.Destroy(_timerUpdater);

                return;
            }

            _secondsToWait = _initialSecondsToWait;
        }
    }
}