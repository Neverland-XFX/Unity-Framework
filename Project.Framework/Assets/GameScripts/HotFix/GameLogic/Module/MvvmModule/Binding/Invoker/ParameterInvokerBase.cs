using System;
using GameLogic.Commands;
using UnityFramework;

namespace GameLogic.Binding
{
    public class ParameterInvokerBase
    {
        protected readonly ICommandParameter commandParameter;
        public ParameterInvokerBase(ICommandParameter commandParameter)
        {
            if (commandParameter == null)
            {
                Log.Error($"commandParameter is null");
                return;
            }

            this.commandParameter = commandParameter;
        }

        protected virtual object GetParameterValue()
        {
            return commandParameter.GetValue();
        }

        protected virtual Type GetParameterValueType()
        {
            return commandParameter.GetValueType();
        }
    }
}