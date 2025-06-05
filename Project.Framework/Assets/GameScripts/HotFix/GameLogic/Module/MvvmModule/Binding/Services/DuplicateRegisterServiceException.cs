using System;

namespace GameLogic.Binding.Services
{
    public class DuplicateRegisterServiceException : Exception
    {
        public DuplicateRegisterServiceException()
        {
        }

        public DuplicateRegisterServiceException(string message) : base(message)
        {
        }

        public DuplicateRegisterServiceException(Exception exception) : base("", exception)
        {
        }

        public DuplicateRegisterServiceException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}