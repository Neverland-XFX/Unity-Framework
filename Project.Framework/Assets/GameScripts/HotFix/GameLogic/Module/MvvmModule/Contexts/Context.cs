﻿using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic.Binding.Services;
using UnityEngine;
using UnityFramework;

namespace GameLogic.Contexts
{
public class Context : IDisposable
    {
        private static ApplicationContext context = null;
        private static Dictionary<string, Context> contexts = null;

        
        public static void OnInitialize()
        {
            //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER //&& UNITY_EDITOR
            try
            {
                if (context != null)
                    context.Dispose();

                if (contexts != null)
                {
                    foreach (var context in contexts.Values)
                        context.Dispose();
                    contexts.Clear();
                }
            }
            catch (Exception) { }
#endif

            context = new ApplicationContext();
            contexts = new Dictionary<string, Context>();
        }

        public static ApplicationContext GetApplicationContext()
        {
            return Context.context;
        }

        public static void SetApplicationContext(ApplicationContext context)
        {
            Context.context = context;
        }

        public static Context GetContext(string key)
        {
            Context context = null;
            contexts.TryGetValue(key, out context);
            return context;
        }

        public static T GetContext<T>(string key) where T : Context
        {
            return (T)GetContext(key);
        }

        public static void AddContext(string key, Context context)
        {
            contexts.Add(key, context);
        }

        public static void RemoveContext(string key)
        {
            contexts.Remove(key);
        }

        private bool innerContainer = false;
        private Context contextBase;
        private IServiceContainer container;
        private Dictionary<string, object> attributes;

        public Context() : this(null, null)
        {
        }

        public Context(IServiceContainer container, Context contextBase)
        {
            this.attributes = new Dictionary<string, object>();
            this.contextBase = contextBase;
            this.container = container;
            if (this.container == null)
            {
                this.innerContainer = true;
                this.container = new ServiceContainer();
            }
        }

        public virtual bool Contains(string name, bool cascade = true)
        {
            if (this.attributes.ContainsKey(name))
                return true;

            if (cascade && this.contextBase != null)
                return this.contextBase.Contains(name, cascade);

            return false;
        }

        public virtual object Get(string name, bool cascade = true)
        {
            return this.Get<object>(name, cascade);
        }

        public virtual T Get<T>(string name, bool cascade = true)
        {
            object v;
            if (this.attributes.TryGetValue(name, out v))
                return (T)v;

            if (cascade && this.contextBase != null)
                return this.contextBase.Get<T>(name, cascade);

            return default(T);
        }

        public virtual void Set(string name, object value)
        {
            this.Set<object>(name, value);
        }

        public virtual void Set<T>(string name, T value)
        {
            this.attributes[name] = value;
        }

        public virtual object Remove(string name)
        {
            return this.Remove<object>(name);
        }

        public virtual T Remove<T>(string name)
        {
            if (!this.attributes.ContainsKey(name))
                return default(T);

            object v = this.attributes[name];
            this.attributes.Remove(name);
            return (T)v;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return this.attributes.GetEnumerator();
        }

        public virtual IServiceContainer GetContainer()
        {
            return this.container;
        }

        public virtual object GetService(Type type)
        {
            object result = this.container.Resolve(type);
            if (result != null)
                return result;

            if (this.contextBase != null)
                return this.contextBase.GetService(type);

            return null;
        }

        public virtual object GetService(string name)
        {
            object result = this.container.Resolve(name);
            if (result != null)
                return result;

            if (this.contextBase != null)
                return this.contextBase.GetService(name);

            return null;
        }

        public virtual T GetService<T>()
        {
            T result = this.container.Resolve<T>();
            if (result != null)
                return result;

            if (this.contextBase != null)
                return this.contextBase.GetService<T>();

            return default(T);
        }

        public virtual T GetService<T>(string name)
        {
            T result = this.container.Resolve<T>(name);
            if (result != null)
                return result;

            if (this.contextBase != null)
                return this.contextBase.GetService<T>(name);

            return default(T);
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (this.innerContainer && this.container != null)
                    {
                        IDisposable dis = this.container as IDisposable;
                        if (dis != null)
                            dis.Dispose();
                    }
                }
                disposed = true;
            }
        }

        ~Context()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}