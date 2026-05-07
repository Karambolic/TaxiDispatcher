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
    /// Attempts to authenticate a dispatcher using the specified phone number and password
    /// </summary>
    /// <remarks>If authentication succeeds, the current dispatcher context is updated. The method does not
    /// throw an exception for invalid credentials</remarks>
    /// <param name="phone">The phone number associated with the dispatcher account to authenticate. Cannot be null or empty.</param>
    /// <param name="password">The password to verify for the dispatcher account. Cannot be null or empty.</param>
    /// <returns>true if authentication is successful and the dispatcher is logged in; otherwise, false.</returns>
    public bool Login(string login, string password)
    {
        // Get hashed password from the database for the given login or null if no matches found/Login not found
        string? storedHash = uow.Dispatchers.GetHashedPasswordByLogin(login);

        // If the login does not exist in the database, return false
        if (storedHash == null)
        {
            return false;
        }

        // Login exists, verify the provided password against the stored hash
        if (!PasswordHasher.VerifyPassword(password, storedHash))
        {
            return false;
        }

        // If password accepted, download the dispatcher data for the current session
        // (without password, just profile: name etc.)
        CurrentDispatcher = uow.Dispatchers.GetByLogin(login);

        return true;
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