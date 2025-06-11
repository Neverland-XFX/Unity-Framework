namespace GameLogic.GoapModule
{
    public interface IActionContext
    {
        ITarget Target { get; }
        IBlackboard Blackboard { get; }
        float DeltaTime { get; }
        float ElapsedTime { get; }
        bool IsInRange { get; }
        void SetData(string key, object value);
        bool TryGetData<T>(string key, out T value);
    }

}