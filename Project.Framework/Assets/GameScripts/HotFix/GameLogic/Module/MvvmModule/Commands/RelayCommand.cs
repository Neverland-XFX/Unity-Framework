using System;

namespace GameLogic.Commands
{
public class RelayCommand : CommandBase
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        public RelayCommand(Action execute, bool keepStrongRef) : this(execute, null, keepStrongRef)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this.canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute();
        }
    }

    public class RelayCommand<T> : CommandBase, ICommand<T>
    {
        private readonly Action<T> execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, bool keepStrongRef) : this(execute, null, keepStrongRef)
        {
        }

        public RelayCommand(Action<T> execute, Func<bool> canExecute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this.canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        public bool CanExecute(T parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public void Execute(T parameter)
        {
            this.execute(parameter);
        }
    }
}