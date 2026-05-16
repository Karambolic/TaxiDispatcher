using System;
using System.Windows.Input;

namespace UI.Infrastructure;

/// <summary>
/// Class that implements the ICommand interface, 
/// allowing to create commands in the ViewModel without having to create a separate class for each command.
/// It takes two parameters: an Action that defines the logic to be executed when the command is invoked, 
/// and an optional Predicate that defines the logic to determine whether the command can execute in its current state at all
/// </summary>
/// <param name="execute"></param>
/// <param name="canExecute"></param>
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