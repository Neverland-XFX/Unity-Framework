using System;
using System.Reflection;
using GameLogic.Binding.Paths;
using GameLogic.Binding.Reflection;
using GameLogic.Interactivity;
using GameLogic.Observables;

namespace GameLogic.Binding.Proxy.Targets.Universal
{
    public class FieldTargetProxy : ValueTargetProxyBase
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(FieldTargetProxy));

        protected readonly IProxyFieldInfo fieldInfo;

        public FieldTargetProxy(object target, IProxyFieldInfo fieldInfo) : base(target)
        {
            this.fieldInfo = fieldInfo;
        }

        public override Type Type { get { return this.fieldInfo.ValueType; } }

        public override TypeCode TypeCode { get { return this.fieldInfo.ValueTypeCode; } }

        public override BindingMode DefaultMode { get { return BindingMode.OneWay; } }

        public override object GetValue()
        {
            var target = this.Target;
            if (target == null)
                return null;

            return fieldInfo.GetValue(target);
        }

        public override TValue GetValue<TValue>()
        {
            var target = this.Target;
            if (target == null)
                return default(TValue);

            if (fieldInfo is IProxyFieldInfo<TValue>)
                return ((IProxyFieldInfo<TValue>)fieldInfo).GetValue(target);

            return (TValue)fieldInfo.GetValue(target);
        }

        public override void SetValue(object value)
        {
            var target = this.Target;
            if (target == null)
                return;

            this.fieldInfo.SetValue(target, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = this.Target;
            if (target == null)
                return;

            if (fieldInfo is IProxyFieldInfo<TValue>)
            {
                ((IProxyFieldInfo<TValue>)fieldInfo).SetValue(target, value);
                return;
            }

            this.fieldInfo.SetValue(target, value);
        }
    }
}