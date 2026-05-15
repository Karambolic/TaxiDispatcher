using BusinessLogic.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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

    public ICommand LoginCommand => new RelayCommand(ExecuteLogin, CanExecuteLogin);

    // Button is only active if Login is filled AND PasswordBox has content
    private bool CanExecuteLogin(object? parameter)
    {
        var passwordBox = parameter as PasswordBox;
        return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(passwordBox?.Password);
    }

    private void ExecuteLogin(object? parameter)
    {
        ErrorMessage = "";
        var passwordBox = parameter as PasswordBox;
        var password = passwordBox?.Password ?? string.Empty;

        try
        {
            dispatcherService.Login(Login, password);

            // Success logic
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is not MainWindow)?.Close();
            Application.Current.MainWindow = mainWindow;
        }
        catch (InvalidCredentialsException ex)
        {
            // Tells if it's the username or the password
            ErrorMessage = ex.Message;
        }
        catch (ProfileMissingException ex)
        {
            // Tells if the dispatcher doesn't have a profile in the ystem (database)
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Connection error: " + ex.Message;
        }
    }
}