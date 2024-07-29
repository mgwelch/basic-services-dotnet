using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

#nullable enable

namespace JohnsonControls.Metasys.BasicServices;

/// <summary>
/// A class to hold username and password (in SecureString) and which
/// provides an <see cref="EncodedPayload"/> property to use to login
/// to REST API /login
/// </summary>
/// <remarks>
/// This class exists so that the password never needs to be converted to a
/// string which represents a security vulnerability since you can't explicitly
/// clear memory associated with a string.
/// <para>
/// You must ensure Dispose is called on this class as soon as you are done
/// with the `EncodedPayload` to clear the password bytes from memory. Best to
/// just wrap it in a using statement.
/// </para>
/// </remarks>
public class MetasysCredentials : IDisposable
{

    /// <summary>
    /// Create a MetasysCredentials Login request payload.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public MetasysCredentials(string username, SecureString password)
    {
        this.username = username;
        this.password = password;
    }
    private readonly string username;
    private readonly SecureString password;


    private byte[]? encodedPayload;

    /// <summary>
    /// Retrieve the Utf8 encoded payload to be used in the login request.
    /// </summary>
    public byte[] EncodedPayload
    {
        get
        {
            if (encodedPayload == null)
            {
                encodedPayload = EncodePayload(username, password);
            }
            return encodedPayload;
        }
    }

    private static byte[] SecureStringToUtf8Bytes(SecureString secureString)
    {
        IntPtr bstr = Marshal.SecureStringToBSTR(secureString);
        int length = secureString.Length;
        char[] chars = new char[length];
        byte[] utf8Bytes;

        try
        {
            Marshal.Copy(bstr, chars, 0, length);
            utf8Bytes = Encoding.UTF8.GetBytes(chars);
        }
        finally
        {
            Array.Clear(chars, 0, length);
            Marshal.ZeroFreeBSTR(bstr);
        }

        return utf8Bytes;
    }


    /// <summary>
    /// Encode this into a the login request payload
    /// </summary>
    /// <remarks>
    /// This method was created to avoid every having the password in a String
    /// </remarks>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public byte[] EncodePayload(string username, SecureString password)
    {
        IntPtr passwordPtr = IntPtr.Zero;
        byte[] passwordBytes = SecureStringToUtf8Bytes(password);
        try
        {
            // Construct JSON payload
            string jsonTemplate = @$"{"{"}""username"":""{username}"",""password"":""";

            //string jsonTemplate = "\"username\":\"{0}\",\"password\":\"";
            byte[] jsonPrefix = Encoding.UTF8.GetBytes(jsonTemplate);
            byte[] jsonSuffix = Encoding.UTF8.GetBytes("\"}");

            byte[] payload = new byte[jsonPrefix.Length + passwordBytes.Length + jsonSuffix.Length];
            Buffer.BlockCopy(jsonPrefix, 0, payload, 0, jsonPrefix.Length);
            Buffer.BlockCopy(passwordBytes, 0, payload, jsonPrefix.Length, passwordBytes.Length);
            Buffer.BlockCopy(jsonSuffix, 0, payload, jsonPrefix.Length + passwordBytes.Length, jsonSuffix.Length);

            return payload;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            if (passwordPtr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordPtr);
            }
            if (passwordBytes != null)
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (encodedPayload != null) Array.Clear(encodedPayload, 0, encodedPayload.Length);
    }
}
