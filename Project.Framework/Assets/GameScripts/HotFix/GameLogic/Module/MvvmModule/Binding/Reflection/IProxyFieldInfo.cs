using System;

namespace GameLogic.Binding.Reflection
{
    public interface IProxyFieldInfo : IProxyMemberInfo
    {
        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target);

        void SetValue(object target, object value);
    }

    public interface IProxyFieldInfo<TValue> : IProxyFieldInfo
    {
        new TValue GetValue(object target);

        void SetValue(object target, TValue value);
    }

    public interface IProxyFieldInfo<T, TValue> : IProxyFieldInfo<TValue>
    {
        TValue GetValue(T target);

        void SetValue(T target, TValue value);
    }
}