using System;
using System.Collections.Generic;

namespace GameLogic.GoapModule
{

    public interface IPlanRunner
    {
        bool IsPlanComplete { get; }
        void Initialize(IActionExecutionManager manager, IEnumerable<IActionTemplate> plan);
        void Start();
        void Step();
        void Interrupt();
        event Action OnPlanCompleted;
        event Action OnPlanInterrupted;
        IActionInstance CurrentAction { get; }
        int CurrentStepIndex { get; }
    }
}