using System;

namespace GameLogic.Binding
{
    public class BindingException : Exception
    {
        public BindingException()
        {
        }

        public BindingException(string message) : base(message)
        {
        }

        public BindingException(string message, Exception exception) : base(message, exception)
        {
        }

        public BindingException(string format, params object[] arguments) : base(string.Format(format, arguments))
        {
        }

        public BindingException(Exception exception, string format, params object[] arguments) : base(string.Format(format, arguments), exception)
        {
        }
    }
}