﻿namespace GameLogic.Binding.Registry
{
    public interface IKeyValueRegistry<K,V>
    {
        V Find(K key);

        V Find(K key, V defaultValue);

        void Register(K key, V value);

        void Unregister(K key);
    }
}