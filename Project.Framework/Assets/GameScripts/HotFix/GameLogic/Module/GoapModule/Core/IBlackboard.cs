using System;

namespace GameLogic.GoapModule
{
    public interface IBlackboard
    {
        void Set<T>(string key, T value);
        bool TryGet<T>(string key, out T value) where T : class,Enum;
        object TryGet(string key);
        void Remove(string key);
    }
}