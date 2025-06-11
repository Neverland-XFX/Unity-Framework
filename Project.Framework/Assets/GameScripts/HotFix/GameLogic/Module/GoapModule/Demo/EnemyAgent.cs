using Sirenix.Utilities;
using UnityEngine;

namespace GameLogic.GoapModule.Demo
{
    public class EnemyAgent:GoapAgentBase
    {
        public sealed override ITarget Target { get; set; }
        public override Transform Self { get; }

        protected override IGoapWorldState InitWorldState()
        {
            return new DemoWorldState();
        }

        protected override IBlackboard InitBlackboard()
        {
            return new SimpleBlackboard();
        }

        protected override IActionDirectory InitActionDirectory()
        {
            return new ActionDirectory();
        }

        protected override IGoalDirectory InitGoalDirectory()
        {
            return new GoalDirectory();
        }

        protected override IActionExecutionManager InitActionExecutionManager()
        {
            return new ActionExecutionManager(this);
        }

        protected override IGoalExecutionManager InitGoalExecutionManager()
        {
            return new DemoGoalExecutionManager(this);
        }

        protected override IPlanRunner InitPlanRunner()
        {
            return new PlanRunner();
        }

        protected override IStimulusManager InitStimulusManager()
        {
            return StimulusManager;
        }

        public EnemyAgent(Transform self,Transform player) : base(self,player)
        {
            Self = self;
            this.Target = new PositionTarget(player.position);
            // 注册Action和Goal
            ActionDirectory = new ActionDirectory();
            ActionDirectory.RegisterAction("Move", new MoveAction());
            ActionDirectory.RegisterAction("Eat", new EatAction());

            GoalDirectory = new GoalDirectory();
            GoalDirectory.RegisterGoal("SatisfyHunger", new SatisfyHungerGoal(),Target);
            // ...可继续注册其他Goal

            
            // 其它管理器按你base实现初始化...
            // 这里只做演示，完整细节可按你自己的base实现

        }

        public override void Update(GameObject player)
        {
            base.Update(player);
        }
    }
}