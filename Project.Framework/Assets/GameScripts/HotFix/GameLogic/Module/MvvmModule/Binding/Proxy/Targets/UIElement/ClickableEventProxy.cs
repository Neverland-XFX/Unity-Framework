using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using GameLogic.Binding.Reflection;
using GameLogic.Commands;
using UnityEngine.UIElements;
using UnityFramework;

#if UNITY_2019_1_OR_NEWER
namespace GameLogic.Binding.Proxy.Targets.UIElement
{
public class ClickableEventProxy : EventTargetProxyBase
    {
        private bool disposed = false;
        protected ICommand command;/* Command Binding */
        protected IInvoker invoker;/* Method Binding or Lua Function Binding */
        protected Delegate handler;/* Delegate Binding */

        protected readonly Clickable clickable;
        protected SendOrPostCallback updateTargetEnableAction;

        public ClickableEventProxy(object target, Clickable clickable) : base(target)
        {
            this.clickable = clickable;
            this.BindEvent();
        }

        public override BindingMode DefaultMode { get { return BindingMode.OneWay; } }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UnbindCommand(this.command);
                this.UnbindEvent();
                disposed = true;
                base.Dispose(disposing);
            }
        }

        public override Type Type { get { return typeof(Clickable); } }

        protected virtual void BindEvent()
        {
            this.clickable.clicked += OnEvent;
        }

        protected virtual void UnbindEvent()
        {
            this.clickable.clicked -= OnEvent;
        }

        protected virtual bool IsValid(Delegate handler)
        {
            if (handler is Action)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count == 0)
                return true;

            return false;
        }

        protected virtual bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters != null && parameters.Length != 0)
                return false;
            return true;
        }

        protected virtual void OnEvent()
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(null);
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke();
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is Action)
                    {
                        (this.handler as Action)();
                    }
                    else
                    {
                        this.handler.DynamicInvoke();
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error("{0}", e);
            }
        }

        public override void SetValue(object value)
        {
            var target = this.Target;
            if (target == null)
                return;

            if (this.command != null)
            {
                UnbindCommand(this.command);
                this.command = null;
            }

            if (this.invoker != null)
                this.invoker = null;

            if (this.handler != null)
                this.handler = null;

            if (value == null)
                return;

            //Bind Command
            ICommand command = value as ICommand;
            if (command != null)
            {
                this.command = command;
                BindCommand(this.command);
                UpdateTargetEnable();
                return;
            }

            //Bind Method
            IProxyInvoker proxyInvoker = value as IProxyInvoker;
            if (proxyInvoker != null)
            {
                if (this.IsValid(proxyInvoker))
                {
                    this.invoker = proxyInvoker;
                    return;
                }

                throw new ArgumentException("Bind method failed.the parameter types do not match.");
            }

            //Bind Delegate
            Delegate handler = value as Delegate;
            if (handler != null)
            {
                if (this.IsValid(handler))
                {
                    this.handler = handler;
                    return;
                }

                throw new ArgumentException("Bind method failed.the parameter types do not match.");
            }

            //Bind Script Function
            IInvoker invoker = value as IInvoker;
            if (invoker != null)
            {
                this.invoker = invoker;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            this.SetValue((object)value);
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (updateTargetEnableAction == null)
                updateTargetEnableAction = UpdateTargetEnable;
            UISynchronizationContext.Post(updateTargetEnableAction, null);
        }

        protected virtual void UpdateTargetEnable(object state = null)
        {
            var target = this.Target;
            if (target == null || !(target is VisualElement))
                return;

            bool value = this.command == null ? false : this.command.CanExecute(null);
            ((VisualElement)target).SetEnabled(value);
        }

        protected virtual void BindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        protected virtual void UnbindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged -= OnCanExecuteChanged;
        }
    }
}
#endif