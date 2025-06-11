using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace GameLogic.GoapModule.Demo
{
    public class GoalDirectory : IGoalDirectory
    {
        private readonly Dictionary<string, IGoalTemplate> _goals = new();

        public bool TryGetGoal(string goalId, out IGoalTemplate goal)
            => _goals.TryGetValue(goalId, out goal);

        public IEnumerable<IGoalTemplate> GetAllGoals()
            => _goals.Values;

        public void RegisterGoal(string goalId, IGoalTemplate goal, ITarget target)
        {
            _goals[goalId] = goal;
            _goals[goalId].Target = target;
        }

        public void UnregisterGoal(string goalId)
            => _goals.Remove(goalId);

        public void Update(){ }
    }
}