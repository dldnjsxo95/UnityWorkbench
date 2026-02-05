using System;
using System.Collections;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Utility for running coroutines from non-MonoBehaviour scripts.
    /// </summary>
    public class CoroutineRunner : MonoSingleton<CoroutineRunner>
    {
        /// <summary>
        /// Runs a coroutine.
        /// </summary>
        public static Coroutine Run(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stops a coroutine.
        /// </summary>
        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                Instance.StopCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Stops a coroutine by IEnumerator.
        /// </summary>
        public static void Stop(IEnumerator coroutine)
        {
            Instance.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Stops all coroutines.
        /// </summary>
        public static void StopAll()
        {
            Instance.StopAllCoroutines();
        }

        /// <summary>
        /// Waits for seconds then executes action.
        /// </summary>
        public static Coroutine WaitThen(float seconds, Action action)
        {
            return Run(WaitThenCoroutine(seconds, action, false));
        }

        /// <summary>
        /// Waits for unscaled seconds then executes action.
        /// </summary>
        public static Coroutine WaitUnscaledThen(float seconds, Action action)
        {
            return Run(WaitThenCoroutine(seconds, action, true));
        }

        /// <summary>
        /// Waits for next frame then executes action.
        /// </summary>
        public static Coroutine NextFrame(Action action)
        {
            return Run(NextFrameCoroutine(action));
        }

        /// <summary>
        /// Waits for end of frame then executes action.
        /// </summary>
        public static Coroutine EndOfFrame(Action action)
        {
            return Run(EndOfFrameCoroutine(action));
        }

        /// <summary>
        /// Waits for fixed update then executes action.
        /// </summary>
        public static Coroutine NextFixedUpdate(Action action)
        {
            return Run(NextFixedUpdateCoroutine(action));
        }

        /// <summary>
        /// Waits until a condition is true then executes action.
        /// </summary>
        public static Coroutine WaitUntil(Func<bool> condition, Action action)
        {
            return Run(WaitUntilCoroutine(condition, action));
        }

        /// <summary>
        /// Waits while a condition is true then executes action.
        /// </summary>
        public static Coroutine WaitWhile(Func<bool> condition, Action action)
        {
            return Run(WaitWhileCoroutine(condition, action));
        }

        /// <summary>
        /// Executes an action over time.
        /// </summary>
        public static Coroutine Tween(float duration, Action<float> onUpdate, Action onComplete = null, bool unscaledTime = false)
        {
            return Run(TweenCoroutine(duration, onUpdate, onComplete, unscaledTime));
        }

        /// <summary>
        /// Executes an action repeatedly at an interval.
        /// </summary>
        public static Coroutine Repeat(float interval, Action action, int count = -1)
        {
            return Run(RepeatCoroutine(interval, action, count));
        }

        #region Coroutine Implementations

        private static IEnumerator WaitThenCoroutine(float seconds, Action action, bool unscaled)
        {
            if (unscaled)
                yield return new WaitForSecondsRealtime(seconds);
            else
                yield return new WaitForSeconds(seconds);

            action?.Invoke();
        }

        private static IEnumerator NextFrameCoroutine(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private static IEnumerator EndOfFrameCoroutine(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        private static IEnumerator NextFixedUpdateCoroutine(Action action)
        {
            yield return new WaitForFixedUpdate();
            action?.Invoke();
        }

        private static IEnumerator WaitUntilCoroutine(Func<bool> condition, Action action)
        {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }

        private static IEnumerator WaitWhileCoroutine(Func<bool> condition, Action action)
        {
            yield return new WaitWhile(condition);
            action?.Invoke();
        }

        private static IEnumerator TweenCoroutine(float duration, Action<float> onUpdate, Action onComplete, bool unscaledTime)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                onUpdate?.Invoke(t);

                yield return null;
                elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            onUpdate?.Invoke(1f);
            onComplete?.Invoke();
        }

        private static IEnumerator RepeatCoroutine(float interval, Action action, int count)
        {
            int executed = 0;
            var wait = new WaitForSeconds(interval);

            while (count < 0 || executed < count)
            {
                yield return wait;
                action?.Invoke();
                executed++;
            }
        }

        #endregion
    }
}
