using BusinessLogic.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Infrastructure;

namespace UI.ViewModels;

public class LoginViewModel(DispatcherService dispatcherService, IServiceProvider serviceProvider) : ViewModelBase
{
    private string _login = "";
    private string _errorMessage = "";

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    // Define the command for the Login button
    public ICommand LoginCommand => new RelayCommand(ExecuteLogin, CanExecuteLogin);

    // A flag for whether the Login button should be enabled
    private bool CanExecuteLogin(object? parameter) => !string.IsNullOrWhiteSpace(Login);


    //Login itself logic, it will be called when the Login button is clicked
    private void ExecuteLogin(object? parameter)
    {
        var passwordBox = parameter as PasswordBox;
        var password = passwordBox?.Password ?? string.Empty;

        if (dispatcherService.Login(Login, password))
        {
            // Get the MainWindow from the DI container
            var mainWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<MainWindow>(serviceProvider);

            // Show the new window
            mainWindow.Show();

            // Close the current Login window
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = mainWindow;
        }
        else
        {
            ErrorMessage = "Invalid login or password!";
        }
    }
}