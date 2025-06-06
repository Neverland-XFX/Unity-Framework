﻿using System;
using UnityEngine.EventSystems;
using UnityFramework;

namespace GameLogic.Binding.Proxy.Targets
{
    public abstract class TargetProxyBase : BindingProxyBase, ITargetProxy
    {
        private readonly WeakReference target;
        protected TypeCode typeCode = TypeCode.Empty;
        protected readonly string targetName;
        public TargetProxyBase(object target)
        {
            if (target != null)
            {
                this.target = new WeakReference(target, false);
                this.targetName = target.ToString();
            }
        }

        public abstract Type Type { get; }

        public virtual TypeCode TypeCode
        {
            get
            {
                if (typeCode == TypeCode.Empty)
                {
#if NETFX_CORE
                    typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(Type);
#else
                    typeCode = Type.GetTypeCode(Type);
#endif
                }
                return typeCode;
            }
        }

        public virtual object Target
        {
            get
            {
                var target = this.target != null ? this.target.Target : null;
                return IsAlive(target) ? target : null;
            }
        }
        private bool IsAlive(object target)
        {
            try
            {
                if (target == null)
                    return false;

                if (target is UIBehaviour)
                {
                    if (((UIBehaviour)target).IsDestroyed())
                        return false;
                    return true;
                }

                if (target is UnityEngine.Object)
                {
                    //Check if the object is valid because it may have been destroyed.
                    //Unmanaged objects,the weak caches do not accurately track the validity of objects.
                    var name = ((UnityEngine.Object)target).name;
                    return true;
                }

                return target != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual BindingMode DefaultMode { get { return BindingMode.OneWay; } }
    }

    public abstract class ValueTargetProxyBase : TargetProxyBase, IModifiable, IObtainable, INotifiable
    {
        private bool disposed = false;
        private bool subscribed = false;

        protected readonly object _lock = new object();
        protected EventHandler valueChanged;

        public ValueTargetProxyBase(object target) : base(target)
        {
        }

        public event EventHandler ValueChanged
        {
            add
            {
                lock (_lock)
                {
                    this.valueChanged += value;

                    if (this.valueChanged != null && !this.subscribed)
                        this.Subscribe();
                }
            }

            remove
            {
                lock (_lock)
                {
                    this.valueChanged -= value;

                    if (this.valueChanged == null && this.subscribed)
                        this.Unsubscribe();
                }
            }
        }

        protected void Subscribe()
        {
            try
            {
                if (subscribed)
                    return;

                var target = this.Target;
                if (target == null)
                    return;

                this.subscribed = true;
                this.DoSubscribeForValueChange(target);
            }
            catch (Exception e)
            {
                Log.Error("{0} Subscribe Exception:{1}", this.targetName, e);
            }
        }

        protected virtual void DoSubscribeForValueChange(object target)
        {
        }

        protected void Unsubscribe()
        {
            try
            {
                if (!subscribed)
                    return;

                var target = this.Target;
                if (target == null)
                    return;

                this.subscribed = false;
                this.DoUnsubscribeForValueChange(target);
            }
            catch (Exception e)
            {
                Log.Error("{0} Unsubscribe Exception:{1}", this.targetName, e);
            }
        }
        protected virtual void DoUnsubscribeForValueChange(object target)
        {
        }

        public abstract object GetValue();

        public abstract TValue GetValue<TValue>();

        public abstract void SetValue<TValue>(TValue value);

        public abstract void SetValue(object value);

        protected void RaiseValueChanged()
        {
            try
            {
                var handler = this.valueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Log.Error("{0}", e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                lock (_lock)
                {
                    this.Unsubscribe();
                }
                base.Dispose(disposing);
            }
        }
    }

    public abstract class EventTargetProxyBase : TargetProxyBase, IModifiable
    {
        public EventTargetProxyBase(object target) : base(target)
        {
        }

        public abstract void SetValue(object value);

        public abstract void SetValue<TValue>(TValue value);
    }
}