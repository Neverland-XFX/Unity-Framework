using System;

namespace GameLogic.GoapModule
{

    public interface IStimulus
    {
        int Priority { get; }
        bool IsTriggered { get; }
        void Update();
        event Action<IStimulus> OnTriggered;
    }

}