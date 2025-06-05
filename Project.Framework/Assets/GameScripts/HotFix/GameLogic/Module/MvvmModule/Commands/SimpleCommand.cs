using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace GameLogic.Commands
{
public class SimpleCommand : CommandBase
    {
        private bool enabled = true;
        private readonly UnityAction execute;

        public SimpleCommand(UnityAction execute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled == value)
                    return;

                this.enabled = value;
                this.RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.Enabled;
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute();
        }
    }

    public class SimpleCommand<T> : CommandBase, ICommand<T>
    {
        private bool enabled = true;
        private readonly Action<T> execute;

        public SimpleCommand(Action<T> execute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled == value)
                    return;

                this.enabled = value;
                this.RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.Enabled;
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public bool CanExecute(T parameter)
        {
            return this.Enabled;
        }

        public void Execute(T parameter)
        {
            this.execute(parameter);
        }
    }
}