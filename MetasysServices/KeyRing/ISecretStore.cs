using System.Runtime.InteropServices;
using System.Security;

namespace JohnsonControls.Metasys.BasicServices;

/// <summary>
/// Represents a secret store for passwords
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Adds or replaces a password in the secret store
    /// </summary>
    /// <remarks>
    /// The password will be recorded as being for a specific host and a specific user
    /// The password can later be retrieved by calling <see cref="TryGetPassword(string, string, out SecureString)"/>
    /// passing in the same hostName and userName that was used to save the password.
    /// </remarks>
    /// <param name="hostName"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    void AddOrReplacePassword(string hostName, string userName, SecureString password);

    /// <summary>
    /// Attempts to retrieve a stored password
    /// </summary>
    /// <param name="hostName"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns><b>true</b> if the password for the user on the specified host exists; <b>false</b> otherwise.</returns>
    bool TryGetPassword(string hostName, string userName, out SecureString password);

    /// <summary>
    /// Deletes the password with specified hostName and userName if it exists.
    /// </summary>
    /// <remarks>This method does nothing if the password doesn't exist</remarks>
    /// <param name="hostName"></param>
    /// <param name="userName"></param>
    void DeletePassword(string hostName, string userName);
}
