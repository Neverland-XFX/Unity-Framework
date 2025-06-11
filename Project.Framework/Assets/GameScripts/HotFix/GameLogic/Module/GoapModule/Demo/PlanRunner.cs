using System;
using System.Collections.Generic;
using UnityFramework;

namespace GameLogic.GoapModule.Demo
{
    public class PlanRunner : IPlanRunner
    {
        private Queue<IActionTemplate> _planQueue;
        private IActionExecutionManager _execManager;
        private IActionInstance _currentInstance;
        private int _currentStepIndex;

        public event Action OnPlanCompleted;
        public event Action OnPlanInterrupted;

        public IActionInstance CurrentAction => _currentInstance;
        public int CurrentStepIndex => _currentStepIndex;
        public bool IsPlanComplete => _planQueue == null || _planQueue.Count == 0;

        public void Initialize(IActionExecutionManager manager, IEnumerable<IActionTemplate> plan)
        {
            _execManager = manager;
            _planQueue = new Queue<IActionTemplate>(plan);
            _currentStepIndex = 0;
            _currentInstance = null;
        }

        public void Start()
        {
            Step(); // 开始第一个action
        }

        public void Step()
        {
            if (_currentInstance != null)
            {
                // 已有实例未完成，不做处理
                if (_currentInstance.State == ActionState.Running) return;
            }

            if (_planQueue == null || _planQueue.Count == 0)
            {
                OnPlanCompleted?.Invoke();
                return;
            }

            var nextAction = _planQueue.Dequeue();
            var context = nextAction.CreateContext(/* 你的Agent, Target等参数 */ null, target:null); // 可传agent/target/blackboard
            _currentInstance = _execManager.StartAction(nextAction.ActionId,nextAction, context);
            // _currentStepIndex++;

            _currentInstance.Start();
            _currentInstance.OnCompleted += (instance) =>
            {
                Log.Warning($" currentActionId: {_currentInstance.Template.ActionId} is Completed");
                _currentInstance = instance;
                Step(); // 自动执行下一个action
            };
            _currentInstance.OnInterrupted += (instance) =>
            {
                OnPlanInterrupted?.Invoke();
            };
        }

        public void Interrupt()
        {
            if (_currentInstance != null)
                _currentInstance.Interrupt();
            _planQueue.Clear();
            OnPlanInterrupted?.Invoke();
        }
    }
}
