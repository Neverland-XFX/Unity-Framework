using System;

namespace GameLogic.GoapModule.Demo
{
    public class ActionExecutionManager : ActionExecutionManagerBase
    {
        public ActionExecutionManager(IGoapAgent agent) : base(agent)
        {
            
        }

        protected override void InitializeActions()
        {
            // _instances.Add("Eat",new EatActionInstance(_agent,new EatAction(),new ActionContext()));
        }

        protected override IActionInstance CreateInstance(IActionTemplate template, IActionContext context)
        {
            if (template.ActionId == "Move")
                return new MoveActionInstance(_agent,template, context); 
            if (template.ActionId == "Eat")
                return new EatActionInstance(_agent,template, context); 
            // ... 其它Action类型
            throw new NotImplementedException("Unknown action type");
        }
    }
}