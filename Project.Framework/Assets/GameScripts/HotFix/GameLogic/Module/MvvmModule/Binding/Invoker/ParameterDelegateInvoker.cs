using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Binding.Reflection;
using GameLogic.Commands;
using UnityFramework;

namespace GameLogic.Binding
{
    public class ParameterDelegateInvoker : ParameterInvokerBase, IInvoker
    {
        private readonly Delegate handler;

        public ParameterDelegateInvoker(Delegate handler, ICommandParameter commandParameter) : base(commandParameter)
        {
            if (handler == null)
            {
                Log.Error($"handler is null");
                return;
            }

            this.handler = handler;
            if (!IsValid(handler))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return this.handler.DynamicInvoke(GetParameterValue());
        }

        protected virtual bool IsValid(Delegate handler)
        {
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 1)
                return false;

            return parameterTypes[0].IsAssignableFrom(GetParameterValueType());
        }
    }

    public class ParameterActionInvoker<T> : IInvoker
    {
        private readonly Action<T> handler;
        private readonly ICommandParameter<T> commandParameter;
        public ParameterActionInvoker(Action<T> handler, ICommandParameter<T> commandParameter)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (commandParameter == null)
                throw new ArgumentNullException("commandParameter");

            this.commandParameter = commandParameter;
            this.handler = handler;
        }

        public object Invoke(params object[] args)
        {
            this.handler(commandParameter.GetValue());
            return null;
        }
    }
}
