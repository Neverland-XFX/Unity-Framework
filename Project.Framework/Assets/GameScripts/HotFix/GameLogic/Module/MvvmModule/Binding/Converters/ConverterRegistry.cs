
using System;
using GameLogic.Binding.Registry;

namespace GameLogic.Binding.Converters
{
    public class ConverterRegistry : KeyValueRegistry<string, IConverter>, IConverterRegistry
    {
        public ConverterRegistry()
        {
            this.Init();
        }

        protected virtual void Init()
        {
        }

        public override void Unregister(string key)
        {
            if (this.lookups.ContainsKey(key))
            {
                try
                {
                    if (lookups[key] is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (Exception) { }
                this.lookups.Remove(key);
            }
        }
    }
}
