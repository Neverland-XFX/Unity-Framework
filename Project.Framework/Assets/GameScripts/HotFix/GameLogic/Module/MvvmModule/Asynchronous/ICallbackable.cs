﻿using System;
using UnityFramework;

namespace GameLogic.Asynchronous
{
public interface ICallbackable
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IAsyncResult> callback);
    }

    public interface ICallbackable<TResult>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IAsyncResult<TResult>> callback);
    }

    public interface IProgressCallbackable<TProgress>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IProgressResult<TProgress>> callback);

        /// <summary>
        /// Called when the progress update.
        /// </summary>
        /// <param name="callback"></param>
        void OnProgressCallback(Action<TProgress> callback);
    }

    public interface IProgressCallbackable<TProgress, TResult>
    {
        /// <summary>
        /// Called when the task is finished.
        /// </summary>
        /// <param name="callback"></param>
        void OnCallback(Action<IProgressResult<TProgress, TResult>> callback);

        /// <summary>
        /// Called when the progress update.
        /// </summary>
        /// <param name="callback"></param>
        void OnProgressCallback(Action<TProgress> callback);
    }

    internal class Callbackable : ICallbackable
    {
        private IAsyncResult result;
        private readonly object _lock = new object();
        private Action<IAsyncResult> callback;
        public Callbackable(IAsyncResult result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this.callback == null)
                        return;

                    var list = this.callback.GetInvocationList();
                    this.callback = null;

                    foreach (Action<IAsyncResult> action in list)
                    {
                        try
                        {
                            action(this.result);
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

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.callback += callback;
            }
        }
    }

    internal class Callbackable<TResult> : ICallbackable<TResult>
    {
        private IAsyncResult<TResult> result;
        private readonly object _lock = new object();
        private Action<IAsyncResult<TResult>> callback;
        public Callbackable(IAsyncResult<TResult> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this.callback == null)
                        return;

                    var list = this.callback.GetInvocationList();
                    this.callback = null;

                    foreach (Action<IAsyncResult<TResult>> action in list)
                    {
                        try
                        {
                            action(this.result);
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

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.callback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress> : IProgressCallbackable<TProgress>
    {
        private IProgressResult<TProgress> result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress>> callback;
        private Action<TProgress> progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this.callback == null)
                        return;

                    var list = this.callback.GetInvocationList();
                    this.callback = null;

                    foreach (Action<IProgressResult<TProgress>> action in list)
                    {
                        try
                        {
                            action(this.result);
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
                    this.progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (this.progressCallback == null)
                        return;

                    var list = this.progressCallback.GetInvocationList();
                    foreach (Action<TProgress> action in list)
                    {
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

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result.Progress);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.progressCallback += callback;
            }
        }
    }

    internal class ProgressCallbackable<TProgress, TResult> : IProgressCallbackable<TProgress, TResult>
    {
        private IProgressResult<TProgress, TResult> result;
        private readonly object _lock = new object();
        private Action<IProgressResult<TProgress, TResult>> callback;
        private Action<TProgress> progressCallback;
        public ProgressCallbackable(IProgressResult<TProgress, TResult> result)
        {
            this.result = result;
        }

        public void RaiseOnCallback()
        {
            lock (_lock)
            {
                try
                {
                    if (this.callback == null)
                        return;

                    var list = this.callback.GetInvocationList();
                    this.callback = null;

                    foreach (Action<IProgressResult<TProgress, TResult>> action in list)
                    {
                        try
                        {
                            action(this.result);
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
                    this.progressCallback = null;
                }
            }
        }

        public void RaiseOnProgressCallback(TProgress progress)
        {
            lock (_lock)
            {
                try
                {
                    if (this.progressCallback == null)
                        return;

                    var list = this.progressCallback.GetInvocationList();
                    foreach (Action<TProgress> action in list)
                    {
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

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.callback += callback;
            }
        }

        public void OnProgressCallback(Action<TProgress> callback)
        {
            lock (_lock)
            {
                if (callback == null)
                    return;

                if (this.result.IsDone)
                {
                    try
                    {
                        callback(this.result.Progress);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Class[{0}] progress callback exception.Error:{1}", this.GetType(), e);
                    }
                    return;
                }

                this.progressCallback += callback;
            }
        }
    }
}