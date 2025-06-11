using UnityEngine;

namespace GameLogic.GoapModule
{

    // public interface IGoapAgent<TAction, TGoal>
    public interface IGoapAgent
    {
        ITarget Target { get; set; }
        Transform Self { get; }
        bool IsTerminated { get; }
        IGoapWorldState WorldState { get; }
        IActionDirectory ActionDirectory { get; }
        IGoalDirectory GoalDirectory { get; }
        IActionExecutionManager ActionExecutionManager { get; }
        IGoalExecutionManager GoalExecutionManager { get; }
        IPlanRunner PlanRunner { get; }
        IStimulusManager StimulusManager { get; }
        IBlackboard Blackboard { get; }
        void UpdateData();
        void Update(GameObject player);
    }

}