using System;

namespace GameLogic.Binding.Reflection
{
    public interface IProxyPropertyInfo : IProxyMemberInfo
    {
        bool IsValueType { get; }

        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target);

        void SetValue(object target, object value);
    }

    public interface IProxyPropertyInfo<TValue> : IProxyPropertyInfo
    {
        new TValue GetValue(object target);

        void SetValue(object target, TValue value);
    }

    public interface IProxyPropertyInfo<T, TValue> : IProxyPropertyInfo<TValue>
    {
        TValue GetValue(T target);

        void SetValue(T target, TValue value);
    }
}