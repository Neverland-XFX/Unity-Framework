﻿using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Binding.Reflection;
using GameLogic.Observables;
using UnityEngine;
using UnityEngine.Events;

namespace GameLogic.Binding.Proxy.Targets.UGUI
{
 public class UnityTargetProxyFactory : ITargetProxyFactory
    {
        [ThreadStatic]
        private static readonly List<Type> TYPES = new List<Type>();
        private static readonly Type[] EMPTY_TYPES = new Type[0];
        public ITargetProxy CreateProxy(object target, BindingDescription description)
        {
            if (TargetNameUtil.IsCollection(description.TargetName))
                return null;

            IProxyType type = description.TargetType != null ? description.TargetType.AsProxy() : target.GetType().AsProxy();
            IProxyMemberInfo memberInfo = type.GetMember(description.TargetName);
            if (memberInfo == null)
                memberInfo = type.GetMember(description.TargetName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (memberInfo == null)
                throw new MissingMemberException(type.Type.FullName, description.TargetName);

            UnityEventBase updateTrigger = null;
            if (!string.IsNullOrEmpty(description.UpdateTrigger))
            {
                IProxyPropertyInfo updateTriggerPropertyInfo = type.GetProperty(description.UpdateTrigger);
                IProxyFieldInfo updateTriggerFieldInfo = updateTriggerPropertyInfo == null ? type.GetField(description.UpdateTrigger) : null;
                if (updateTriggerPropertyInfo != null)
                    updateTrigger = updateTriggerPropertyInfo.GetValue(target) as UnityEventBase;

                if (updateTriggerFieldInfo != null)
                    updateTrigger = updateTriggerFieldInfo.GetValue(target) as UnityEventBase;

                if (updateTriggerPropertyInfo == null && updateTriggerFieldInfo == null)
                    throw new MissingMemberException(type.Type.FullName, description.UpdateTrigger);

                //Other Property Type
                if (updateTrigger == null) /* by UniversalTargetProxyFactory */
                    return null;
            }

            var propertyInfo = memberInfo as IProxyPropertyInfo;
            if (propertyInfo != null)
            {
                if (typeof(IObservableProperty).IsAssignableFrom(propertyInfo.ValueType))
                    return null;

                if (typeof(UnityEventBase).IsAssignableFrom(propertyInfo.ValueType))
                {
                    //Event Type
                    object unityEvent = propertyInfo.GetValue(target);
                    Type[] paramTypes = GetUnityEventParametersType(propertyInfo.ValueType);
                    return CreateUnityEventProxy(target, (UnityEventBase)unityEvent, paramTypes);
                }

                //Other Property Type
                if (updateTrigger == null)/* by UniversalTargetProxyFactory */
                    return null;

                return CreateUnityPropertyProxy(target, propertyInfo, updateTrigger);
            }

            var fieldInfo = memberInfo as IProxyFieldInfo;
            if (fieldInfo != null)
            {
                if (typeof(IObservableProperty).IsAssignableFrom(fieldInfo.ValueType))
                    return null;

                if (typeof(UnityEventBase).IsAssignableFrom(fieldInfo.ValueType))
                {
                    //Event Type
                    object unityEvent = fieldInfo.GetValue(target);
                    Type[] paramTypes = GetUnityEventParametersType(fieldInfo.ValueType);
                    return CreateUnityEventProxy(target, (UnityEventBase)unityEvent, paramTypes);
                }

                //Other Property Type
                if (updateTrigger == null)/* by UniversalTargetProxyFactory */
                    return null;

                return CreateUnityFieldProxy(target, fieldInfo, updateTrigger);
            }

            return null;
        }

        protected virtual ITargetProxy CreateUnityPropertyProxy(object target, IProxyPropertyInfo propertyInfo, UnityEventBase updateTrigger)
        {
            TypeCode typeCode = propertyInfo.ValueTypeCode;
            switch (typeCode)
            {
                case TypeCode.String: return new UnityPropertyProxy<string>(target, propertyInfo, (UnityEvent<string>)updateTrigger);
                case TypeCode.Boolean: return new UnityPropertyProxy<bool>(target, propertyInfo, (UnityEvent<bool>)updateTrigger);
                case TypeCode.SByte: return new UnityPropertyProxy<sbyte>(target, propertyInfo, (UnityEvent<sbyte>)updateTrigger);
                case TypeCode.Byte: return new UnityPropertyProxy<byte>(target, propertyInfo, (UnityEvent<byte>)updateTrigger);
                case TypeCode.Int16: return new UnityPropertyProxy<short>(target, propertyInfo, (UnityEvent<short>)updateTrigger);
                case TypeCode.UInt16: return new UnityPropertyProxy<ushort>(target, propertyInfo, (UnityEvent<ushort>)updateTrigger);
                case TypeCode.Int32: return new UnityPropertyProxy<int>(target, propertyInfo, (UnityEvent<int>)updateTrigger);
                case TypeCode.UInt32: return new UnityPropertyProxy<uint>(target, propertyInfo, (UnityEvent<uint>)updateTrigger);
                case TypeCode.Int64: return new UnityPropertyProxy<long>(target, propertyInfo, (UnityEvent<long>)updateTrigger);
                case TypeCode.UInt64: return new UnityPropertyProxy<ulong>(target, propertyInfo, (UnityEvent<ulong>)updateTrigger);
                case TypeCode.Char: return new UnityPropertyProxy<char>(target, propertyInfo, (UnityEvent<char>)updateTrigger);
                case TypeCode.Single: return new UnityPropertyProxy<float>(target, propertyInfo, (UnityEvent<float>)updateTrigger);
                case TypeCode.Double: return new UnityPropertyProxy<double>(target, propertyInfo, (UnityEvent<double>)updateTrigger);
                case TypeCode.Decimal: return new UnityPropertyProxy<decimal>(target, propertyInfo, (UnityEvent<decimal>)updateTrigger);
                case TypeCode.DateTime: return new UnityPropertyProxy<DateTime>(target, propertyInfo, (UnityEvent<DateTime>)updateTrigger);
                default:
                    {
                        Type valueType = propertyInfo.ValueType;
                        if (valueType.Equals(typeof(Vector2)))
                            return new UnityPropertyProxy<Vector2>(target, propertyInfo, (UnityEvent<Vector2>)updateTrigger);
                        else if (valueType.Equals(typeof(Vector3)))
                            return new UnityPropertyProxy<Vector3>(target, propertyInfo, (UnityEvent<Vector3>)updateTrigger);
                        else if (valueType.Equals(typeof(Vector4)))
                            return new UnityPropertyProxy<Vector4>(target, propertyInfo, (UnityEvent<Vector4>)updateTrigger);
                        else
                            return (ITargetProxy)Activator.CreateInstance(typeof(UnityPropertyProxy<>).MakeGenericType(valueType), target, propertyInfo, updateTrigger);//JIT Exception
                    }
            }
        }

        protected virtual ITargetProxy CreateUnityFieldProxy(object target, IProxyFieldInfo fieldInfo, UnityEventBase updateTrigger)
        {
            TypeCode typeCode = fieldInfo.ValueTypeCode;
            switch (typeCode)
            {
                case TypeCode.String: return new UnityFieldProxy<string>(target, fieldInfo, (UnityEvent<string>)updateTrigger);
                case TypeCode.Boolean: return new UnityFieldProxy<bool>(target, fieldInfo, (UnityEvent<bool>)updateTrigger);
                case TypeCode.SByte: return new UnityFieldProxy<sbyte>(target, fieldInfo, (UnityEvent<sbyte>)updateTrigger);
                case TypeCode.Byte: return new UnityFieldProxy<byte>(target, fieldInfo, (UnityEvent<byte>)updateTrigger);
                case TypeCode.Int16: return new UnityFieldProxy<short>(target, fieldInfo, (UnityEvent<short>)updateTrigger);
                case TypeCode.UInt16: return new UnityFieldProxy<ushort>(target, fieldInfo, (UnityEvent<ushort>)updateTrigger);
                case TypeCode.Int32: return new UnityFieldProxy<int>(target, fieldInfo, (UnityEvent<int>)updateTrigger);
                case TypeCode.UInt32: return new UnityFieldProxy<uint>(target, fieldInfo, (UnityEvent<uint>)updateTrigger);
                case TypeCode.Int64: return new UnityFieldProxy<long>(target, fieldInfo, (UnityEvent<long>)updateTrigger);
                case TypeCode.UInt64: return new UnityFieldProxy<ulong>(target, fieldInfo, (UnityEvent<ulong>)updateTrigger);
                case TypeCode.Char: return new UnityFieldProxy<char>(target, fieldInfo, (UnityEvent<char>)updateTrigger);
                case TypeCode.Single: return new UnityFieldProxy<float>(target, fieldInfo, (UnityEvent<float>)updateTrigger);
                case TypeCode.Double: return new UnityFieldProxy<double>(target, fieldInfo, (UnityEvent<double>)updateTrigger);
                case TypeCode.Decimal: return new UnityFieldProxy<decimal>(target, fieldInfo, (UnityEvent<decimal>)updateTrigger);
                case TypeCode.DateTime: return new UnityFieldProxy<DateTime>(target, fieldInfo, (UnityEvent<DateTime>)updateTrigger);
                default:
                    {
                        Type valueType = fieldInfo.ValueType;
                        if (valueType.Equals(typeof(Vector2)))
                            return new UnityFieldProxy<Vector2>(target, fieldInfo, (UnityEvent<Vector2>)updateTrigger);
                        else if (valueType.Equals(typeof(Vector3)))
                            return new UnityFieldProxy<Vector3>(target, fieldInfo, (UnityEvent<Vector3>)updateTrigger);
                        else if (valueType.Equals(typeof(Vector4)))
                            return new UnityFieldProxy<Vector4>(target, fieldInfo, (UnityEvent<Vector4>)updateTrigger);
                        else
                            return (ITargetProxy)Activator.CreateInstance(typeof(UnityFieldProxy<>).MakeGenericType(valueType), target, fieldInfo, updateTrigger);//JIT Exception
                    }
            }
        }

        protected virtual ITargetProxy CreateUnityEventProxy(object target, UnityEventBase unityEvent, Type[] paramTypes)
        {
            switch (paramTypes.Length)
            {
                case 0:
                    return new UnityEventProxy(target, (UnityEvent)unityEvent);
                case 1:
#if NETFX_CORE
                    TypeCode typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(paramTypes[0]);
#else
                    TypeCode typeCode = Type.GetTypeCode(paramTypes[0]);
#endif
                    switch (typeCode)
                    {
                        case TypeCode.String: return new UnityEventProxy<string>(target, (UnityEvent<string>)unityEvent);
                        case TypeCode.Boolean: return new UnityEventProxy<bool>(target, (UnityEvent<bool>)unityEvent);
                        case TypeCode.SByte: return new UnityEventProxy<sbyte>(target, (UnityEvent<sbyte>)unityEvent);
                        case TypeCode.Byte: return new UnityEventProxy<byte>(target, (UnityEvent<byte>)unityEvent);
                        case TypeCode.Int16: return new UnityEventProxy<short>(target, (UnityEvent<short>)unityEvent);
                        case TypeCode.UInt16: return new UnityEventProxy<ushort>(target, (UnityEvent<ushort>)unityEvent);
                        case TypeCode.Int32: return new UnityEventProxy<int>(target, (UnityEvent<int>)unityEvent);
                        case TypeCode.UInt32: return new UnityEventProxy<uint>(target, (UnityEvent<uint>)unityEvent);
                        case TypeCode.Int64: return new UnityEventProxy<long>(target, (UnityEvent<long>)unityEvent);
                        case TypeCode.UInt64: return new UnityEventProxy<ulong>(target, (UnityEvent<ulong>)unityEvent);
                        case TypeCode.Char: return new UnityEventProxy<char>(target, (UnityEvent<char>)unityEvent);
                        case TypeCode.Single: return new UnityEventProxy<float>(target, (UnityEvent<float>)unityEvent);
                        case TypeCode.Double: return new UnityEventProxy<double>(target, (UnityEvent<double>)unityEvent);
                        case TypeCode.Decimal: return new UnityEventProxy<decimal>(target, (UnityEvent<decimal>)unityEvent);
                        case TypeCode.DateTime: return new UnityEventProxy<DateTime>(target, (UnityEvent<DateTime>)unityEvent);
                        default:
                            {
                                Type valueType = paramTypes[0];
                                if (valueType.Equals(typeof(Vector2)))
                                    return new UnityEventProxy<Vector2>(target, (UnityEvent<Vector2>)unityEvent);
                                else if (valueType.Equals(typeof(Vector3)))
                                    return new UnityEventProxy<Vector3>(target, (UnityEvent<Vector3>)unityEvent);
                                else if (valueType.Equals(typeof(Vector4)))
                                    return new UnityEventProxy<Vector4>(target, (UnityEvent<Vector4>)unityEvent);
                                else
                                    return (ITargetProxy)Activator.CreateInstance(typeof(UnityEventProxy<>).MakeGenericType(valueType), target, unityEvent);//JIT Exception
                            }
                    }
                case 2:
                    return (ITargetProxy)Activator.CreateInstance(typeof(UnityEventProxy<,>).MakeGenericType(paramTypes), target, unityEvent);//If creating an exception, define a static type:static Type t = tyeof(UnityEventProxy<P1,P2>)
                case 3:
                    return (ITargetProxy)Activator.CreateInstance(typeof(UnityEventProxy<,,>).MakeGenericType(paramTypes), target, unityEvent);
                case 4:
                    return (ITargetProxy)Activator.CreateInstance(typeof(UnityEventProxy<,,,>).MakeGenericType(paramTypes), target, unityEvent);
                default:
                    throw new NotSupportedException("Too many parameters");
            }
        }

        protected Type[] GetUnityEventParametersType(Type type)
        {
            MethodInfo info = type.GetMethod("Invoke");
            if (info == null)
                throw new MemberAccessException($"{type.Name}.Invoke() method has been stripped, please declare to preserve this method in the link.xml file");

            ParameterInfo[] parameters = info.GetParameters();
            if (parameters == null || parameters.Length <= 0)
                return EMPTY_TYPES;

            TYPES.Clear();
            foreach (ParameterInfo parameter in parameters)
            {
                TYPES.Add(parameter.ParameterType);
            }

            return TYPES.ToArray();
        }
    }
}