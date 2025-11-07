using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCON
{
    /// <summary>
    /// Dispatcher to execute actions on Unity's main thread
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher instance;
        private static readonly Queue<Action> executionQueue = new Queue<Action>();

        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    CreateInstance();
                }
                return instance;
            }
        }

        private static void CreateInstance()
        {
            if (instance != null) return;

            var go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;

            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }

            // Ensure instance exists
            if (instance == null)
            {
                CreateInstance();
            }
        }

        private void Update()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    var action = executionQueue.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"Error executing action on main thread: {ex.Message}");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}
