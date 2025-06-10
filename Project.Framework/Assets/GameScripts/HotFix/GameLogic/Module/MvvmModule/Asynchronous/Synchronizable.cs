using System;
using System.Threading;

namespace GameLogic.Asynchronous
{
public interface ISynchronizable
    {
        bool WaitForDone();

        object WaitForResult(int millisecondsTimeout = 0);

        object WaitForResult(TimeSpan timeout);
    }

    public interface ISynchronizable<TResult> : ISynchronizable
    {
        new TResult WaitForResult(int millisecondsTimeout = 0);

        new TResult WaitForResult(TimeSpan timeout);
    }

    internal class Synchronizable : ISynchronizable
    {
        private readonly IAsyncResult _result;
        private readonly object _lock;
        public Synchronizable(IAsyncResult result, object @lock)
        {
            this._result = result;
            this._lock = @lock;
        }

        public bool WaitForDone()
        {
            if (_result.IsDone)
                return _result.IsDone;

            lock (_lock)
            {
                if (!_result.IsDone)
                    Monitor.Wait(_lock);
            }

            return _result.IsDone;
        }

        /// <summary>
        /// Wait for the result,will block the current thread.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public object WaitForResult(int millisecondsTimeout = 0)
        {
            if (_result.IsDone)
            {
                if (_result.Exception != null)
                    throw _result.Exception;

                return _result.Result;
            }

            lock (_lock)
            {
                if (!_result.IsDone)
                {
                    if (millisecondsTimeout > 0)
                        Monitor.Wait(_lock, millisecondsTimeout);
                    else
                        Monitor.Wait(_lock);
                }
            }

            if (!_result.IsDone)
                throw new TimeoutException();

            if (_result.Exception != null)
                throw _result.Exception;

            return _result.Result;
        }

        /// <summary>
        ///  Wait for the result,will block the current thread.
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public object WaitForResult(TimeSpan timeout)
        {
            if (_result.IsDone)
            {
                if (_result.Exception != null)
                    throw _result.Exception;

                return _result.Result;
            }

            lock (_lock)
            {
                if (!_result.IsDone)
                {
                    Monitor.Wait(_lock, timeout);
                }
            }

            if (!_result.IsDone)
                throw new TimeoutException();

            if (_result.Exception != null)
                throw _result.Exception;

            return _result.Result;
        }
    }

    internal class Synchronizable<TResult> : ISynchronizable<TResult>
    {
        private readonly IAsyncResult<TResult> _result;
        private readonly object _lock;
        public Synchronizable(IAsyncResult<TResult> result, object @lock)
        {
            this._result = result;
            this._lock = @lock;
        }

        /// <summary>
        /// Wait for done,will block the current thread.
        /// </summary>
        /// <returns></returns>
        public bool WaitForDone()
        {
            if (_result.IsDone)
                return _result.IsDone;

            lock (_lock)
            {
                if (!_result.IsDone)
                    Monitor.Wait(_lock);
            }

            return _result.IsDone;
        }

        /// <summary>
        /// Wait for the result,will block the current thread.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public TResult WaitForResult(int millisecondsTimeout = 0)
        {
            if (_result.IsDone)
            {
                if (_result.Exception != null)
                    throw _result.Exception;

                return _result.Result;
            }

            lock (_lock)
            {
                if (!_result.IsDone)
                {
                    if (millisecondsTimeout > 0)
                        Monitor.Wait(_lock, millisecondsTimeout);
                    else
                        Monitor.Wait(_lock);
                }
            }

            if (!_result.IsDone)
                throw new TimeoutException();

            if (_result.Exception != null)
                throw _result.Exception;

            return _result.Result;
        }

        /// <summary>
        /// Wait for the result,will block the current thread.
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public TResult WaitForResult(TimeSpan timeout)
        {
            if (_result.IsDone)
            {
                if (_result.Exception != null)
                    throw _result.Exception;

                return _result.Result;
            }

            lock (_lock)
            {
                if (!_result.IsDone)
                {
                    Monitor.Wait(_lock, timeout);
                }
            }

            if (!_result.IsDone)
                throw new TimeoutException();

            if (_result.Exception != null)
                throw _result.Exception;

            return _result.Result;
        }

        object ISynchronizable.WaitForResult(int millisecondsTimeout)
        {
            return WaitForResult(millisecondsTimeout);
        }

        object ISynchronizable.WaitForResult(TimeSpan timeout)
        {
            return WaitForResult(timeout);
        }
    }
}