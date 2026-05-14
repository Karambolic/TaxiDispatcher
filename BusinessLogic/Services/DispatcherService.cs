using Domain.Entities;
using Infrastructure;
using Infrastructure.Security;

namespace BusinessLogic.Services;

public class DispatcherService(UnitOfWork uow)
{
    /// <summary>
    /// Current dispatcher logged in the system
    /// </summary>
    public Dispatcher? CurrentDispatcher { get; private set; }

    /// <summary>
    /// Attempts to authenticate a dispatcher using the specified login and password
    /// </summary>
    /// <remarks>If authentication succeeds, the current dispatcher context is updated. The method does not
    /// throw an exception for invalid credentials</remarks>
    /// <param name="login">The login associated with the dispatcher account to authenticate. Cannot be null or empty.</param>
    /// <param name="password">The password to verify for the dispatcher account. Cannot be null or empty.</param>
    /// <returns>true if authentication is successful and the dispatcher is logged in; otherwise, false.</returns>
    public bool Login(string login, string password)
    {
        // Get the hash via the login string
        string? storedHash = uow.Dispatchers.GetHashedPasswordByLogin(login);

        if (storedHash == null) 
            return false;

        // Check if the provided password matches the hash
        if (!PasswordHasher.VerifyPassword(password, storedHash)) 
            return false;

        // Fetch the full Dispatcher if login and password are correct
        CurrentDispatcher = uow.Dispatchers.GetByLogin(login);

        return CurrentDispatcher != null;
    }

    public void Logout()
    {
        CurrentDispatcher = null;
    }

    /// <summary>
    /// Dispatcher can update only their profile (first name, last name). Phone number and login are not changeable
    /// </summary>
    /// <param name="firstName">First name to change to</param>
    /// <param name="lastName">Last name to change to</param>
    /// <returns>true if the update is successful; otherwise - false</returns>
    public bool UpdateCurrentProfile(string firstName, string lastName)
    {
        if (CurrentDispatcher == null) return false;

        CurrentDispatcher.FirstName = firstName;
        CurrentDispatcher.LastName = lastName;

        return uow.Dispatchers.Update(CurrentDispatcher);
    }
}