
#if UNITY_2019_1_OR_NEWER
using GameLogic.Binding.Proxy.Targets.Universal;
using GameLogic.Binding.Reflection;
using UnityEngine.UIElements;


namespace GameLogic.Binding.Proxy.Targets.UIElement
{
    public class VisualElementPropertyProxy<TValue> : PropertyTargetProxy
    {
        private readonly INotifyValueChanged<TValue> notifyValueChanged;
        public VisualElementPropertyProxy(object target, IProxyPropertyInfo propertyInfo) : base(target, propertyInfo)
        {
            //不能所有的属性都自动绑定，可能绑定的其他值，与事件不匹配的值
            if (target is INotifyValueChanged<TValue>)
                this.notifyValueChanged = (INotifyValueChanged<TValue>)target;
            else
                notifyValueChanged = null;
        }

        public override BindingMode DefaultMode { get { return BindingMode.TwoWay; } }

        protected override void DoSubscribeForValueChange(object target)
        {
            if (this.notifyValueChanged == null || target == null)
                return;

            notifyValueChanged.RegisterValueChangedCallback(OnValueChanged);
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (notifyValueChanged != null)
                notifyValueChanged.UnregisterValueChangedCallback(OnValueChanged);
        }

        private void OnValueChanged(ChangeEvent<TValue> e)
        {
            this.RaiseValueChanged();
        }
    }
}
#endif