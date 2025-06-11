using System.Collections.Generic;
using System.Linq;

namespace GameLogic.GoapModule
{
public abstract class GoalExecutionManagerBase : IGoalExecutionManager
{
    protected readonly Dictionary<string, IGoalTemplate> GoalDict = new();
    protected readonly List<IGoalTemplate> ActiveGoals = new();
    protected IGoapAgent Agent { get; private set; }

    public IGoalTemplate CurrentGoal { get; private set; }

    public GoalExecutionManagerBase(IGoapAgent agent)
    {
        Agent = agent;
        InitGoals();
        BindGoalEvents();
    }

    /// <summary>
    /// 子类实现：注册所有目标
    /// </summary>
    protected abstract void InitGoals();

    /// <summary>
    /// 绑定目标的激活/失活事件
    /// </summary>
    private void BindGoalEvents()
    {
        foreach (var goal in GoalDict.Values)
        {
            goal.OnActivated += OnGoalActivated;
            goal.OnDeactivated += OnGoalDeactivated;
        }
    }

    private void OnGoalActivated(IGoalTemplate goal)
    {
        if (!ActiveGoals.Contains(goal))
            ActiveGoals.Add(goal);
    }
    private void OnGoalDeactivated(IGoalTemplate goal)
    {
        ActiveGoals.Remove(goal);
    }

    public void AddGoal(IGoalTemplate goal)
    {
        if (!GoalDict.ContainsKey(goal.GoalId))
        {
            GoalDict.Add(goal.GoalId, goal);
            goal.OnActivated += OnGoalActivated;
            goal.OnDeactivated += OnGoalDeactivated;
        }
    }

    public void RemoveGoal(string goalId)
    {
        if (GoalDict.TryGetValue(goalId, out var goal))
        {
            goal.OnActivated -= OnGoalActivated;
            goal.OnDeactivated -= OnGoalDeactivated;
            GoalDict.Remove(goalId);
            ActiveGoals.Remove(goal);
        }
    }

    public IGoalTemplate GetGoal(string goalId)
    {
        GoalDict.TryGetValue(goalId, out var goal);
        return goal;
    }

    /// <summary>
    /// 返回当前所有激活目标中优先级最高的一个
    /// </summary>
    public IGoalTemplate SelectBestGoal(IGoapWorldState worldState)
    {
        if (ActiveGoals.Count == 0) return null;
        return ActiveGoals.OrderByDescending(g => g.Priority).FirstOrDefault();
    }

    public virtual void Update()
    {
        foreach (var goal in GoalDict.Values)
            goal.UpdateGoalData(Agent);

        CurrentGoal = SelectBestGoal(Agent.WorldState);
    }
}
}