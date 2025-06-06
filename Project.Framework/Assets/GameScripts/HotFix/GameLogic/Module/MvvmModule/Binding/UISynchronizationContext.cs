using System.Threading;
using UnityEngine;
using UnityFramework;

namespace GameLogic.Binding
{
    public class UISynchronizationContext
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitialize()
        {
            context = SynchronizationContext.Current;
            threadId = Thread.CurrentThread.ManagedThreadId;
        }

        private static int threadId;
        private static SynchronizationContext context;

        public static bool InThread { get { return threadId == Thread.CurrentThread.ManagedThreadId; } }

        public static void Post(SendOrPostCallback callback, object state)
        {
            Log.Info($"1.6.1,context={context}, threadId={threadId},Thread.CurrentThread.ManagedThreadId = {Thread.CurrentThread.ManagedThreadId}, callback={callback}, state={state}");
            if (threadId == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
                context.Post(callback, state);
            
            Log.Info("1.6.2");
        }
        public static void Send(SendOrPostCallback callback, object state)
        {
            if (threadId == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
                context.Send(callback, state);
        }
    }
}