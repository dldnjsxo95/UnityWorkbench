using System;
using System.Collections.Generic;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Central manager for scheduling delayed actions and repeating timers.
    /// </summary>
    public class TimerManager : MonoSingleton<TimerManager>
    {
        private readonly List<ScheduledAction> _scheduledActions = new List<ScheduledAction>();
        private readonly List<ScheduledAction> _toAdd = new List<ScheduledAction>();
        private readonly List<ScheduledAction> _toRemove = new List<ScheduledAction>();

        private int _nextId = 1;

        private void Update()
        {
            // Add pending
            if (_toAdd.Count > 0)
            {
                _scheduledActions.AddRange(_toAdd);
                _toAdd.Clear();
            }

            // Update all scheduled actions
            float time = Time.time;
            float unscaledTime = Time.unscaledTime;

            foreach (var action in _scheduledActions)
            {
                if (action.IsPaused) continue;

                float currentTime = action.UnscaledTime ? unscaledTime : time;

                if (currentTime >= action.NextExecutionTime)
                {
                    action.Callback?.Invoke();
                    action.ExecutionCount++;

                    if (action.RepeatCount > 0 && action.ExecutionCount >= action.RepeatCount)
                    {
                        _toRemove.Add(action);
                    }
                    else if (action.RepeatCount == 0) // One-shot
                    {
                        _toRemove.Add(action);
                    }
                    else // Repeating (-1 = infinite)
                    {
                        action.NextExecutionTime = currentTime + action.Interval;
                    }
                }
            }

            // Remove completed
            if (_toRemove.Count > 0)
            {
                foreach (var action in _toRemove)
                {
                    _scheduledActions.Remove(action);
                }
                _toRemove.Clear();
            }
        }

        /// <summary>
        /// Schedules a one-shot delayed action.
        /// </summary>
        public int Schedule(float delay, Action callback, bool unscaledTime = false)
        {
            return ScheduleInternal(delay, 0f, 0, callback, unscaledTime);
        }

        /// <summary>
        /// Schedules a repeating action.
        /// </summary>
        public int ScheduleRepeating(float interval, Action callback, bool unscaledTime = false, int repeatCount = -1)
        {
            return ScheduleInternal(interval, interval, repeatCount, callback, unscaledTime);
        }

        /// <summary>
        /// Schedules a repeating action with initial delay.
        /// </summary>
        public int ScheduleRepeating(float delay, float interval, Action callback, bool unscaledTime = false, int repeatCount = -1)
        {
            return ScheduleInternal(delay, interval, repeatCount, callback, unscaledTime);
        }

        private int ScheduleInternal(float delay, float interval, int repeatCount, Action callback, bool unscaledTime)
        {
            int id = _nextId++;
            float currentTime = unscaledTime ? Time.unscaledTime : Time.time;

            var action = new ScheduledAction
            {
                Id = id,
                Callback = callback,
                NextExecutionTime = currentTime + delay,
                Interval = interval,
                RepeatCount = repeatCount,
                UnscaledTime = unscaledTime,
                ExecutionCount = 0
            };

            _toAdd.Add(action);
            return id;
        }

        /// <summary>
        /// Cancels a scheduled action.
        /// </summary>
        public bool Cancel(int id)
        {
            for (int i = 0; i < _scheduledActions.Count; i++)
            {
                if (_scheduledActions[i].Id == id)
                {
                    _toRemove.Add(_scheduledActions[i]);
                    return true;
                }
            }

            for (int i = 0; i < _toAdd.Count; i++)
            {
                if (_toAdd[i].Id == id)
                {
                    _toAdd.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Pauses a scheduled action.
        /// </summary>
        public bool Pause(int id)
        {
            var action = GetAction(id);
            if (action != null)
            {
                action.IsPaused = true;
                action.PausedAt = action.UnscaledTime ? Time.unscaledTime : Time.time;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resumes a paused action.
        /// </summary>
        public bool Resume(int id)
        {
            var action = GetAction(id);
            if (action != null && action.IsPaused)
            {
                float currentTime = action.UnscaledTime ? Time.unscaledTime : Time.time;
                float pausedDuration = currentTime - action.PausedAt;
                action.NextExecutionTime += pausedDuration;
                action.IsPaused = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancels all scheduled actions.
        /// </summary>
        public void CancelAll()
        {
            _scheduledActions.Clear();
            _toAdd.Clear();
            _toRemove.Clear();
        }

        private ScheduledAction GetAction(int id)
        {
            foreach (var action in _scheduledActions)
            {
                if (action.Id == id) return action;
            }
            foreach (var action in _toAdd)
            {
                if (action.Id == id) return action;
            }
            return null;
        }

        /// <summary>
        /// Gets the count of active scheduled actions.
        /// </summary>
        public int ActiveCount => _scheduledActions.Count + _toAdd.Count;

        private class ScheduledAction
        {
            public int Id;
            public Action Callback;
            public float NextExecutionTime;
            public float Interval;
            public int RepeatCount; // -1 = infinite, 0 = one-shot, >0 = specific count
            public bool UnscaledTime;
            public int ExecutionCount;
            public bool IsPaused;
            public float PausedAt;
        }

        #region Static Helpers

        /// <summary>
        /// Quick delay using TimerManager.
        /// </summary>
        public static int Delay(float seconds, Action callback, bool unscaledTime = false)
        {
            return Instance.Schedule(seconds, callback, unscaledTime);
        }

        /// <summary>
        /// Quick repeating timer using TimerManager.
        /// </summary>
        public static int Repeat(float interval, Action callback, bool unscaledTime = false)
        {
            return Instance.ScheduleRepeating(interval, callback, unscaledTime);
        }

        /// <summary>
        /// Cancels a timer by ID.
        /// </summary>
        public static bool CancelTimer(int id)
        {
            return Instance.Cancel(id);
        }

        #endregion
    }
}
