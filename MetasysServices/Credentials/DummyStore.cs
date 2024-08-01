using System.Security;

namespace JohnsonControls.Metasys.BasicServices;


/// <summary>
/// An implementation of <see cref="ICredentialManager"/> that doesn't do anything
/// </summary>
/// <remarks>
/// This is the instance of ISecretStore used by <see cref="SecretStore"/> if
/// no suitable functional instance can be found.
/// </remarks>
class DummyStore : ICredentialManager
{
    public override void AddOrReplacePassword(string hostName, string userName, SecureString password)
    {
    }


    public override void DeletePassword(string hostName, string userName)
    {
    }

    public override bool TryGetPassword(string hostName, string userName, out SecureString password)
    {
        password = new();
        password.MakeReadOnly();
        return false;
    }
}
