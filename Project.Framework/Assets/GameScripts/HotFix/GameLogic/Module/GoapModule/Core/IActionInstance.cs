using System;

namespace GameLogic.GoapModule
{

    public interface IActionInstance
    {
        IActionTemplate Template { get; }
        IActionContext Context { get; }
        ActionState State { get; }
        void Start();
        void Update();
        void Stop();
        event Action<IActionInstance> OnCompleted;
        event Action<IActionInstance> OnInterrupted;
        void Interrupt();
    }
}