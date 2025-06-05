using System;
using System.ComponentModel;
using GameLogic.Binding.Reflection;
using GameLogic.Binding.Sources;
using UnityFramework;

namespace GameLogic.Binding.Proxy.Sources.Object
{
public class PropertyNodeProxy : NotifiableSourceProxyBase, IObtainable, IModifiable, INotifiable
    {
        protected IProxyPropertyInfo propertyInfo;

        public PropertyNodeProxy(IProxyPropertyInfo propertyInfo) : this(null, propertyInfo)
        {
        }

        public PropertyNodeProxy(object source, IProxyPropertyInfo propertyInfo) : base(source)
        {
            this.propertyInfo = propertyInfo;

            if (this.source == null)
                return;

            if (this.source is INotifyPropertyChanged)
            {
                var sourceNotify = this.source as INotifyPropertyChanged;
                sourceNotify.PropertyChanged += OnPropertyChanged;
            }
            else
            {
                Log.Error("The type {0} does not inherit the INotifyPropertyChanged interface and does not support the PropertyChanged event.", propertyInfo.DeclaringType.Name);
            }
        }

        public override Type Type { get { return propertyInfo.ValueType; } }

        public override TypeCode TypeCode { get { return propertyInfo.ValueTypeCode; } }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;
            if (string.IsNullOrEmpty(name) || name.Equals(propertyInfo.Name))
                this.RaiseValueChanged();
        }

        public virtual object GetValue()
        {
            return propertyInfo.GetValue(source);
        }

        public virtual TValue GetValue<TValue>()
        {
            var proxy = propertyInfo as IProxyPropertyInfo<TValue>;
            if (proxy != null)
                return proxy.GetValue(source);

            return (TValue)this.propertyInfo.GetValue(source);
        }

        public virtual void SetValue(object value)
        {
            propertyInfo.SetValue(source, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            var proxy = propertyInfo as IProxyPropertyInfo<TValue>;
            if (proxy != null)
            {
                proxy.SetValue(source, value);
                return;
            }

            this.propertyInfo.SetValue(source, value);
        }

        #region IDisposable Support    
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this.source != null && this.source is INotifyPropertyChanged)
                {
                    var sourceNotify = this.source as INotifyPropertyChanged;
                    sourceNotify.PropertyChanged -= OnPropertyChanged;
                }
                disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}