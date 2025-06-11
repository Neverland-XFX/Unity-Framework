using System;
using UnityFramework;

namespace GameLogic.Commands
{
    public class ParameterCommand : ICommand
    {
        
        private readonly object _lock = new object();
        private readonly ICommand _wrappedCommand;
        private readonly ICommandParameter _commandParameter;
        
        public ParameterCommand(ICommand wrappedCommand, ICommandParameter commandParameter)
        {
            if (wrappedCommand == null)
            {
                Log.Error($"wrappedCommand is null");
                return;
            }
                

            this._commandParameter = commandParameter;
            this._wrappedCommand = wrappedCommand;
        }

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { _wrappedCommand.CanExecuteChanged += value; } }
            remove { lock (_lock) { _wrappedCommand.CanExecuteChanged -= value; } }
        }

        public bool CanExecute(object parameter)
        {
            return _wrappedCommand.CanExecute(GetParameterValue());
        }

        public void Execute(object parameter)
        {
            var param = GetParameterValue();
            if (_wrappedCommand.CanExecute(param))
                _wrappedCommand.Execute(param);
        }
        

        protected virtual object GetParameterValue()
        {
            return _commandParameter.GetValue();
        }

        protected virtual Type GetParameterValueType()
        {
            return _commandParameter.GetValueType();
        }
    }
    public class ParameterCommand<T> : ICommand
    {
        private readonly object _lock = new object();
        private readonly ICommand<T> wrappedCommand;
        private readonly ICommandParameter<T> commandParameter;
        public ParameterCommand(ICommand<T> wrappedCommand, ICommandParameter<T> commandParameter)
        {
            if (wrappedCommand == null)
                throw new ArgumentNullException("wrappedCommand");
            if (commandParameter == null)
                throw new ArgumentNullException("commandParameter");

            this.commandParameter = commandParameter;
            this.wrappedCommand = wrappedCommand;
        }

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { this.wrappedCommand.CanExecuteChanged += value; } }
            remove { lock (_lock) { this.wrappedCommand.CanExecuteChanged -= value; } }
        }

        public bool CanExecute(object parameter)
        {
            return wrappedCommand.CanExecute(commandParameter.GetValue());
        }

        public void Execute(object parameter)
        {
            var param = commandParameter.GetValue();
            if (wrappedCommand.CanExecute(param))
                wrappedCommand.Execute(param);
        }
    }
}