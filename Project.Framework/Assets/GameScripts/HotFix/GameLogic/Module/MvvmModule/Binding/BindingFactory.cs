using GameLogic.Binding.Contexts;
using GameLogic.Binding.Proxy.Targets;
using GameLogic.Binding.Sources;

namespace GameLogic.Binding
{
    public class BindingFactory : IBindingFactory
    {
        private ISourceProxyFactory _sourceProxyFactory;
        private ITargetProxyFactory _targetProxyFactory;

        public ISourceProxyFactory SourceProxyFactory
        {
            get => _sourceProxyFactory;
            set => _sourceProxyFactory = value;
        }
        public ITargetProxyFactory TargetProxyFactory
        {
            get => _targetProxyFactory;
            set => _targetProxyFactory = value;
        }

        public BindingFactory(ISourceProxyFactory sourceProxyFactory, ITargetProxyFactory targetProxyFactory)
        {
            this._sourceProxyFactory = sourceProxyFactory;
            this._targetProxyFactory = targetProxyFactory;
        }

        public IBinding Create(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription)
        {
            return new Binding(bindingContext, source, target, bindingDescription, this._sourceProxyFactory, this._targetProxyFactory);
        }
    }
}