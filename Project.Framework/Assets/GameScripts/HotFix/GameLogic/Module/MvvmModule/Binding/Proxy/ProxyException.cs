using System;

namespace GameLogic.Binding.Proxy
{
    public class ProxyException : BindingException
    {
        public ProxyException()
        {
        }

        public ProxyException(string message) : base(message)
        {
        }

        public ProxyException(string message, Exception exception) : base(message, exception)
        {
        }

        public ProxyException(string format, params object[] arguments) : base(string.Format(format, arguments))
        {
        }

        public ProxyException(Exception exception, string format, params object[] arguments) : base(string.Format(format, arguments), exception)
        {
        }
    }
}