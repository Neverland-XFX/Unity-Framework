using GameLogic.Binding.Proxy.Sources;
using GameLogic.Binding.Sources;
using GameLogic.Observables;

namespace GameLogic.Binding.Proxy.Text
{
    public class LiteralSourceProxyFactory : TypedSourceProxyFactory<LiteralSourceDescription>
    {
        protected override bool TryCreateProxy(object source, LiteralSourceDescription description, out ISourceProxy proxy)
        {
            var value = description.Literal;
            if (value != null && value is IObservableProperty)
                proxy = new ObservableLiteralSourceProxy(value as IObservableProperty);
            else
                proxy = new LiteralSourceProxy(value);
            return true;
        }
    }
}