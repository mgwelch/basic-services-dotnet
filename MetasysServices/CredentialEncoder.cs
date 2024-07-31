using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Flurl.Http.Testing;

namespace JohnsonControls.Metasys.BasicServices;
/// <summary>Content provided by a byte array</summary>
/// <remarks>
/// Works just like a ByteArrayContent but in testing mode (HttpTest.Current isn't null) it retains the
/// contents of the request as a string for verification purposes. In production it does not do that.
///
/// The content is assumed to be a utf8 encoded string.
/// </remarks>
public class CapturedByteArrayContent : HttpContent
{
    private readonly byte[] payload;
    public CapturedByteArrayContent(byte[] payload)
    {
        this.payload = payload;
        if (HttpTest.Current != null)
        {
            Content = System.Text.Encoding.UTF8.GetString(payload);
        }
    }

    /// <inheritdoc/>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        return stream.WriteAsync(payload, 0, payload.Length);

        Array.Clear(payload, 0, payload.Length);
    }

    /// <summary>
    /// Returns the decoded byte array.
    /// </summary>
    /// <remarks>The byte array is assumed to be utf8 encoded string.
    /// <para>This will always be null in production.<para>
    /// </remarks>
    public string? Content { get; private set; }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = payload.Length;
        return true;
    }


}



/// <summary>
/// A class to hold username and password (in SecureString) and which
/// provides an <see cref="EncodedPayload"/> property to use to login
/// to REST API /login
/// </summary>
/// <remarks>
/// You must ensure Dispose is called on this class as soon as you are done
/// with the `EncodedPayload` to clear the password bytes from memory
/// </remarks>
public class MetasysCredentials : IDisposable
{
    public MetasysCredentials(string username, SecureString password)
    {
        this.username = username;
        this.password = password;
    }
    private readonly string username;
    private readonly SecureString password;


    byte[]? encodedPayload;

    public byte[] EncodedPayload
    {
        get
        {
            Dispose();
            encodedPayload = EncodePayload(username, password);
            return encodedPayload;
        }
    }

    public static byte[] SecureStringToUtf8Bytes(SecureString secureString)
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


    public void Dispose()
    {
        if (encodedPayload != null) Array.Clear(encodedPayload, 0, encodedPayload.Length);
    }
}
