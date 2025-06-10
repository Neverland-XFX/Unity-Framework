using System;

namespace GameLogic.Asynchronous
{
    public interface IPromise
    {
        object Result { get; }

        Exception Exception { get; }

        bool IsDone { get; }

        bool IsCancelled { get; }

        bool IsCancellationRequested { get; }

        void SetCancelled();

        void SetException(string error);

        void SetException(Exception exception);

        void SetResult(object result = null);
    }

    public interface IPromise<TResult> : IPromise
    {
        new TResult Result { get; }

        void SetResult(TResult result);
    }
}