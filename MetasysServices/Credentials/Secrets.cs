using System.Runtime.InteropServices;
using System.Security;

namespace JohnsonControls.Metasys.BasicServices
{

    /// <summary>
    /// A cross platform API for managing Metasys passwords using appropriate credential manager
    /// on each operating system.
    /// </summary>
    public class SecretStore
    {
        static SecretStore()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                secretStore = new Keychain();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && LinuxLibSecret.IsSecretToolAvailable())
            {
                secretStore = new LinuxLibSecret();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                secretStore = new WindowsCredentials();
            }
            else
            {
                secretStore = new DummyStore();
            }
        }

        static readonly ICredentialManager secretStore;

        /// <inheritdoc cref="ICredentialManager.AddOrReplacePassword(string, string, SecureString)"/>
        public static void AddOrReplacePassword(string hostName, string userName, SecureString password)
        {
            secretStore.AddOrReplacePassword(hostName, userName, password);
        }

        /// <inheritdoc cref="ICredentialManager.TryGetPassword(string, string, out SecureString)"/>
        public static bool TryGetPassword(string hostName, string userName, out SecureString password)
        {
            return secretStore.TryGetPassword(hostName, userName, out password);
        }

        /// <inheritdoc cref="ICredentialManager.DeletePassword(string, string)"/>
        public static void DeletePassword(string hostName, string userName)
        {
            secretStore.DeletePassword(hostName, userName);
        }
    }

}
