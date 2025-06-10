using System;
using UnityFramework;

namespace GameLogic.Asynchronous
{
public interface ICallbackable
    {
        void OnCallback(Action<IAsyncResult> callback);
    }

    public interface ICallbackable<TResult>
    {
        void OnCallback(Action<IAsyncResult<TResult>> callback);
    }

    public interface IProgressCallbackable<TProgress>
    {
        void OnCallback(Action<IProgressResult<TProgress>> callback);

        void OnProgressCallback(Action<TProgress> callback);
    }

    public interface IProgressCallbackable<TProgress, TResult>
    {
        void OnCallback(Action<IProgressResult<TProgress, TResult>> callback);

        void OnProgressCallback(Action<TProgress> callback);
    }

    internal class Callbackable : ICallbackable
    {
        private readonly IAsyncResult _result;
        private readonly object _lock = new object();
        private Action<IAsyncResult> _callback;
        public Callbackable(IAsyncResult result)
        {
            this._result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this._callback == null)
                        return;

                    var list = this._callback.GetInvocationList();
                    this._callback = null;

                    foreach (var @delegate in list)
                    {
                        var action = (Action<IAsyncResult>)@delegate;
                        try
                        {
                            action(this._result);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                }
            }
        }

        public void OnCallback(Action<IAsyncResult> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._callback += callback;
            }
        }
    }

    internal class Callbackable<TResult> : ICallbackable<TResult>
    {
        private readonly IAsyncResult<TResult> _result;
        private readonly object _lock = new object();
        private Action<IAsyncResult<TResult>> _callback;
        public Callbackable(IAsyncResult<TResult> result)
        {
            this._result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this._callback == null)
                        return;

                    var list = this._callback.GetInvocationList();
                    this._callback = null;

                    foreach (var @delegate in list)
                    {
                        var action = (Action<IAsyncResult<TResult>>)@delegate;
                        try
                        {
                            action(this._result);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                }
            }
        }

        public void OnCallback(Action<IAsyncResult<TResult>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._callback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress> : IProgressCallbackable<TProgress>
    {
        private readonly IProgressResult<TProgress> _result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress>> _callback;
        private Action<TProgress> _progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress> result)
        {
            this._result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this._callback == null)
                        return;

                    var list = this._callback.GetInvocationList();
                    this._callback = null;

                    foreach (var @delegate in list)
                    {
                        var action = (Action<IProgressResult<TProgress>>)@delegate;
                        try
                        {
                            action(this._result);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                }
                finally
                {
                    this._progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (this._progressCallback == null)
                        return;

                    var list = this._progressCallback.GetInvocationList();
                    foreach (var @delegate in list)
                    {
                        var action = (Action<TProgress>)@delegate;
                        try
                        {
                            action(progress);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                }
            }
        }

        public void OnCallback(Action<IProgressResult<TProgress>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result.Progress);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._progressCallback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress, TResult> : IProgressCallbackable<TProgress, TResult>
    {
        private readonly IProgressResult<TProgress, TResult> _result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress, TResult>> _callback;
        private Action<TProgress> _progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress, TResult> result)
        {
            this._result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this._callback == null)
                        return;

                    var list = this._callback.GetInvocationList();
                    this._callback = null;

                    foreach (var @delegate in list)
                    {
                        var action = (Action<IProgressResult<TProgress, TResult>>)@delegate;
                        try
                        {
                            action(this._result);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                }
                finally
                {
                    this._progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (this._progressCallback == null)
                        return;

                    var list = this._progressCallback.GetInvocationList();
                    foreach (var @delegate in list)
                    {
                        var action = (Action<TProgress>)@delegate;
                        try
                        {
                            action(progress);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                }
            }
        }

        public void OnCallback(Action<IProgressResult<TProgress, TResult>> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this._result.IsDone)
                {
                    try
                    {
                        callback(this._result.Progress);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this._progressCallback += callback;
            }
        }
    }
}