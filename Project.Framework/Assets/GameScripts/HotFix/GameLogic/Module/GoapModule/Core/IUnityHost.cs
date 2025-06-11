using UnityEngine;

namespace GameLogic.GoapModule
{
    public interface IUnityHost
    {
        Transform Transform { get; }
        GameObject GameObject { get; }
    }
}