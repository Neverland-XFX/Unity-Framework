using System.Collections.Generic;
using System.Linq;
using UnityFramework;

namespace GameLogic.GoapModule
{
    public abstract class ActionExecutionManagerBase : IActionExecutionManager
    {
        protected readonly Dictionary<string, IActionInstance> _instances = new();
        protected readonly IGoapAgent _agent;

        public ActionExecutionManagerBase(IGoapAgent agent)
        {
            _agent = agent;
            // InitializeActions();
        }

        /// <summary>
        /// 由子类实现，注册所有可用行为实例
        /// </summary>
        protected abstract void InitializeActions();

        public virtual IActionInstance GetInstance(string actionId)
        {
            _instances.TryGetValue(actionId, out var instance);
            return instance;
        }

        public virtual IEnumerable<IActionInstance> GetAllInstances()
        {
            return _instances.Values;
        }

        /// <summary>
        /// 所有行为实例的统一Update（通常每帧调用一次）
        /// </summary>
        public virtual void Update()
        {
            for (int i = 0; i < _instances.Values.Count; i++)
            {
                _instances.Values.ElementAt(i)?.Update();
            }
        }

        public IActionInstance StartAction(string actionId,IActionTemplate template, IActionContext context)
        {
            var instance = CreateInstance(template, context);
            _instances.Add(actionId,instance);

            // 自动监听完成/中断移除
            instance.OnCompleted += (_) => _instances.Remove(actionId);
            instance.OnInterrupted += (_) => _instances.Remove(actionId);

            (instance as ActionInstanceBase)?.Start();
            return instance;
        }
        /// <summary>
        /// 派生类负责生成具体的Action实例
        /// </summary>
        protected abstract IActionInstance CreateInstance(IActionTemplate template, IActionContext context);
        
        
        public void StopAction(string actionId)
        {
            _instances.TryGetValue(actionId, out var value);
            if (value != null)
            {
                value.Interrupt();
                _instances.Remove(actionId);   
            }
            else
            {
                Log.Error($"Action {actionId} not found");
            }
        }

    }
}