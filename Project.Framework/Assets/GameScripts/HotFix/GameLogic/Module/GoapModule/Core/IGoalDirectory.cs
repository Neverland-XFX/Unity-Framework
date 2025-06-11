using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.GoapModule
{

    public interface IGoalDirectory
    {
        bool TryGetGoal(string goalId, out IGoalTemplate goal);
        IEnumerable<IGoalTemplate> GetAllGoals();
        void RegisterGoal(string goalId, IGoalTemplate goal, ITarget target);
        void UnregisterGoal(string goalId);
        void Update();
    }

}