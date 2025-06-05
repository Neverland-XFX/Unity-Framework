using System;
using System.Threading;
using GameLogic.Binding.Reflection;
using UnityEngine;

namespace GameLogic.Binding.Proxy.Targets.Universal
{
public class MethodTargetProxy : TargetProxyBase, IObtainable, IProxyInvoker
    {
        protected readonly IProxyMethodInfo methodInfo;
        protected SendOrPostCallback postCallback;
        public MethodTargetProxy(object target, IProxyMethodInfo methodInfo) : base(target)
        {
            this.methodInfo = methodInfo;
            if (!methodInfo.ReturnType.Equals(typeof(void)))
                throw new ArgumentException("methodInfo");
        }

        public override BindingMode DefaultMode { get { return BindingMode.OneWayToSource; } }

        public override Type Type { get { return typeof(IProxyInvoker); } }

        public IProxyMethodInfo ProxyMethodInfo { get { return this.methodInfo; } }

        public object GetValue()
        {
            return this;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)this.GetValue();
        }

        public object Invoke(params object[] args)
        {
            if (UISynchronizationContext.InThread)
            {
                if (this.methodInfo.IsStatic)
                {
                    this.methodInfo.Invoke(null, args);
                    return null;
                }

                var target = this.Target;
                if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                    return null;

                return this.methodInfo.Invoke(target, args);
            }
            else
            {
                if (postCallback == null)
                {
                    postCallback = state =>
                    {
                        if (this.methodInfo.IsStatic)
                        {
                            this.methodInfo.Invoke(null, args);
                            return;
                        }

                        var target = this.Target;
                        if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                            return;

                        this.methodInfo.Invoke(target, (object[])state);
                    };
                }
                UISynchronizationContext.Post(postCallback, args);
                return null;
            }
        }
    }
}