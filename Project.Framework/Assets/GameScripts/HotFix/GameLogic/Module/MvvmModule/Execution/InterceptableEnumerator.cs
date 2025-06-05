using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework;

namespace GameLogic.Execution
{
public class InterceptableEnumerator : IEnumerator
    {
        private const int CAPACITY = 100;
        private static readonly ConcurrentQueue<InterceptableEnumerator> pools = new ConcurrentQueue<InterceptableEnumerator>();

        public static InterceptableEnumerator Create(IEnumerator routine)
        {
            InterceptableEnumerator enumerator;
            if (pools.TryDequeue(out enumerator))
            {
                enumerator.stack.Push(routine);
                return enumerator;
            }
            return new InterceptableEnumerator(routine);
        }

        private static void Free(InterceptableEnumerator enumerator)
        {
            if (pools.Count > CAPACITY)
                return;

            enumerator.Clear();
            pools.Enqueue(enumerator);
        }

        private object current;
        private Stack<IEnumerator> stack = new Stack<IEnumerator>();
        private List<Func<bool>> hasNext = new List<Func<bool>>();
        private Action<Exception> onException;
        private Action onFinally;

        public InterceptableEnumerator(IEnumerator routine)
        {
            this.stack.Push(routine);
        }

        public object Current { get { return this.current; } }

        public bool MoveNext()
        {
            try
            {
                if (!this.HasNext())
                {
                    this.OnFinally();
                    return false;
                }

                if (stack.Count <= 0)
                {
                    this.OnFinally();
                    return false;
                }

                IEnumerator ie = stack.Peek();
                bool hasNext = ie.MoveNext();
                if (!hasNext)
                {
                    this.stack.Pop();
                    return MoveNext();
                }

                this.current = ie.Current;
                if (this.current is IEnumerator)
                {
                    stack.Push(this.current as IEnumerator);
                    return MoveNext();
                }

                if (this.current is Coroutine)
                    Log.Warning("The Enumerator's results contains the 'UnityEngine.Coroutine' type,If occurs an exception,it can't be catched.It is recommended to use 'yield return routine',rather than 'yield return StartCoroutine(routine)'.");

                return true;
            }
            catch (Exception e)
            {
                this.OnException(e);
                this.OnFinally();
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        private void OnException(Exception e)
        {
            try
            {
                if (this.onException == null)
                    return;

                onException(e);
            }
            catch (Exception) { }
        }

        private void OnFinally()
        {
            try
            {
                if (this.onFinally == null)
                    return;

                onFinally();
            }
            catch (Exception) { }
            finally
            {
                Free(this);
            }
        }

        private void Clear()
        {
            this.current = null;
            this.onException = null;
            this.onFinally = null;
            this.hasNext.Clear();
            this.stack.Clear();
        }

        private bool HasNext()
        {
            if (hasNext.Count > 0)
            {
                foreach (Func<bool> action in this.hasNext)
                {
                    if (!action())
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Register a condition code block.
        /// </summary>
        /// <param name="hasNext"></param>
        public virtual void RegisterConditionBlock(Func<bool> hasNext)
        {
            if (hasNext != null)
                this.hasNext.Add(hasNext);
        }

        /// <summary>
        /// Register a code block, when an exception occurs it will be executed.
        /// </summary>
        /// <param name="onException"></param>
        public virtual void RegisterCatchBlock(Action<Exception> onException)
        {
            if (onException != null)
                this.onException += onException;
        }

        /// <summary>
        /// Register a code block, when the end of the operation is executed.
        /// </summary>
        /// <param name="onFinally"></param>
        public virtual void RegisterFinallyBlock(Action onFinally)
        {
            if (onFinally != null)
                this.onFinally += onFinally;
        }
    }
}