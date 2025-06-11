namespace GameLogic.GoapModule
{

    public interface IGoalExecutionManager
    {
        IGoalTemplate CurrentGoal { get; }
        void AddGoal(IGoalTemplate goal);
        void RemoveGoal(string goalId);
        IGoalTemplate GetGoal(string goalId);
        IGoalTemplate SelectBestGoal(IGoapWorldState worldState);
        void Update();
    }
}