using System;
using System.Runtime.InteropServices;
using System.Security;

namespace JohnsonControls.Metasys.BasicServices
{

    public class SecretStore
    {
        static SecretStore()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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

        static readonly ISecretStore secretStore;
        public static void AddOrReplacePassword(string hostName, string userName, SecureString password)
        {
            secretStore.AddOrReplacePassword(hostName, userName, password);
        }

        public static bool TryGetPassword(string hostName, string userName, out SecureString password)
        {
            return secretStore.TryGetPassword(hostName, userName, out password);
        }

        public static void DeletePassword(string hostName, string userName)
        {
            secretStore.DeletePassword(hostName, userName);
        }
    }

}
