using System;
using UnityEngine;

namespace GameLogic.GoapModule
{

    public interface IGoalTemplate
    {
        ITarget Target { get; set; }
        string GoalId { get; }
        float Priority { get; }
        ICondition[] RequiredStates { get; }
        ICondition[] SatisfiedStates { get; }
        bool IsAchieved(IGoapAgent agent);
        event Action<IGoalTemplate> OnActivated;
        event Action<IGoalTemplate> OnDeactivated;
        void UpdateGoalData(IGoapAgent agent);
    }
}