using GameLogic.Binding.Contexts;

namespace GameLogic.Binding
{
    public interface IBindingFactory
    {
        IBinding Create(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription);
    }
}