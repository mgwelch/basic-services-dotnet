using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Security;
namespace JohnsonControls.Metasys.BasicServices;


/// <summary>
/// An implementation of <see cref="ISecretStore"/> which uses the Keychain on macOS to
/// store passwords.
/// </summary>
/// <remarks>
/// To manually add passwords to the Keychain, launch the Keychain application. Then select
/// File | New Password Item. In the dialog enter your host name as the "Keychain Item Name" (for
/// example, my-ads-server.my-company-com). Add your Metasys User Name in the "Account Name" field.
/// Finally type your password in the "Password" field.
/// </remarks>
public class Keychain : ISecretStore
{
    private const string SecurityLibrary = "/System/Library/Frameworks/Security.framework/Security";

    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainItemDelete(IntPtr itemRef);
    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainAddGenericPassword(
        IntPtr keychain,
        uint serviceNameLength,
        string serviceName,
        uint accountNameLength,
        string accountName,
        uint passwordLength,
        byte[] passwordData,
        IntPtr itemRef);

    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainFindGenericPassword(
        IntPtr keychain,
        uint serviceNameLength,
        string serviceName,
        uint accountNameLength,
        string accountName,
        out uint passwordLength,
        out IntPtr passwordData,
        out IntPtr itemRef);

    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainItemFreeContent(
        IntPtr attrList,
        IntPtr data);

    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainSearchCreateFromAttributes(
        IntPtr keychainOrArray,
        IntPtr itemClass,
        IntPtr attrList,
        out IntPtr searchRef
    );

    [DllImport(SecurityLibrary)]
    private static extern int SecKeychainSearchCopyNext(
        IntPtr searchRef,
        out IntPtr itemRef
    );

    private static void AssertRunningOnMacOS()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new InvalidOperationException("This service is macOS specific.");
        }
    }

    private void AddPassword(string hostName, string userName, SecureString password)
    {
        AssertRunningOnMacOS();

        var passwordData = SecureStringToByteArray(password);
        var status = SecKeychainAddGenericPassword(
            IntPtr.Zero,
            (uint)hostName.Length,
            hostName,
            (uint)userName.Length,
            userName,
            (uint)passwordData.Length,
            passwordData,
            IntPtr.Zero);

        Array.Clear(passwordData, 0, passwordData.Length);
        if (status != 0)
        {
            throw new InvalidOperationException("Password was not saved. Perhaps a password is already saved for this account.");
        }
    }

    /// <inheritdoc/>
    public bool TryGetPassword(string hostName, string userName, out SecureString password)
    {
        AssertRunningOnMacOS();

        uint passwordLength;
        IntPtr passwordData;
        IntPtr itemRef;

        var status = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            (uint)hostName.Length,
            hostName,
            (uint)userName.Length,
            userName,
            out passwordLength,
            out passwordData,
            out itemRef);

        if (status == 0 && passwordData != IntPtr.Zero)
        {
            // char[] passwordChars = new char[(int)passwordLength / (sizeof(char))];
            // Marshal.Copy(passwordData, passwordChars, 0, (int)passwordLength / sizeof(char));

            // Copy the password data to a byte array
            byte[] passwordBytes = new byte[passwordLength];
            Marshal.Copy(passwordData, passwordBytes, 0, (int)passwordLength);

            // Convert the byte array to a string using UTF-8 encoding
            char[] passwordChars = System.Text.Encoding.UTF8.GetChars(passwordBytes);

            var securePassword = new SecureString();
            passwordChars.ToList().ForEach(securePassword.AppendChar);
            securePassword.MakeReadOnly();

            Array.Clear(passwordBytes, 0, passwordBytes.Length);
            Array.Clear(passwordChars, 0, passwordChars.Length);
            SecKeychainItemFreeContent(IntPtr.Zero, passwordData);
            password = securePassword;
            return true;
        }
        else
        {
            password = new();
            return false;
        }
    }

    /// <inheritdoc/>
    public void AddOrReplacePassword(string hostName, string userName, SecureString newPassword)
    {

        AssertRunningOnMacOS();

        IntPtr itemRef = IntPtr.Zero;
        try
        {
            // Find the existing password
            uint passwordLength;
            IntPtr passwordData;
            var status = SecKeychainFindGenericPassword(
                IntPtr.Zero,
                (uint)hostName.Length,
                hostName,
                (uint)userName.Length,
                userName,
                out passwordLength,
                out passwordData,
                out itemRef);

            if (status == 0 && itemRef != IntPtr.Zero)
            {
                // Delete the existing password
                status = SecKeychainItemDelete(itemRef);
                if (status != 0)
                {
                    throw new InvalidOperationException($"The entry for {hostName}.{userName} could not be replaced.");
                }
            }

            // Add the new password
            AddPassword(hostName, userName, newPassword);
        }
        finally
        {
            // Free the allocated memory for the existing password
            if (itemRef != IntPtr.Zero)
            {
                SecKeychainItemFreeContent(IntPtr.Zero, itemRef);
            }
        }
    }

    private static void DeleteKeychainEntry(string hostName, string userName, bool exceptionIfNotFound = false)
    {
        AssertRunningOnMacOS();

        IntPtr itemRef = IntPtr.Zero;
        uint passwordLength;
        IntPtr passwordData;

        int result = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            (uint)hostName.Length,
            hostName,
            (uint)userName.Length,
            userName,
            out passwordLength,
            out passwordData,
            out itemRef
        );

        if (result == 0 && itemRef != IntPtr.Zero)
        {
            result = SecKeychainItemDelete(itemRef);
            if (result != 0)
            {
                throw new InvalidOperationException($"The entry for {hostName}.{userName} could not be deleted.");
            }

        }
        else if (exceptionIfNotFound)
        {
            throw new InvalidOperationException($"The entry for {hostName}.{userName} could not be found.");
        }

        if (passwordData != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(passwordData);
        }
    }

    /// <summary>
    /// Looks for any entries in the Keychain for the specified hostname
    /// </summary>
    /// <param name="hostName"></param>
    /// <returns></returns>
    public static bool HasKeychainEntry(string hostName)
    {
        IntPtr searchRef = IntPtr.Zero;
        IntPtr itemRef = IntPtr.Zero;

        // Create a search query for the service name
        int result = SecKeychainSearchCreateFromAttributes(
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            out searchRef
        );

        if (result == 0 && searchRef != IntPtr.Zero)
        {
            while (SecKeychainSearchCopyNext(searchRef, out itemRef) == 0)
            {
                // Here you can add additional checks if needed
                if (itemRef != IntPtr.Zero)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static byte[] SecureStringToByteArray(SecureString secureString)
    {
        // Convert SecureString to UTF-8 byte array
        IntPtr unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
        string passwordString = Marshal.PtrToStringUni(unmanagedString);
        byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(passwordString);
        return passwordBytes;
    }

    /// <inheritdoc/>
    public void DeletePassword(string hostName, string userName)
    {
        DeleteKeychainEntry(hostName, userName);
    }
}
