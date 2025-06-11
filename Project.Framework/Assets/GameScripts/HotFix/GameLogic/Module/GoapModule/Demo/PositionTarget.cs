using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.GoapModule.Demo
{
// --- ITarget 实现
    public class PositionTarget : ITarget
    {
        public UnityEngine.Vector3 Position { get;private set; }
        public bool IsValid()
        {
            return true;
        }

        public PositionTarget(UnityEngine.Vector3 pos) { Position = pos; }

        public void SetPosition(UnityEngine.Vector3 pos)
        {
            Position = pos;
        }
    }

// --- 简单Blackboard实现
    public class SimpleBlackboard : IBlackboard
    {
        private readonly Dictionary<string, object> _data = new();
        public object this[string key] { get => _data.TryGetValue(key, out var v) ? v : null; set => _data[key] = value; }
        public void Set<T>(string key, T value)
        {
            _data.TryAdd(key, value);
            _data[key] = value;
        }

        public bool TryGet<T>(string key, out T value) where T : class,Enum
        {
            var result = _data.TryGetValue(key, out var v);
            value = v as T;
            return result;
        }

        public object TryGet(string key) 
        {
            _data.TryGetValue(key, out var v);
            return v;
        }

        public void Remove(string key)
        {
            if (_data.ContainsKey(key))
            {
                _data.Remove(key);
            }
        }
    }

}