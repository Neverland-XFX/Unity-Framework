namespace GameLogic.GoapModule.Demo
{
    public class EatAction : ActionTemplateBase
    {
        public override string ActionId => "Eat";
        public override int Cost => 2;
        public override int Priority => 20;
        public override bool IsInterruptible => false;

        public override ICondition[] Preconditions => new[] {
            new BoolCondition("Hungry", true),
            new BoolCondition("AtTarget", true)
        };
        public override ICondition[] Effects => new[] {
            new BoolCondition("Hungry", false)
        };
    }
}