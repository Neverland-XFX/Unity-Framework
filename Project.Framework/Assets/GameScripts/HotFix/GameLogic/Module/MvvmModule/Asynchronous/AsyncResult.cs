using System;
using System.Threading;
using GameLogic.Execution;

namespace GameLogic.Asynchronous
{
public class AsyncResult : IAsyncResult, IPromise
    {
        private bool _done = false;
        private object _result = null;
        private Exception _exception = null;

        private bool _cancelled = false;
        private readonly bool _cancelable = false;
        private bool _cancellationRequested;

        protected readonly object Lock = new object();

        private Synchronizable _synchronizable;
        private Callbackable _callbackable;

        public AsyncResult() : this(false)
        {
        }

        protected AsyncResult(bool cancelable)
        {
            _cancelable = cancelable;
        }

        public virtual Exception Exception => _exception;

        public virtual bool IsDone => _done;

        public virtual object Result => _result;

        public virtual bool IsCancellationRequested => _cancellationRequested;

        public virtual bool IsCancelled => _cancelled;

        public virtual void SetException(string error)
        {
            if (_done)
                return;

            var exception = new Exception(string.IsNullOrEmpty(error) ? "unknown error!" : error);
            SetException(exception);
        }

        public virtual void SetException(Exception exception)
        {
            lock (Lock)
            {
                if (_done)
                    return;

                _exception = exception;
                _done = true;
                Monitor.PulseAll(Lock);
            }

            RaiseOnCallback();
        }

        public virtual void SetResult(object result = null)
        {
            lock (Lock)
            {
                if (_done)
                    return;

                _result = result;
                _done = true;
                Monitor.PulseAll(Lock);
            }

            RaiseOnCallback();
        }

        public virtual void SetCancelled()
        {
            lock (Lock)
            {
                if (!_cancelable || _done)
                    return;

                _cancelled = true;
                _exception = new OperationCanceledException();
                _done = true;
                Monitor.PulseAll(Lock);
            }

            RaiseOnCallback();
        }

        public virtual bool Cancel()
        {
            if (!_cancelable)
                throw new NotSupportedException();

            if (IsDone)
                return false;

            _cancellationRequested = true;
            SetCancelled();
            return true;
        }

        protected virtual void RaiseOnCallback()
        {
            if (_callbackable != null)
                _callbackable.RaiseOnCallback();
        }

        public virtual ICallbackable Callbackable()
        {
            lock (Lock)
            {
                return _callbackable ??= new Callbackable(this);
            }
        }

        public virtual ISynchronizable Synchronized()
        {
            lock (Lock)
            {
                return _synchronizable ??= new Synchronizable(this, Lock);
            }
        }

        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }
    }

    public sealed class AsyncResult<TResult> : AsyncResult, IAsyncResult<TResult>, IPromise<TResult>
    {
        private Synchronizable<TResult> synchronizable;
        private Callbackable<TResult> callbackable;

        public AsyncResult() : this(false)
        {
        }

        private AsyncResult(bool cancelable) : base(cancelable)
        {
        }

        /// <summary>
        /// The execution result
        /// </summary>
        public new TResult Result
        {
            get
            {
                var result = base.Result;
                return result != null ? (TResult)result : default(TResult);
            }
        }

        public void SetResult(TResult result)
        {
            base.SetResult(result);
        }

        protected override void RaiseOnCallback()
        {
            base.RaiseOnCallback();
            callbackable?.RaiseOnCallback();
        }

        public new ICallbackable<TResult> Callbackable()
        {
            lock (Lock)
            {
                return callbackable ??= new Callbackable<TResult>(this);
            }
        }

        public new ISynchronizable<TResult> Synchronized()
        {
            lock (Lock)
            {
                return synchronizable ??= new Synchronizable<TResult>(this, Lock);
            }
        }
    }
}