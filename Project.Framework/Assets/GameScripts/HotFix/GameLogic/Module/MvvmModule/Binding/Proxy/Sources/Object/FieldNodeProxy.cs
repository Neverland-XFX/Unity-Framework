using System;
using GameLogic.Binding.Reflection;
using GameLogic.Binding.Sources;

namespace GameLogic.Binding.Proxy.Sources.Object
{
    public class FieldNodeProxy : SourceProxyBase, IObtainable, IModifiable
    {
        protected IProxyFieldInfo fieldInfo;

        public FieldNodeProxy(IProxyFieldInfo fieldInfo) : this(null, fieldInfo)
        {
        }

        public FieldNodeProxy(object source, IProxyFieldInfo fieldInfo) : base(source)
        {
            this.fieldInfo = fieldInfo;
        }

        public override Type Type { get { return fieldInfo.ValueType; } }

        public override TypeCode TypeCode { get { return fieldInfo.ValueTypeCode; } }

        public virtual object GetValue()
        {
            return fieldInfo.GetValue(source);
        }

        public virtual TValue GetValue<TValue>()
        {
            var proxy = fieldInfo as IProxyFieldInfo<TValue>;
            if (proxy != null)
                return proxy.GetValue(source);

            return (TValue)this.fieldInfo.GetValue(source);
        }

        public virtual void SetValue(object value)
        {
            fieldInfo.SetValue(source, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            var proxy = fieldInfo as IProxyFieldInfo<TValue>;
            if (proxy != null)
            {
                proxy.SetValue(source, value);
                return;
            }

            this.fieldInfo.SetValue(source, value);
        }
    }
}