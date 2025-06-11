using System;
using GameLogic.Commands;

namespace GameLogic.Binding
{
    public class ConstantCommandParameter : ICommandParameter
    {
        private object parameter;
        public ConstantCommandParameter(object parameter)
        {
            this.parameter = parameter;
        }
        public object GetValue()
        {
            return parameter;
        }

        public Type GetValueType()
        {
            return parameter != null ? parameter.GetType() : typeof(object);
        }
    }

    public class ConstantCommandParameter<T> : ICommandParameter<T>
    {
        private T parameter;
        public ConstantCommandParameter(T parameter)
        {
            this.parameter = parameter;
        }
        public T GetValue()
        {
            return parameter;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        object ICommandParameter.GetValue()
        {
            return GetValue();
        }
    }
}
