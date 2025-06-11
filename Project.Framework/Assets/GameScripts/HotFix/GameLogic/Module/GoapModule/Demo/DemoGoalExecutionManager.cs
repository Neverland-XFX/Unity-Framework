namespace GameLogic.GoapModule.Demo
{
    public class DemoGoalExecutionManager : GoalExecutionManagerBase
    {
        public DemoGoalExecutionManager(IGoapAgent agent) : base(agent)
        {
        }

        protected override void InitGoals()
        {
            GoalDict.Add("Eat",new SatisfyHungerGoal());
        }
    }
}