using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.GoapModule
{
    /// <summary>
    /// 
    /// </summary>
    public class GoapWorldStateBase : IGoapWorldState
    {
        private readonly Dictionary<string, object> _data = new();
        public event Action<string, object> OnStateChanged;

        public string ToHashString()
        {
            // 排序后拼接所有key-value
            var keys = _data.Keys.OrderBy(k => k, StringComparer.Ordinal);
            var sb = new StringBuilder(64);

            foreach (var key in keys)
            {
                var val = _data[key];
                sb.Append(key).Append('=').Append(val?.ToString() ?? "null").Append(';');
            }
            return sb.ToString();
        }


        public void Set(string key, object value)
        {
            _data.TryAdd(key, value);
            if (_data.TryGetValue(key, out var oldVal))
            {
                if ((oldVal == null && value == null) || (oldVal != null && oldVal.Equals(value)))
                    return; // 没变
            }

            _data[key] = value;
            OnStateChanged?.Invoke(key, value);
        }

        public object Get(string key)
        {
            _data.TryGetValue(key, out var val);
            return val;
        }

        public bool ContainsKey(string key) => _data.ContainsKey(key);

        public void Remove(string key) => _data.Remove(key);

        public IEnumerable<string> Keys => _data.Keys;

        /// <summary>
        /// 合并另一状态，已存在的key会被覆盖
        /// </summary>
        public void MergeFrom(IGoapWorldState other)
        {
            if (other == null) return;
            foreach (var key in other.Keys)
                Set(key, other.Get(key));
        }

        /// <summary>
        /// 深拷贝快照
        /// </summary>
        public IGoapWorldState Clone()
        {
            var newState = new GoapWorldStateBase();
            foreach (var key in _data.Keys)
                newState._data[key] = _data[key];
            return newState;
        }

        /// <summary>
        /// 找出与另一状态的不同键集合
        /// </summary>
        public IEnumerable<string> DiffKeys(IGoapWorldState other)
        {
            var diff = new List<string>();
            foreach (var key in Keys)
            {
                if (!other.ContainsKey(key) || !Equals(other.Get(key), Get(key)))
                    diff.Add(key);
            }

            foreach (var key in other.Keys)
            {
                if (!ContainsKey(key))
                    diff.Add(key);
            }

            return diff;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var pair in _data)
                sb.AppendLine($"{pair.Key}: {pair.Value}");
            return sb.ToString();
        }
    }
    
    public class GoapWorldState<TKey> : GoapWorldStateBase
        where TKey : notnull
    {
        public void Set(TKey key, object value) => base.Set(key.ToString(), value);
        public object Get(TKey key) => base.Get(key.ToString());
        public bool ContainsKey(TKey key) => base.ContainsKey(key.ToString());
    }
}