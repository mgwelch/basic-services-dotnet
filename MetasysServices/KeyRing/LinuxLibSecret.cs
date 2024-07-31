using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

#nullable enable

namespace JohnsonControls.Metasys.BasicServices;

/// <summary>
/// Provides access to secrets stored using the Linux library libsecret.
/// </summary>
/// <remarks>
/// This class has a dependency on the command line tool <c>secret-tool</c> which
/// can be installed on Ubuntu using <c>sudo apt install libsecret-tools</c>.
/// <para>
/// The gui tool related to this is the Gnome Passwords application also known as
/// "seahorse". This tool can be used to view saved passwords but it is not useful
/// for adding passwords as it doesn't use the attributes needed for <see cref="LinuxLibSecret"/>
/// to retrieve them.
/// </para>
/// <para>
/// If you wish to manually add passwords to the secret store. You can use <c>secret-tool</c> to do so.
/// Here is an example:
/// <code>
/// secret-tool store -l "My Label" "Host Name" my-ads-server.com "User Name" apiServiceAccount
/// </code>
/// In this example, the <c>-l</c> parameter allows you to pick a label for this entry. You can choose any label you wish.
/// The strings "Host Name" and "User Name" are attributes used to lookup the password later. You must enter them exactly as
/// shown. The strings <c>my-ads-server.com</c> and <c>apiServiceAccount</c> are your host name and user name respectively.
/// After running this command you'll be prompted to enter your password.
/// </para>
/// <para>
/// You can also pass the password on the command line but take care as it will be in plain text. To do this
/// you pipe the standard input to the <c>secret-tool</c> command like this:
/// <code>
/// echo "myPlainTextPassword" | secret-tool -l "api@my-ads-server.com" "Host Name" my-ads-server.com "User Name" api
/// </code>
/// </para>
/// <para>
/// If you wish to manually lookup passwords from the command line you can do so like this
/// <code>
/// > secret-tool lookup "Host Name" my-ads-server.com "User Name" api
/// myPlainTextPassword
/// </code>
/// </para>
/// </remarks>
public class LinuxLibSecret : ICredentialManager
{

    private static void AssertRunningOnLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || !IsSecretToolAvailable())
        {
            throw new InvalidOperationException("This service can only be run on Linux and requires 'secret-tool' to be installed.");
        }
    }

    /// <summary>
    /// Checks to see if the command line tool "secret-tool" is available
    /// </summary>
    /// <remarks>
    /// If the tool is not available it can be installed on Debian/Ubuntu systems
    /// using <code>sudo apt install libsecret-tools</code>
    /// </remarks>.
    /// <returns></returns>
    public static bool IsSecretToolAvailable()
    {
        const string toolName = "secret-tool";
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = toolName,
                Arguments = "search \"dummy attribute\" \"dummy value\"", // or any other argument that doesn't alter state
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit((int)TimeSpan.FromSeconds(2).TotalMilliseconds);
                return process.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override void AddOrReplacePassword(string hostName, string userName, SecureString password)
    {
        AssertRunningOnLinux();
        RunSecretToolStore(PrefixHostName(hostName), userName, password);
    }
    /// <inheritdoc/>
    public override bool TryGetPassword(string hostName, string userName, out SecureString password)
    {
        AssertRunningOnLinux();
        var result = RunSecretToolLookup(PrefixHostName(hostName), userName);
        password = result == null ? new() : result;
        return result != null;
    }

    /// <inheritdoc/>
    public override void DeletePassword(string hostName, string userName)
    {
        AssertRunningOnLinux();
        var process = Process.Start("secret-tool", $"clear {HostNameAttribute} \"{PrefixHostName(hostName)}\" {UserNameAttribute} \"{userName}\"");
        process.WaitForExit(2000);
    }


    const string HostNameAttribute = "\"Host Name\"";
    const string UserNameAttribute = "\"User Name\"";


    private static char[] SecureStringToCharArray(SecureString secureString)
    {
        IntPtr unmanagedString = IntPtr.Zero;
        byte[] utf8Bytes;

        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            int utf16ByteCount = secureString.Length * 2; // UTF-16 encoding uses 2 bytes per character
            byte[] utf16Bytes = new byte[utf16ByteCount];
            Marshal.Copy(unmanagedString, utf16Bytes, 0, utf16ByteCount);

            utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }

        char[] charArray = Encoding.UTF8.GetChars(utf8Bytes);
        return charArray;

    }

    private static void RunSecretToolStore(string service, string username, SecureString password)
    {
        var label = $"\"Password for {username}@{service}\"";
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "secret-tool",
            Arguments = $"store -l {label} {HostNameAttribute} \"{service}\" {UserNameAttribute} \"{username}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        char[]? chars = null;
        try
        {
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                chars = SecureStringToCharArray(password);

                chars.ToList().ForEach(c => process.StandardInput.Write(c));
                process.StandardInput.Close();

                process.WaitForExit((int)TimeSpan.FromSeconds(3).TotalMilliseconds);
            }
        }
        finally
        {
            if (chars != null)
            {
                Array.Clear(chars, 0, chars.Length);
            }
        }
    }

    private static SecureString? RunSecretToolLookup(string service, string username)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "secret-tool",
            Arguments = $"lookup {HostNameAttribute} \"{service}\" {UserNameAttribute} \"{username}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            var password = new SecureString();
            int nextChar;
            int length = 0;
            while ((nextChar = process.StandardOutput.Read()) != -1)
            {
                length++;
                password.AppendChar((char)nextChar);
            }
            password.MakeReadOnly();
            process.WaitForExit();
            return length > 0 ? password : null;
        }
    }

}
