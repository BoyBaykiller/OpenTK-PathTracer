using System;
using System.Threading;
using System.Collections.Generic;


namespace OpenTK_PathTracer
{
    static class ThreadManager
    {
        public enum Priority
        {
            /// <summary>
            /// This thread halts until the invocation of the action on the main thread
            /// </summary>
            Now = 0,

            /// <summary>
            /// Queues the action on the main thread but does not wait for its invocation
            /// </summary>
            ASAP = 1,
        }

        private static List<Action> invocationQueue = new List<Action>();
        private static List<Action> invocationQueueCopied = new List<Action>();
        private static AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
        private static bool actionsToBeInvoked = false;

        /// <summary>
        /// Queues an action for invocation on main Thread
        /// </summary>
        /// <param name="action">Action to be invoked</param>
        /// <param name="priority">Priority of execution</param>
        public static void ExecuteOnMainThread(Action action, Priority priority)
        {
            //if (Thread.CurrentThread.ManagedThreadId == 1)
            //    throw new Exception("Calling this function on Thread 1 is undesired");

            if (action == null)
                return;

            if (priority == Priority.Now)
            {
                Action modifiedAction = new Action(() =>
                {
                    action.Invoke();
                    stopWaitHandle.Set();
                });

                lock (invocationQueue)
                {
                    invocationQueue.Add(modifiedAction);
                    actionsToBeInvoked = true;
                }

                stopWaitHandle.WaitOne();
            }
            else
            {
                if (priority == Priority.ASAP)
                {
                    lock (invocationQueue)
                    {
                        invocationQueue.Add(action);
                        actionsToBeInvoked = true;
                    }

                }
            }
        }

        public static void InvokeQueuedActions()
        {
            //if (Thread.CurrentThread.ManagedThreadId != 1)
            //    throw new Exception($"Unexpected execution thread. This function should be called from main thread");

            if (actionsToBeInvoked)
            {
                invocationQueueCopied.Clear();
                lock (invocationQueue)
                {
                    invocationQueueCopied.AddRange(invocationQueue);
                    invocationQueue.Clear();
                    actionsToBeInvoked = false;
                }

                for (int i = 0; i < invocationQueueCopied.Count; i++)
                    invocationQueueCopied[i].Invoke();
            }

        }
    }
}
