using System.Runtime.InteropServices;
using System.Security;

namespace JohnsonControls.Metasys.BasicServices;


/// <summary>
/// An implementation of <see cref="ISecretStore"/> that doesn't do anything
/// </summary>
/// <remarks>
/// This is the instance of ISecretStore used by <see cref="Secrets"/> if
/// no suitable functional instance can be found.
/// </remarks>
class DummyStore : ISecretStore
{
    public void AddOrReplacePassword(string hostName, string userName, SecureString password)
    {
    }

    public void AddPassword(string hostName, string userName, SecureString password)
    {
    }

    public void DeletePassword(string hostName, string userName)
    {
    }

    public bool TryGetPassword(string hostName, string userName, out SecureString password)
    {
        password = new();
        password.MakeReadOnly();
        return false;
    }
}
