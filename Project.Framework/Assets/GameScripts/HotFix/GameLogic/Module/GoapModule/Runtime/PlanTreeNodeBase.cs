namespace GameLogic.GoapModule
{
    /// <summary>
    /// 规划用树节点：每个节点存一个行为处理器（或动作模板）、父节点、世界状态、总代价等
    /// </summary>
    public class PlanTreeNodeBase
    {
        private static int _idCounter = 0;

        public int Id { get; private set; }
        public PlanTreeNodeBase Parent { get; set; }
        public IActionTemplate ActionTemplate { get; }
        public IGoapWorldState CurrentState { get; set; }
        public IGoapWorldState GoalState { get; set; }
        public float Cost { get; set; }

        public PlanTreeNodeBase(IActionTemplate actionTemplate, IGoapWorldState current, IGoapWorldState goal)
        {
            Id = _idCounter++;
            ActionTemplate = actionTemplate;
            Parent = null;
            CurrentState = current?.Clone() ?? new GoapWorldStateBase();
            GoalState = goal?.Clone() ?? new GoapWorldStateBase();
            Cost = 0f;
        }

        /// <summary>
        /// 克隆此节点（包括状态），不拷贝父链
        /// </summary>
        public PlanTreeNodeBase CloneShallow()
        {
            return new PlanTreeNodeBase(ActionTemplate, CurrentState, GoalState)
            {
                Cost = this.Cost
            };
        }

        /// <summary>
        /// 重置全局ID计数器（用于新一轮规划）
        /// </summary>
        public static void ResetId() => _idCounter = 0;
    }
}