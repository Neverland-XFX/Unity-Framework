using System.Collections.Generic;
using GameLogic.Binding.Contexts;

namespace GameLogic.Binding.Binders
{
    public interface IBinder
    {
        IBinding Bind(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription);

        IEnumerable<IBinding> Bind(IBindingContext bindingContext, object source, object target, IEnumerable<BindingDescription> bindingDescriptions);

    }
}