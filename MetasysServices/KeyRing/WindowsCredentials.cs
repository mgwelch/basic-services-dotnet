using System;
using System.Runtime.InteropServices;
using System.Security;
using CredentialManagement;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
namespace JohnsonControls.Metasys.BasicServices
{

    /// <summary>
    /// An implementation of <see cref="ISecretStore"/> that uses Windows Credential Manager to
    /// save passwords.
    /// </summary>
    public class WindowsCredentials : ISecretStore
    {
        /// <inheritdoc/>
        public void AddOrReplacePassword(string hostName, string userName, SecureString password)
        {
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
            var credential = new Credential { Target = hostName, Username = userName };
            credential.Delete();
        }

        /// <inheritdoc/>
        public bool TryGetPassword(string hostName, string userName, out SecureString password)
        {

            var credential = new Credential { Target = hostName, Username = userName };
            var result = credential.Load();
            password = credential.SecurePassword;
            return result;

        }

    }
}
