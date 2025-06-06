﻿namespace GameLogic.Binding.Proxy.Targets
{
    public interface ITargetProxyFactory
    {
        ITargetProxy CreateProxy(object target, BindingDescription description);
    }
}