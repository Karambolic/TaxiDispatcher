using Domain.Entities;
using Infrastructure;
using Infrastructure.Security;
using UI;

namespace BusinessLogic.Services;

// Custom exceptions for clearer feedback
public class InvalidCredentialsException(string message) : Exception(message);
public class ProfileMissingException(string message) : Exception(message);

public class DispatcherService(UnitOfWork uow, DispatcherSession session)
{
    public Dispatcher? CurrentDispatcher => session.CurrentDispatcher;

    /// <summary>
    /// Authenticates a dispatcher. Throws specific exceptions if login fails.
    /// </summary>
    /// <param name="login">The dispatcher's login (e.g., phone number)</param>
    /// <param name="password">The dispatcher's password</param>
    /// <exception cref="InvalidCredentialsException">Thrown when the login or password is incorrect.</exception>
    /// <exception cref="ProfileMissingException">Thrown when the dispatcher profile is missing despite valid credentials.</exception>
    public void Login(string login, string password)
    {
        string? storedHash = uow.Dispatchers.GetHashedPasswordByLogin(login)?.Trim();

        if (string.IsNullOrEmpty(storedHash))
            throw new InvalidCredentialsException("User not found.");

        if (!PasswordHasher.VerifyPassword(password, storedHash))
            throw new InvalidCredentialsException("Incorrect password.");

        var dispatcher = uow.Dispatchers.GetByLogin(login);
        if (dispatcher == null)
            throw new ProfileMissingException("Credentials exist, but the Dispatcher profile is missing!");

        session.CurrentDispatcher = dispatcher;
    }

    public void Logout() => session.CurrentDispatcher = null;

    public bool UpdateCurrentProfile(string firstName, string lastName)
    {
        if (session.CurrentDispatcher == null) return false;
        session.CurrentDispatcher.FirstName = firstName;
        session.CurrentDispatcher.LastName = lastName;
        return uow.Dispatchers.Update(session.CurrentDispatcher);
    }
}
