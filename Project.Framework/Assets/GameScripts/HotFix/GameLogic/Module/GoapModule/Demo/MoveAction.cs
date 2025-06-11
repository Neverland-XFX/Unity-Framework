namespace GameLogic.GoapModule.Demo
{
    public class MoveAction : ActionTemplateBase
    {
        public override string ActionId => "Move";
        public override int Cost => 1;
        public override int Priority => 10;
        public override bool IsInterruptible => true;

        public override ICondition[] Preconditions => new[] {
            new BoolCondition("HasPath", true)
        };

        public override ICondition[] Effects => new[] {
            new BoolCondition("AtTarget", true),
            new BoolCondition("Hungry", true),
        };

        public override bool CanExecute(IGoapAgent agent, IActionContext context)
        {
            // 额外条件判断（如体力足够）
            return base.CanExecute(agent, context)
                   && agent.Blackboard.TryGet("Stamina") is > 1.0f;
        }
    }

}