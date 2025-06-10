using System;

namespace GameLogic.Asynchronous
{
public interface IAsyncResult
    {
        object Result { get; }

        Exception Exception { get; }

        bool IsDone { get; }

        bool IsCancelled { get; }

        bool Cancel();

        ICallbackable Callbackable();

        ISynchronizable Synchronized();

        object WaitForDone();

    }

    public interface IAsyncResult<TResult> : IAsyncResult
    {
        new TResult Result { get; }

        new ICallbackable<TResult> Callbackable();

        new ISynchronizable<TResult> Synchronized();
    }
}