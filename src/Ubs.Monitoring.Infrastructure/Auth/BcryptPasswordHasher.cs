using Ubs.Monitoring.Application.Auth;

namespace Ubs.Monitoring.Infrastructure.Auth;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Verifies whether the provided plaintext password matches the stored password hash.
    /// </summary>
    /// <param name="password">
    /// The plaintext password provided by the user attempting to authenticate.
    /// </param>
    /// <param name="passwordHash">
    /// The stored BCrypt password hash retrieved from persistence.
    /// </param>
    /// <returns>
    /// <c>true</c> if the password matches the hash; otherwise, <c>false</c>.
    /// </returns>
    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash is invalid or was generated using an unsupported BCrypt variant.
            // Consider this an authentication failure rather than a server error.
            return false;
        }
    }
}
