﻿using System;
using System.Windows.Input;

namespace DummyDB
{
    internal class CommandDelegate : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove {CommandManager.RequerySuggested -= value; }
        }

        public CommandDelegate(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.CanExecute(parameter);
        }

        public void Execute(object parameter) 
        { 
            this.execute(parameter);
        }
    }
}
