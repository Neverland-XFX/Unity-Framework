using System;
using GameLogic.Binding.Reflection;
using GameLogic.Commands;
using UnityFramework;

namespace GameLogic.Binding.Converters
{
    public class ParameterConverter: AbstractConverter
    {
        
        private readonly ICommandParameter commandParameter;
        public ParameterConverter(ICommandParameter commandParameter)
        {
            if (commandParameter == null)
            {
                Log.Error($"commandParameter is null");
                return;
            }

            this.commandParameter = commandParameter;
        }

        public override object Convert(object value)
        {
            return value switch
            {
                null => null,
                Delegate @delegate => new ParameterDelegateInvoker(@delegate, commandParameter),
                ICommand command => new ParameterCommand(command, commandParameter),
                // IScriptInvoker scriptInvoker => new ParameterScriptInvoker(scriptInvoker,commandParameter),
                IProxyInvoker invoker => new ParameterProxyInvoker(invoker, commandParameter),
                IInvoker value1 => new ParameterWrapInvoker(value1, commandParameter),
                _ => throw new NotSupportedException(string.Format("Unsupported type \"{0}\".", value.GetType()))
            };
        }

        public override object ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }

    public class ParameterConverter<T> : AbstractConverter
    {
        private readonly ICommandParameter<T> commandParameter;
        public ParameterConverter(ICommandParameter<T> commandParameter)
        {
            if (commandParameter == null)
                throw new ArgumentNullException("commandParameter");

            this.commandParameter = commandParameter;
        }

        public override object Convert(object value)
        {
            if (value == null)
                return null;

            if (value is IInvoker<T> invoker)
                return new ParameterWrapInvoker<T>(invoker, commandParameter);

            if (value is ICommand<T> command)
                return new ParameterCommand<T>(command, commandParameter);

            if (value is Action<T> action)
                return new ParameterActionInvoker<T>(action, commandParameter);

            if (value is Delegate)
                return new ParameterDelegateInvoker(value as Delegate, commandParameter);

            if (value is ICommand)
                return new ParameterCommand(value as ICommand, commandParameter);

            // if (value is IScriptInvoker)
            //     return new ParameterScriptInvoker(value as IScriptInvoker, commandParameter);

            if (value is IProxyInvoker)
                return new ParameterProxyInvoker(value as IProxyInvoker, commandParameter);

            if (value is IInvoker)
                return new ParameterWrapInvoker(value as IInvoker, commandParameter);

            throw new NotSupportedException(string.Format("Unsupported type \"{0}\".", value.GetType()));
        }

        public override object ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }
}
