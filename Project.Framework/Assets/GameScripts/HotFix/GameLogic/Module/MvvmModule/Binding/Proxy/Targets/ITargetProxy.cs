using System;

namespace GameLogic.Binding.Proxy.Targets
{
    public interface ITargetProxy : IBindingProxy
    {
        Type Type { get; }

        TypeCode TypeCode { get; }

        object Target { get; }

        BindingMode DefaultMode { get; }
    }
}