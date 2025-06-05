using System;
using System.Diagnostics;
using GameLogic.Binding.Proxy;
using GameLogic.Binding.Proxy.Sources;
using UnityFramework;

namespace GameLogic.Binding.Sources
{
    public class EmptSourceProxy : SourceProxyBase, IObtainable, IModifiable
    {
        private SourceDescription description;

        public EmptSourceProxy(SourceDescription description) : base(null)
        {
            this.description = description;
        }

        public override Type Type { get { return typeof(object); } }

        public virtual object GetValue()
        {
            //DebugWarning();
            return null;
        }

        public virtual TValue GetValue<TValue>()
        {
            //DebugWarning();
            return default(TValue);
        }

        public virtual void SetValue(object value)
        {
            DebugWarning();
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            DebugWarning();
        }

        [Conditional("DEBUG")]
        private void DebugWarning()
        {
            Log.Error("this is an empty source proxy,If you see this, then the DataContext is null.The SourceDescription is \"{0}\"", description.ToString());
        }
    }
}