﻿using System;
using System.Threading;
using GameLogic.Execution;

namespace GameLogic.Asynchronous
{
public class AsyncResult : IAsyncResult, IPromise
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(AsyncResult));

        private bool done = false;
        private object result = null;
        private Exception exception = null;

        private bool cancelled = false;
        protected bool cancelable = false;
        protected bool cancellationRequested;

        protected readonly object _lock = new object();

        private Synchronizable synchronizable;
        private Callbackable callbackable;

        public AsyncResult() : this(false)
        {
        }

        public AsyncResult(bool cancelable)
        {
            this.cancelable = cancelable;
        }

        /// <summary>
        /// Exception
        /// </summary>
        public virtual Exception Exception
        {
            get { return this.exception; }
        }

        /// <summary>
        /// Returns  "true" if this task finished.
        /// </summary>
        public virtual bool IsDone
        {
            get { return this.done; }
        }

        /// <summary>
        /// The execution result
        /// </summary>
        public virtual object Result
        {
            get { return this.result; }
        }

        public virtual bool IsCancellationRequested
        {
            get { return this.cancellationRequested; }
        }

        /// <summary>
        /// Returns "true" if this task was cancelled before it completed normally.
        /// </summary>
        public virtual bool IsCancelled
        {
            get { return this.cancelled; }
        }

        public virtual void SetException(string error)
        {
            if (this.done)
                return;

            var exception = new Exception(string.IsNullOrEmpty(error) ? "unknown error!" : error);
            SetException(exception);
        }

        public virtual void SetException(Exception exception)
        {
            lock (_lock)
            {
                if (this.done)
                    return;

                this.exception = exception;
                this.done = true;
                Monitor.PulseAll(_lock);
            }

            this.RaiseOnCallback();
        }

        public virtual void SetResult(object result = null)
        {
            lock (_lock)
            {
                if (this.done)
                    return;

                this.result = result;
                this.done = true;
                Monitor.PulseAll(_lock);
            }

            this.RaiseOnCallback();
        }

        public virtual void SetCancelled()
        {
            lock (_lock)
            {
                if (!this.cancelable || this.done)
                    return;

                this.cancelled = true;
                this.exception = new OperationCanceledException();
                this.done = true;
                Monitor.PulseAll(_lock);
            }

            this.RaiseOnCallback();
        }

        /// <summary>
        /// Attempts to cancel execution of this task.  This attempt will 
        /// fail if the task has already completed, has already been cancelled,
        /// or could not be cancelled for some other reason.If successful,
        /// and this task has not started when "Cancel" is called,
        /// this task should never run. 
        /// </summary>
        /// <exception cref="NotSupportedException">If not supported, throw an exception.</exception>
        /// <returns></returns>
        public virtual bool Cancel()
        {
            if (!this.cancelable)
                throw new NotSupportedException();

            if (this.IsDone)
                return false;

            this.cancellationRequested = true;
            this.SetCancelled();
            return true;
        }

        protected virtual void RaiseOnCallback()
        {
            if (this.callbackable != null)
                this.callbackable.RaiseOnCallback();
        }

        public virtual ICallbackable Callbackable()
        {
            lock (_lock)
            {
                return this.callbackable ?? (this.callbackable = new Callbackable(this));
            }
        }

        public virtual ISynchronizable Synchronized()
        {
            lock (_lock)
            {
                return this.synchronizable ?? (this.synchronizable = new Synchronizable(this, this._lock));
            }
        }

        /// <summary>
        /// Wait for the result,suspends the coroutine.
        /// eg:
        /// IAsyncResult result;
        /// yiled return result.WaitForDone();
        /// </summary>
        /// <returns></returns>
        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }
    }

    public class AsyncResult<TResult> : AsyncResult, IAsyncResult<TResult>, IPromise<TResult>
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(AsyncResult<TResult>));

        private Synchronizable<TResult> synchronizable;
        private Callbackable<TResult> callbackable;

        public AsyncResult() : this(false)
        {
        }

        public AsyncResult(bool cancelable) : base(cancelable)
        {
        }

        /// <summary>
        /// The execution result
        /// </summary>
        public virtual new TResult Result
        {
            get
            {
                var result = base.Result;
                return result != null ? (TResult)result : default(TResult);
            }
        }

        public virtual void SetResult(TResult result)
        {
            base.SetResult(result);
        }

        protected override void RaiseOnCallback()
        {
            base.RaiseOnCallback();
            if (this.callbackable != null)
                this.callbackable.RaiseOnCallback();
        }

        public new virtual ICallbackable<TResult> Callbackable()
        {
            lock (_lock)
            {
                return this.callbackable ?? (this.callbackable = new Callbackable<TResult>(this));
            }
        }

        public new virtual ISynchronizable<TResult> Synchronized()
        {
            lock (_lock)
            {
                return this.synchronizable ?? (this.synchronizable = new Synchronizable<TResult>(this, this._lock));
            }
        }
    }
}