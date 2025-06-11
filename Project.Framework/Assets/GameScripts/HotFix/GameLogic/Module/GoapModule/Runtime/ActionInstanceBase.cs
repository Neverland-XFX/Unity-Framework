using System;
using UnityFramework;

namespace GameLogic.GoapModule
{
    public abstract class ActionInstanceBase : IActionInstance
    {
        public IActionTemplate Template { get; private set; }
        public IActionContext Context { get; private set; }
        public ActionState State { get; protected set; }

        public event Action<IActionInstance> OnCompleted;
        public event Action<IActionInstance> OnInterrupted;
        

        protected IGoapAgent Agent;

        public ActionInstanceBase(IGoapAgent agent, IActionTemplate template,
            IActionContext context)
        {
            this.Agent = agent;
            this.Template = template;
            this.Context = context;
            State = ActionState.Inactive;
        }

        public virtual void Start()
        {
            State = ActionState.Entering;
            // 可以在这里设置初始状态、注册回调等
            OnStart();
        }

        public virtual void Update()
        {
            if (State == ActionState.Completed || State == ActionState.Interrupted)
                return;

            State = ActionState.Running;
            OnUpdate();
        }

        public virtual void Stop()
        {
            if (State != ActionState.Completed && State != ActionState.Interrupted)
            {
                State = ActionState.Interrupted;
                OnInterrupted?.Invoke(this);
                OnStop();
            }
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate()
        {
            
        }

        protected virtual void OnStop()
        {
        }
        
        public virtual void Interrupt()
        {
            if (State == ActionState.Completed || State == ActionState.Interrupted)
                return;
            State = ActionState.Interrupted;
            OnInterrupted?.Invoke(this);
            OnInterrupt();
        }

        /// <summary> 派生类可重载：当被外部中断时响应 </summary>
        protected virtual void OnInterrupt() { }
        
        /// <summary>
        /// 通知完成，自动设置状态并派发事件
        /// </summary>
        protected void Complete()
        {
            Log.Warning($"Action is complete,TemplateId:{Template.ActionId}");
            if (State != ActionState.Completed)
            {
                State = ActionState.Completed;
                OnCompleted?.Invoke(this);
            }
        }
    }
}