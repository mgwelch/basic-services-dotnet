using System;
using System.Runtime.InteropServices;
using System.Security;
using CredentialManagement;
namespace JohnsonControls.Metasys.BasicServices
{



    /// <summary>
    /// An implementation of <see cref="ISecretStore"/> that uses Windows Credential Manager to
    /// save passwords.
    /// </summary>
    public class WindowsCredentials : ISecretStore
    {
        private static void AssertRunningOnWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new InvalidOperationException("This service can only be run on Linux and requires 'secret-tool' to be installed.");
            }
        }

        /// <inheritdoc/>
        public void AddOrReplacePassword(string hostName, string userName, SecureString password)
        {
            AssertRunningOnWindows();
            new Credential()
            {
                Target = hostName,
                Username = userName,
                SecurePassword = password,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }


        /// <inheritdoc/>
        public void DeletePassword(string hostName, string userName)
        {
            AssertRunningOnWindows();

            var credential = new Credential { Target = hostName, Username = userName };
            credential.Delete();
        }

        /// <inheritdoc/>
        public bool TryGetPassword(string hostName, string userName, out SecureString password)
        {
            AssertRunningOnWindows();

            var credential = new Credential { Target = hostName, Username = userName };
            var result = credential.Load();
            password = credential.SecurePassword;
            return result;

        }

    }
}
