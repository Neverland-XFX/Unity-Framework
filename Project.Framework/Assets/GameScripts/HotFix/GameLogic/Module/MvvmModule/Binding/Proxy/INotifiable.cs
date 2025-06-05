using System;

namespace GameLogic.Binding.Proxy
{
    public interface INotifiable
    {
        event EventHandler ValueChanged;
    }
}