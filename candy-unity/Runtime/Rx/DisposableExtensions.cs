#if R3
using System;
using R3;
using UnityEngine;

namespace Candy.Unity
{
    /// <summary>
    /// Extensions for R3 disposables with Unity lifecycle integration.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Adds this disposable to be automatically disposed when the target component is disabled.
        /// Useful for subscriptions that should stop when a GameObject is disabled and restart when re-enabled.
        /// </summary>
        /// <typeparam name="T">The disposable type</typeparam>
        /// <param name="disposable">The disposable to track</param>
        /// <param name="component">The component whose OnDisable lifecycle will trigger disposal</param>
        /// <returns>The original disposable for chaining</returns>
        public static T AddToOnDisable<T>(this T disposable, Component component) where T : IDisposable
        {
            if (component == null || disposable == null)
                return disposable;

            var tracker = component.GetComponent<DisableTracker>();
            if (tracker == null)
                tracker = component.gameObject.AddComponent<DisableTracker>();

            tracker.AddDisposable(disposable);
            return disposable;
        }

        /// <summary>
        /// Internal component that tracks OnDisable lifecycle events.
        /// Automatically added to GameObjects when using AddToOnDisable.
        /// </summary>
        private class DisableTracker : MonoBehaviour
        {
            private CompositeDisposable _disposables = new();

            public void AddDisposable(IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            private void OnDisable()
            {
                _disposables?.Dispose();
                _disposables = new CompositeDisposable();
            }

            private void OnDestroy()
            {
                _disposables?.Dispose();
            }
        }
    }
}
#endif