using System;
using GameLogic.Commands;

namespace GameLogic.Binding
{
    public class ParameterWrapInvoker : IInvoker
    {
        protected readonly IInvoker Invoker;
        protected readonly ICommandParameter CommandParameter;
        public ParameterWrapInvoker(IInvoker invoker, ICommandParameter commandParameter)
        {
            this.Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
            this.CommandParameter = commandParameter ?? throw new ArgumentNullException(nameof(commandParameter));
        }

        public object Invoke(params object[] args)
        {
            return this.Invoker.Invoke(CommandParameter.GetValue());
        }
    }

    public class ParameterWrapInvoker<T> : IInvoker
    {
        protected readonly IInvoker<T> Invoker;
        protected readonly ICommandParameter<T> CommandParameter;
        public ParameterWrapInvoker(IInvoker<T> invoker, ICommandParameter<T> commandParameter)
        {
            this.Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
            this.CommandParameter = commandParameter ?? throw new ArgumentNullException(nameof(commandParameter));
        }

        public object Invoke(params object[] args)
        {
            return this.Invoker.Invoke(CommandParameter.GetValue());
        }
    }
}
