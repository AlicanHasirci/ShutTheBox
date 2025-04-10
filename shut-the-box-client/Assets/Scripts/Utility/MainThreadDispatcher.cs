using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utility
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private readonly Queue<Action> _executionQueue = new(10);
        public static MainThreadDispatcher Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var gameObject = new GameObject("UnityMainThreadDispatcher");
            Instance = gameObject.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public UniTask EnqueueAsync(Action action)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            Enqueue(WrappedAction);
            return tcs.Task;

            void WrappedAction()
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
        }
    }
}
