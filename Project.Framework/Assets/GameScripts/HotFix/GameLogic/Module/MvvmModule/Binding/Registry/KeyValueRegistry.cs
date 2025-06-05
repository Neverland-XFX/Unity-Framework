using System.Collections.Generic;
using UnityFramework;

namespace GameLogic.Binding.Registry
{
    public class KeyValueRegistry<K, V> : IKeyValueRegistry<K, V>
    {

        protected readonly Dictionary<K, V> lookups = new Dictionary<K, V>();

        public virtual V Find(K key)
        {
            V toReturn;
            this.lookups.TryGetValue(key, out toReturn);
            return toReturn;
        }

        public virtual V Find(K key, V defaultValue)
        {
            V toReturn;
            if (this.lookups.TryGetValue(key, out toReturn))
                return toReturn;

            return defaultValue;
        }

        public virtual void Register(K key, V value)
        {
            if (this.lookups.ContainsKey(key))
            {
                Log.Warning("The Key({0}) already exists", key);
            }
            this.lookups[key] = value;
        }

        public virtual void Unregister(K key)
        {
            this.lookups.Remove(key);
        }
    }
}