using System.Linq;
using UnityEngine;

namespace GameLogic.GoapModule.Demo
{
    public class MoveActionInstance : ActionInstanceBase
    {
        private Vector3 _mTargetPosition;
        private Transform _self;
        public MoveActionInstance(IGoapAgent agent, IActionTemplate template, IActionContext context) : base(agent, template, context)
        {
        }

        protected override void OnStart()
        {
            base.OnStart();
            _mTargetPosition = Agent.GoalDirectory.GetAllGoals().First(i => i != null).Target.Position;
            _self = Agent.Self;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            _mTargetPosition = Agent.GoalDirectory.GetAllGoals().First(i => i != null).Target.Position;
            
            if (Vector3.Distance(_self.position, _mTargetPosition) > 0.1f)
            {
                _self.position = Vector3.MoveTowards(
                    _self.position,
                    _mTargetPosition,
                    0.9f * Time.deltaTime
                );
            }
            else
            {
                Complete();
            }
        }
    }
}