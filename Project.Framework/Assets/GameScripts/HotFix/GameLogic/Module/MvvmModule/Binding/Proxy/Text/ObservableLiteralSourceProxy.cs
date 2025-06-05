using System;
using GameLogic.Binding.Sources;
using GameLogic.Observables;

namespace GameLogic.Binding.Proxy.Text
{
    public class ObservableLiteralSourceProxy : NotifiableSourceProxyBase, ISourceProxy, IObtainable
    {
        private IObservableProperty observableProperty;

        public ObservableLiteralSourceProxy(IObservableProperty source) : base(source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            this.observableProperty = source;
            this.observableProperty.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            this.RaiseValueChanged();
        }

        public override Type Type { get { return observableProperty.Type; } }

        public virtual object GetValue()
        {
            return this.observableProperty.Value;
        }

        public virtual TValue GetValue<TValue>()
        {
            return (TValue)Convert.ChangeType(this.observableProperty.Value, typeof(TValue));
        }

        #region IDisposable Support    
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this.observableProperty != null)
                    this.observableProperty.ValueChanged -= OnValueChanged;

                disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}