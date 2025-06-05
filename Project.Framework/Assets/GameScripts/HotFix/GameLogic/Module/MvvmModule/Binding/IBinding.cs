using System;
using GameLogic.Binding.Contexts;

namespace GameLogic.Binding
{
    public interface IBinding : IDisposable
    {
        IBindingContext BindingContext { get; set; }

        object Target { get; }

        object DataContext { get; set; }
    }
}