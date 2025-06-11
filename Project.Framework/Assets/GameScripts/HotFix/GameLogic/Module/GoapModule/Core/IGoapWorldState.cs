using System;
using System.Collections.Generic;

namespace GameLogic.GoapModule
{
    public interface IGoapWorldState
    {
        void Set(string key, object value);
        object Get(string key);
        bool ContainsKey(string key);
        void Remove(string key);
        IEnumerable<string> Keys { get; }
        void MergeFrom(IGoapWorldState other);
        IGoapWorldState Clone();
        IEnumerable<string> DiffKeys(IGoapWorldState other);
        event Action<string, object> OnStateChanged;
        string ToHashString(); // 用于状态去重
    }
}