namespace GameLogic.GoapModule
{

    public interface IActionTemplate
    {
        string ActionId { get; }
        int Cost { get; }
        int Priority { get; }
        bool IsInterruptible { get; }
        ICondition[] Preconditions { get; }
        ICondition[] Effects { get; }
        bool CanExecute(IGoapAgent agent, IActionContext context);
        IActionContext CreateContext(IGoapAgent agent, ITarget target);
        IActionContext CreateContext(IGoapAgent agent, IBlackboard blackboard);
    }
}