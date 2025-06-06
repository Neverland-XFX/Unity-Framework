﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using GameLogic.Binding.Reflection;
using GameLogic.Binding.Sources;

namespace GameLogic.Binding.Proxy.Sources.Object
{
public abstract class ItemNodeProxy<TKey> : NotifiableSourceProxyBase, IObtainable, IModifiable, INotifiable
    {
        protected TKey key;
        protected IProxyItemInfo itemInfo;
        protected bool isList;
        protected Regex regex;

        public ItemNodeProxy(ICollection source, TKey key, IProxyItemInfo itemInfo) : base(source)
        {
            this.key = key;
            this.isList = (source is IList);

            this.itemInfo = itemInfo;

            if (this.source != null && this.source is INotifyCollectionChanged)
            {
                var sourceCollection = this.source as INotifyCollectionChanged;
                sourceCollection.CollectionChanged += OnCollectionChanged;
            }

            if (!this.isList)
            {
                this.regex = new Regex(@"\[" + this.key + ",", RegexOptions.IgnorePatternWhitespace);
            }
        }

        public override Type Type { get { return this.itemInfo.ValueType; } }

        public override TypeCode TypeCode { get { return itemInfo.ValueTypeCode; } }

        protected abstract void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

        public virtual object GetValue()
        {
            return this.itemInfo.GetValue(source, key);
        }

        public virtual TValue GetValue<TValue>()
        {
            if (!typeof(TValue).IsAssignableFrom(this.itemInfo.ValueType))
                throw new InvalidCastException();

            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
                return proxy.GetValue(source, key);

            return (TValue)this.itemInfo.GetValue(source, key);
        }

        public virtual void SetValue(object value)
        {
            this.itemInfo.SetValue(source, key, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
            {
                proxy.SetValue(source, key, value);
                return;
            }

            this.itemInfo.SetValue(source, key, value);
        }

        #region IDisposable Support    
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this.source != null && this.source is INotifyCollectionChanged)
                {
                    var sourceCollection = this.source as INotifyCollectionChanged;
                    sourceCollection.CollectionChanged -= OnCollectionChanged;
                }
                disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }

    public class IntItemNodeProxy : ItemNodeProxy<int>
    {
        public IntItemNodeProxy(ICollection source, int key, IProxyItemInfo itemInfo) : base(source, key, itemInfo)
        {
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isList)
            {
                //IList or Array
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        this.RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        if (this.key == e.OldStartingIndex || this.key == e.NewStartingIndex)
                            this.RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (this.key == e.OldStartingIndex || this.key == e.NewStartingIndex)
                            this.RaiseValueChanged();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        int endIndex = e.NewItems != null ? e.NewStartingIndex + e.NewItems.Count : e.NewStartingIndex + 1;
                        if (this.key >= e.NewStartingIndex && this.key < endIndex)
                            this.RaiseValueChanged();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //IDictionary
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    this.RaiseValueChanged();
                    return;
                }

                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (regex.IsMatch(item.ToString()))
                        {
                            this.RaiseValueChanged();
                            return;
                        }
                    }
                }

                if (e.OldItems != null && e.OldItems.Count > 0)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (regex.IsMatch(item.ToString()))
                        {
                            this.RaiseValueChanged();
                            return;
                        }
                    }
                }
            }
        }


    }

    public class StringItemNodeProxy : ItemNodeProxy<string>
    {
        public StringItemNodeProxy(ICollection source, string key, IProxyItemInfo itemInfo) : base(source, key, itemInfo)
        {
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //IDictionary
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.RaiseValueChanged();
                return;
            }

            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    if (regex.IsMatch(item.ToString()))
                    {
                        this.RaiseValueChanged();
                        return;
                    }
                }
            }

            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (regex.IsMatch(item.ToString()))
                    {
                        this.RaiseValueChanged();
                        return;
                    }
                }
            }
        }
    }
}