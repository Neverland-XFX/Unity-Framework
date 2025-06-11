using System;
using System.Collections.Generic;

namespace GameLogic.GoapModule
{
    public abstract class PlanRunnerBase : IPlanRunner
    {
        protected IActionExecutionManager ActionManager;
        protected Queue<IActionTemplate> PlanQueue;
        protected IActionInstance CurrentActionInstance;

        public bool IsPlanComplete => PlanQueue == null || PlanQueue.Count == 0;
        public IActionInstance CurrentAction => CurrentActionInstance;
        public int CurrentStepIndex { get; protected set; }

        public event Action OnPlanCompleted;
        public event Action OnPlanInterrupted;

        /// <summary>
        /// 按接口要求显式实现 Initialize
        /// </summary>
        public virtual void Initialize(IActionExecutionManager manager, IEnumerable<IActionTemplate> plan)
        {
            ActionManager = manager ?? throw new ArgumentNullException(nameof(manager));
            PlanQueue = plan != null ? new Queue<IActionTemplate>(plan) : null;
            CurrentActionInstance = null;
            CurrentStepIndex = 0;
        }

        /// <summary>
        /// 开始执行当前计划
        /// </summary>
        public virtual void Start()
        {
            if (PlanQueue == null || PlanQueue.Count == 0)
            {
                CurrentActionInstance = null;
                OnPlanCompleted?.Invoke();
                return;
            }
            CurrentStepIndex = 0;
            StepToNextAction();
        }

        /// <summary>
        /// 每帧推进计划
        /// </summary>
        public virtual void Step()
        {
            // 计划已完成，直接回调
            if (IsPlanComplete)
            {
                OnPlanCompleted?.Invoke();
                return;
            }

            // 当前动作完成，推进到下一个
            if (CurrentActionInstance == null || CurrentActionInstance.State == ActionState.Completed)
            {
                StepToNextAction();
            }
            else
            {
                // 正在执行动作，驱动其更新
                CurrentActionInstance.Update();
            }
        }

        /// <summary>
        /// 推进到计划中的下一个动作
        /// </summary>
        protected virtual void StepToNextAction()
        {
            if (PlanQueue == null || PlanQueue.Count == 0)
            {
                CurrentActionInstance = null;
                OnPlanCompleted?.Invoke();
                return;
            }
            var nextActionTemplate = PlanQueue.Dequeue();
            CurrentActionInstance = ActionManager.GetInstance(nextActionTemplate.ActionId);
            CurrentActionInstance?.Start();
            CurrentStepIndex++;
        }

        /// <summary>
        /// 立即中断当前计划
        /// </summary>
        public virtual void Interrupt()
        {
            PlanQueue?.Clear();
            CurrentActionInstance?.Stop();
            CurrentActionInstance = null;
            OnPlanInterrupted?.Invoke();
        }
    }
}
