namespace GameLogic.GoapModule
{
    /// <summary>
    /// 规划用树结构（工厂与根节点）
    /// </summary>
    public class PlanSearchTreeBase<TAction>
    {
        //支持多ActionHandler/ActionInstance,再加相应构造函数即可，无需大改。
        //public IActionInstance<TAction> ActionInstance { get; }
        //public IActionHandler<TAction> ActionHandler { get; }
        
        
        public PlanTreeNodeBase CreateNode(IActionTemplate action, IGoapWorldState current, IGoapWorldState goal)
        {
            return new PlanTreeNodeBase(action, current, goal);
        }

        public PlanTreeNodeBase CreateRootNode(IGoapWorldState world, IGoapWorldState goal)
        {
            PlanTreeNodeBase.ResetId();
            return new PlanTreeNodeBase(null, world, goal);
        }
    }
}