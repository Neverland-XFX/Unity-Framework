using GameLogic.Binding.Proxy.Sources;

namespace GameLogic.Binding.Sources
{
    public interface ISourceProxyFactory
    {
        ISourceProxy CreateProxy(object source, SourceDescription description);
    }
}