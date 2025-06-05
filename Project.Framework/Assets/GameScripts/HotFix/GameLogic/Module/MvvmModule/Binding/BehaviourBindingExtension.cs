using System;
using System.Collections.Generic;
using GameLogic;
using GameLogic.Binding;
using GameLogic.Binding.Binders;
using GameLogic.Binding.Builder;
using GameLogic.Binding.Contexts;
using GameLogic.Contexts;
using UnityEngine;
using UnityFramework;

namespace GameLogic
{
public static class UIWindowBindingExtension
    {
        private static IBinder binder;
        public static IBinder Binder
        {
            get
            {
                if (binder == null)
                    binder = Context.GetApplicationContext().GetService<IBinder>();

                if (binder == null)
                    throw new Exception("Data binding service is not initialized,please create a BindingServiceBundle service before using it.");

                return binder;
            }
        }

        public static IBindingContext BindingContext(this UIWindow uiWindow)
        {
            if (uiWindow == null || uiWindow.gameObject == null)
                return null;

            if (uiWindow.gameObject.GetComponent<BindingContextLifecycle>() == null)
            {
                uiWindow.gameObject.AddComponent<BindingContextLifecycle>();
            }
            BindingContextLifecycle bindingContextLifecycle = uiWindow.transform.GetComponent<BindingContextLifecycle>();
            if (bindingContextLifecycle == null)
                bindingContextLifecycle = uiWindow.gameObject.AddComponent<BindingContextLifecycle>();

            IBindingContext bindingContext = bindingContextLifecycle.BindingContext;
            if (bindingContext == null)
            {
                bindingContext = new BindingContext(uiWindow, Binder);
                bindingContextLifecycle.BindingContext = bindingContext;
            }
            return bindingContext;
        }

        public static BindingSet<TUIWindow, TSource> CreateBindingSet<TUIWindow, TSource>(this TUIWindow UIWindow) where TUIWindow : UIWindow
        {
            IBindingContext context = UIWindow.BindingContext();
            return new BindingSet<TUIWindow, TSource>(context, UIWindow);
        }

        public static BindingSet<TUIWindow, TSource> CreateBindingSet<TUIWindow, TSource>(this TUIWindow UIWindow, TSource dataContext) where TUIWindow : UIWindow
        {
            IBindingContext context = UIWindow.BindingContext();
            context.DataContext = dataContext;
            return new BindingSet<TUIWindow, TSource>(context, UIWindow);
        }

        public static BindingSet<TUIWindow> CreateBindingSet<TUIWindow>(this TUIWindow UIWindow) where TUIWindow : UIWindow
        {
            IBindingContext context = UIWindow.BindingContext();
            return new BindingSet<TUIWindow>(context, UIWindow);
        }

        public static BindingSet CreateSimpleBindingSet(this UIWindow UIWindow)
        {
            IBindingContext context = UIWindow.BindingContext();
            return new BindingSet(context, UIWindow);
        }

        public static void SetDataContext(this UIWindow UIWindow, object dataContext)
        {
            UIWindow.BindingContext().DataContext = dataContext;
        }

        public static object GetDataContext(this UIWindow UIWindow)
        {
            return UIWindow.BindingContext().DataContext;
        }

        public static void AddBinding(this UIWindow UIWindow, BindingDescription bindingDescription)
        {
            UIWindow.BindingContext().Add(UIWindow, bindingDescription);
        }

        public static void AddBindings(this UIWindow UIWindow, IEnumerable<BindingDescription> bindingDescriptions)
        {
            UIWindow.BindingContext().Add(UIWindow, bindingDescriptions);
        }

        public static void AddBinding(this UIWindow UIWindow, IBinding binding)
        {
            UIWindow.BindingContext().Add(binding);
        }

        public static void AddBinding(this UIWindow UIWindow, IBinding binding, object key = null)
        {
            UIWindow.BindingContext().Add(binding, key);
        }

        public static void AddBindings(this UIWindow UIWindow, IEnumerable<IBinding> bindings, object key = null)
        {
            if (bindings == null)
                return;

            UIWindow.BindingContext().Add(bindings, key);
        }

        public static void AddBinding(this UIWindow UIWindow, object target, BindingDescription bindingDescription, object key = null)
        {
            UIWindow.BindingContext().Add(target, bindingDescription, key);
        }

        public static void AddBindings(this UIWindow UIWindow, object target, IEnumerable<BindingDescription> bindingDescriptions, object key = null)
        {
            UIWindow.BindingContext().Add(target, bindingDescriptions, key);
        }

        public static void AddBindings(this UIWindow UIWindow, IDictionary<object, IEnumerable<BindingDescription>> bindingMap, object key = null)
        {
            if (bindingMap == null)
                return;

            IBindingContext context = UIWindow.BindingContext();
            foreach (var kvp in bindingMap)
            {
                context.Add(kvp.Key, kvp.Value, key);
            }
        }

        public static void ClearBindings(this UIWindow UIWindow, object key)
        {
            UIWindow.BindingContext().Clear(key);
        }

        public static void ClearAllBindings(this UIWindow UIWindow)
        {
            UIWindow.BindingContext().Clear();
        }
    }
}