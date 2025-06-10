using System;
using GameLogic.Binding.Contexts;
using UnityEngine.EventSystems;

namespace GameLogic.Binding
{
public abstract class AbstractBinding : IBinding
    {
        private IBindingContext _bindingContext;
        private WeakReference _target;
        private object _dataContext;

        protected AbstractBinding(IBindingContext bindingContext, object dataContext, object target)
        {
            _bindingContext = bindingContext;
            _target = new WeakReference(target, false);
            _dataContext = dataContext;
        }

        public virtual IBindingContext BindingContext
        {
            get => _bindingContext;
            set => _bindingContext = value;
        }

        public virtual object Target
        {
            get
            {
                var target = this._target?.Target;
                return IsAlive(target) ? target : null;
            }
        }

        private bool IsAlive(object target)
        {
            try
            {
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

        public virtual object DataContext
        {
            get => _dataContext;
            set
            {
                if (_dataContext == value)
                    return;

                _dataContext = value;
                OnDataContextChanged();
            }
        }

        protected abstract void OnDataContextChanged();

        #region IDisposable Support     

        protected virtual void Dispose(bool disposing)
        {
            this._bindingContext = null;
            this._dataContext = null;
            this._target = null;
        }

        ~AbstractBinding()
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