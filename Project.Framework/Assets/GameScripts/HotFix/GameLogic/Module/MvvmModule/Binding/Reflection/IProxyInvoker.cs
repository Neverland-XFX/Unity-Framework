namespace GameLogic.Binding.Reflection
{
    public interface IProxyInvoker: IInvoker
    {
        IProxyMethodInfo ProxyMethodInfo { get; }
    }
}