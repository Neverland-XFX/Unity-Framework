using System;
using UnityEngine;
using UnityFramework;

namespace GameLogic.GoapModule
{
    public abstract class GoapAgentBase : IGoapAgent
    {
        public abstract ITarget Target { get; set; }
        public virtual Transform Self { get; }
        public virtual bool IsTerminated { get; protected set; }
        public IGoapWorldState WorldState { get; protected set; }
        public IActionDirectory ActionDirectory { get; protected set; }
        public IGoalDirectory GoalDirectory { get; protected set; }
        public IActionExecutionManager ActionExecutionManager { get; protected set; }
        public IGoalExecutionManager GoalExecutionManager { get; protected set; }
        public IPlanRunner PlanRunner { get; protected set; }
        public IStimulusManager StimulusManager { get; protected set; }
        public IBlackboard Blackboard { get; protected set; }

        /// <summary>
        /// 构造函数：子类在构造时依次实现Init方法
        /// </summary>
        protected GoapAgentBase(Transform self, Transform player)
        {
            Self = self;
            WorldState = InitWorldState();
            Blackboard = InitBlackboard();
            ActionDirectory = InitActionDirectory();
            GoalDirectory = InitGoalDirectory();
            ActionExecutionManager = InitActionExecutionManager();
            GoalExecutionManager = InitGoalExecutionManager();
            PlanRunner = InitPlanRunner();
            StimulusManager = InitStimulusManager();

            // 事件链/变更监听可在这里注册
            if (WorldState is GoapWorldStateBase ws)
                ws.OnStateChanged += OnWorldStateChanged;
        }

        /// <summary>
        /// 子类实现以下组件的初始化
        /// </summary>
        protected abstract IGoapWorldState InitWorldState();

        protected abstract IBlackboard InitBlackboard();
        protected abstract IActionDirectory InitActionDirectory();
        protected abstract IGoalDirectory InitGoalDirectory();
        protected abstract IActionExecutionManager InitActionExecutionManager();
        protected abstract IGoalExecutionManager InitGoalExecutionManager();
        protected abstract IPlanRunner InitPlanRunner();
        protected abstract IStimulusManager InitStimulusManager();

        /// <summary>
        /// 状态变更时自动更新相关数据
        /// </summary>
        protected virtual void OnWorldStateChanged(string key, object value)
        {
            UpdateData();
        }

        /// <summary>a
        /// 主动拉取/刷新所有Agent子系统
        /// </summary>
        public virtual void UpdateData()
        {
            if (IsTerminated) return;
            GoalExecutionManager?.Update();
            ActionExecutionManager?.Update();
            StimulusManager?.Update();
            Blackboard?.Set("__tick__", DateTime.Now); // 示例：记录时间
        }

        /// <summary>
        /// 每帧或周期性调用的主调度方法
        /// </summary>
        /// <param name="player"></param>
        public virtual void Update(GameObject player)
        {
            if (IsTerminated) return;
            Log.Info($"current position is equal :{player.transform.position == Target.Position}");
            Target.SetPosition(player.transform.position);
            StimulusManager?.Update();
            // PlanRunner?.Step();
            ActionExecutionManager?.Update();
        }
    }
}