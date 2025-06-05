using GameLogic.Binding.Paths;
using GameLogic.Binding.Sources;

namespace GameLogic.Binding.Proxy.Sources.Object
{
    public interface INodeProxyFactory
    {
        ISourceProxy Create(object source, PathToken token);
    }
}