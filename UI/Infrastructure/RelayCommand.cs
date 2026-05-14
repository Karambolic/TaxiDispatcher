using System;
using System.Windows.Input;

namespace UI.Infrastructure;

public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand
{
    // Happens when changes occur that affect whether or not the command should execute
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Defines the logic that determines whether the command can execute in its current state
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    // Defines the logic to be called when the command is invoked
    public void Execute(object? parameter) => execute(parameter);
}