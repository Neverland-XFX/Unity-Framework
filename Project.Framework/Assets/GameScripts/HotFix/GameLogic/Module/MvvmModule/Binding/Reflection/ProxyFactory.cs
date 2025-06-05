using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace GameLogic.Binding.Reflection
{
    public class ProxyFactory
    {
        public static readonly ProxyFactory Default = new ProxyFactory();

        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<Type, ProxyType> types = new ConcurrentDictionary<Type, ProxyType>();

        //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER //&& UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnInitialize()
        {
            Default.types.Clear();
        }
#endif
        internal ConcurrentDictionary<Type, ProxyType> Types { get { return types; } }

        internal virtual ProxyType GetType(Type type, bool create = true)
        {
            ProxyType ret;
            if (this.types.TryGetValue(type, out ret) && ret != null)
                return ret;

            return create ? this.types.GetOrAdd(type, (t) => new ProxyType(t, this)) : null;
        }

        public IProxyType Get(Type type)
        {
            return GetType(type, true);
        }

        public void Register(IProxyMemberInfo proxyMemberInfo)
        {
            if (proxyMemberInfo == null)
                return;

            ProxyType proxyType = this.GetType(proxyMemberInfo.DeclaringType);
            proxyType.Register(proxyMemberInfo);
        }

        public void Unregister(IProxyMemberInfo proxyMemberInfo)
        {
            if (proxyMemberInfo == null)
                return;

            ProxyType proxyType = this.GetType(proxyMemberInfo.DeclaringType);
            proxyType.Unregister(proxyMemberInfo);
        }
    }
}