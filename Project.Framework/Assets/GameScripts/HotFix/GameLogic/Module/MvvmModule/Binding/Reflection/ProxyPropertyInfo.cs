﻿using System;
using System.Reflection;
using UnityFramework;

namespace GameLogic.Binding.Reflection
{
public class ProxyPropertyInfo : IProxyPropertyInfo
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ProxyPropertyInfo));

        private readonly bool isValueType;
        private TypeCode typeCode;
        protected PropertyInfo propertyInfo;
        protected MethodInfo getMethod;
        protected MethodInfo setMethod;

        public ProxyPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            this.propertyInfo = propertyInfo;
            //this.isValueType = this.propertyInfo.DeclaringType.GetTypeInfo().IsValueType;
            this.isValueType = this.propertyInfo.DeclaringType.IsValueType;

            if (this.propertyInfo.CanRead)
                this.getMethod = propertyInfo.GetGetMethod();

            if (this.propertyInfo.CanWrite && !this.isValueType)
                this.setMethod = propertyInfo.GetSetMethod();
        }

        public virtual bool IsValueType { get { return isValueType; } }

        public virtual Type ValueType { get { return this.propertyInfo.PropertyType; } }

        public TypeCode ValueTypeCode
        {
            get
            {
                if (typeCode == TypeCode.Empty)
                {
#if NETFX_CORE
                    typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(ValueType);
#else
                    typeCode = Type.GetTypeCode(ValueType);
#endif
                }
                return typeCode;
            }
        }

        public virtual Type DeclaringType { get { return this.propertyInfo.DeclaringType; } }

        public virtual string Name { get { return this.propertyInfo.Name; } }

        public virtual bool IsStatic { get { return this.propertyInfo.IsStatic(); } }

        public virtual object GetValue(object target)
        {
            if (this.getMethod == null)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is not public");

            return this.getMethod.Invoke(target, null);
        }

        public virtual void SetValue(object target, object value)
        {
            if (!propertyInfo.CanWrite)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is read-only.");

            if (this.IsValueType)
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (this.setMethod == null)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is not public");

            this.setMethod.Invoke(target, new object[] { value });
        }
    }

    public class ProxyPropertyInfo<T, TValue> : ProxyPropertyInfo, IProxyPropertyInfo<T, TValue>
    {
        private Func<T, TValue> getter;
        private Action<T, TValue> setter;

        public ProxyPropertyInfo(string propertyName) : this(typeof(T).GetProperty(propertyName))
        {
        }

        public ProxyPropertyInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            if (!typeof(TValue).Equals(this.propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The property types do not match!");

            if (this.IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is static.");

            this.getter = this.MakeGetter(propertyInfo);
            this.setter = this.MakeSetter(propertyInfo);
        }

        public ProxyPropertyInfo(string propertyName, Func<T, TValue> getter, Action<T, TValue> setter) : this(typeof(T).GetProperty(propertyName), getter, setter)
        {
        }

        public ProxyPropertyInfo(PropertyInfo propertyInfo, Func<T, TValue> getter, Action<T, TValue> setter) : base(propertyInfo)
        {
            if (!typeof(TValue).Equals(this.propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The property types do not match!");

            if (this.IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is static.");

            this.getter = getter;
            this.setter = setter;
        }

        public override Type DeclaringType { get { return typeof(T); } }

        private Action<T, TValue> MakeSetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (this.IsValueType)
                    return null;

                var setMethod = propertyInfo.GetSetMethod();
                if (setMethod == null)
                    return null;

                return (Action<T, TValue>)setMethod.CreateDelegate(typeof(Action<T, TValue>));
            }
            catch (Exception e)
            {
                Log.Error("{0}", e);
            }

            return null;
        }

        private Func<T, TValue> MakeGetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (this.IsValueType)
                    return null;

                var getMethod = propertyInfo.GetGetMethod();
                if (getMethod == null)
                    return null;

                return (Func<T, TValue>)getMethod.CreateDelegate(typeof(Func<T, TValue>));
            }
            catch (Exception e)
            {
                Log.Error("{0}", e);
            }
            return null;
        }

        public TValue GetValue(T target)
        {
            if (this.getter != null)
                return this.getter(target);

            return (TValue)base.GetValue(target);
        }

        TValue IProxyPropertyInfo<TValue>.GetValue(object target)
        {
            return this.GetValue((T)target);
        }

        public override object GetValue(object target)
        {
            if (this.getter != null)
                return this.getter((T)target);

            return base.GetValue(target);
        }

        public void SetValue(T target, TValue value)
        {
            if (this.IsValueType)
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter(target, value);
                return;
            }

            base.SetValue(target, value);
        }

        public void SetValue(object target, TValue value)
        {
            this.SetValue((T)target, value);
        }

        public override void SetValue(object target, object value)
        {
            if (this.IsValueType)
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter((T)target, (TValue)value);
                return;
            }

            base.SetValue(target, value);
        }

    }
}