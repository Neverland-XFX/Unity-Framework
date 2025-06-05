using System;
using GameLogic.Binding.Proxy;

namespace GameLogic.Binding.Sources
{
    public interface ISourceProxy : IBindingProxy
    {
        Type Type { get; }

        TypeCode TypeCode { get; }

        object Source { get; }
    }
}