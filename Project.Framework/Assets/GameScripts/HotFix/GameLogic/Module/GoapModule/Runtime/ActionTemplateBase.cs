namespace GameLogic.GoapModule
{
    public abstract class ActionTemplateBase : IActionTemplate
    {
        public abstract string ActionId { get; }
        public abstract int Cost { get; }
        public abstract int Priority { get; }
        public abstract bool IsInterruptible { get; }

        public abstract ICondition[] Preconditions { get; }
        public abstract ICondition[] Effects { get; }

        /// <summary>
        /// 检查此行为在当前agent与context下是否可执行
        /// </summary>
        public virtual bool CanExecute(IGoapAgent agent, IActionContext context)
        {
            foreach (var pre in Preconditions)
                if (!pre.IsMet(agent, agent.WorldState))
                    return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IActionContext CreateContext(IGoapAgent agent, ITarget target)
        {
            // 默认实现：只传递 target（可继承扩展更多参数）
            return new ActionContext(target, agent?.Blackboard);
        }

        public virtual IActionContext CreateContext(IGoapAgent agent, IBlackboard blackboard)
        {
            // 默认实现：只传递 blackboard，没有目标时 target 为空
            return new ActionContext(null, blackboard);
        }
    }
}