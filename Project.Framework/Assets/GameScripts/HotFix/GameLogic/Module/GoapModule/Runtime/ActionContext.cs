using System.Collections.Generic;

namespace GameLogic.GoapModule
{
    public class ActionContext : IActionContext
    {
        public ITarget Target { get; }
        public IBlackboard Blackboard { get; }
        public float DeltaTime { get; }
        public float ElapsedTime { get; }
        public bool IsInRange { get; }

        private readonly Dictionary<string, object> _data = new();

        public ActionContext(
            ITarget target = null,
            IBlackboard blackboard = null,
            float deltaTime = 0f,
            float elapsedTime = 0f,
            bool isInRange = false)
        {
            Target = target;
            Blackboard = blackboard;
            DeltaTime = deltaTime;
            ElapsedTime = elapsedTime;
            IsInRange = isInRange;
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }

        public bool TryGetData<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }
    }

}