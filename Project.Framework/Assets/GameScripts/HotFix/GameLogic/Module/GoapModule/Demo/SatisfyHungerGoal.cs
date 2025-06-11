using System;
using UnityEngine;
using UnityFramework;

namespace GameLogic.GoapModule.Demo
{
    public class SatisfyHungerGoal : IGoalTemplate
    {
        public ITarget Target { get; set; }
        public string GoalId => "SatisfyHunger";
        public float Priority => 10;
        public ICondition[] RequiredStates => new[] { new BoolCondition("Hungry", true) };
        public ICondition[] SatisfiedStates => new[] { new BoolCondition("Hungry", false) };
        public bool IsAchieved(IGoapAgent agent) => agent.WorldState.ContainsKey("Hungry") && Equals(agent.WorldState.Get("Hungry"), false);


        public event Action<IGoalTemplate> OnActivated;
        public event Action<IGoalTemplate> OnDeactivated;
        public void UpdateGoalData(IGoapAgent agent) { }
    }
}