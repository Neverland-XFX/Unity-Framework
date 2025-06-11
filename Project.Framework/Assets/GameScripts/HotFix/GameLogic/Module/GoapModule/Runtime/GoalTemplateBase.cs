using System;
using UnityEngine;

namespace GameLogic.GoapModule
{
    public abstract class GoalTemplateBase : IGoalTemplate
    {
        public abstract ITarget Target { get; set; }
        public abstract string GoalId { get; }
        public abstract float Priority { get; }
        public abstract ICondition[] RequiredStates { get; }
        public abstract ICondition[] SatisfiedStates { get; }

        public event Action<IGoalTemplate> OnActivated;
        public event Action<IGoalTemplate> OnDeactivated;

        protected IGoapAgent Agent { get; private set; }

        /// <summary>
        /// 构造传入Agent，便于获取环境/黑板等
        /// </summary>
        public GoalTemplateBase(IGoapAgent agent)
        {
            Agent = agent;
        }

        /// <summary>
        /// 目标是否达成（可重写以自定义逻辑）
        /// </summary>
        public virtual bool IsAchieved(IGoapAgent agent)
        {
            // 默认为目标所有效果在Agent当前世界状态中全部满足
            foreach (var cond in SatisfiedStates)
                if (!cond.IsMet(agent, agent.WorldState))
                    return false;
            return true;
        }

        /// <summary>
        /// 供外部调用，触发激活事件
        /// </summary>
        protected void RaiseActivated() => OnActivated?.Invoke(this);

        /// <summary>
        /// 供外部调用，触发失活事件
        /// </summary>
        protected void RaiseDeactivated() => OnDeactivated?.Invoke(this);

        /// <summary>
        /// 可定期调用，用于判定目标的激活与失活
        /// </summary>
        public virtual void UpdateGoalData(IGoapAgent agent)
        {
            bool isActive = true;
            foreach (var cond in RequiredStates)
                if (!cond.IsMet(agent, agent.WorldState))
                {
                    isActive = false;
                    break;
                }
            if (isActive)
                RaiseActivated();
            else
                RaiseDeactivated();
        }

        public virtual void UpdateTarget(ITarget agent)
        {
            this.Target = agent;
        }

        public virtual void UpdateTargetPosition(Vector3 position)
        {
            Target.SetPosition(position);
        }
    }
}