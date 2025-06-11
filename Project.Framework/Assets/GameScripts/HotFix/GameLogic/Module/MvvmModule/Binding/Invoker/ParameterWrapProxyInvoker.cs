using System;
using GameLogic.Binding.Reflection;
using GameLogic.Commands;

namespace GameLogic.Binding
{
    public class ParameterProxyInvoker : ParameterInvokerBase, IInvoker
    {
        private readonly IProxyInvoker invoker;

        public ParameterProxyInvoker(IProxyInvoker invoker, ICommandParameter commandParameter) : base(commandParameter)
        {
            if (invoker == null)
                throw new ArgumentNullException("invoker");

            this.invoker = invoker;
            if (!IsValid(invoker))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return this.invoker.Invoke(GetParameterValue());
        }

        protected bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(GetParameterValueType());
        }
    }
}
