namespace GameLogic.Asynchronous
{
    public interface IProgressPromise<TProgress> : IPromise
    {
        TProgress Progress { get; }

        void UpdateProgress(TProgress progress);
    }

    public interface IProgressPromise<TProgress, TResult> : IProgressPromise<TProgress>, IPromise<TResult>
    {
    }
}