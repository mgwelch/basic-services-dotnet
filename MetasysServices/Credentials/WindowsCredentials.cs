using System;
using System.Runtime.InteropServices;
using System.Security;
using CredentialManagement;
namespace JohnsonControls.Metasys.BasicServices
{



    /// <summary>
    /// An implementation of <see cref="ICredentialManager"/> that uses Windows Credential Manager to
    /// save passwords.
    /// </summary>
    public class WindowsCredentials : ICredentialManager
    {
        private static void AssertRunningOnWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new InvalidOperationException("This service can only be run on Linux and requires 'secret-tool' to be installed.");
            }
        }

        /// <inheritdoc/>
        public override void AddOrReplacePassword(string hostName, string userName, SecureString password)
        {
            AssertRunningOnWindows();
            var target = PrefixHostName(hostName);
            new Credential()
            {
                Target = target,
                Username = userName,
                SecurePassword = password,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }


        /// <inheritdoc/>
        public override void DeletePassword(string hostName, string userName)
        {
            AssertRunningOnWindows();
            var target = PrefixHostName(hostName);
            var credential = new Credential { Target = target, Username = userName };
            credential.Delete();
        }

        /// <inheritdoc/>
        public override bool TryGetPassword(string hostName, string userName, out SecureString password)
        {
            AssertRunningOnWindows();
            var target = PrefixHostName(hostName);
            var credential = new Credential { Target = target, Username = userName };
            var result = credential.Load();
            password = credential.SecurePassword;
            return result;

        }

    }
}
