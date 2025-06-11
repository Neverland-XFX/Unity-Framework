using System;

namespace GameLogic.Commands
{
    public interface ICommandParameter
    {
        object GetValue();

        Type GetValueType();
    }

    public interface ICommandParameter<T> : ICommandParameter
    {
        new T GetValue();
    }
}