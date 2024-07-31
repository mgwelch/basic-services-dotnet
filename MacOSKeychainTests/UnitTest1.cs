using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace JohnsonControls.Metasys.BasicServices;

internal static class SecureStringExtensions
{
    public static string ToPlainString(this SecureString secureString)
    {

        if (secureString == null)
            throw new ArgumentNullException(nameof(secureString));

        IntPtr? unmanagedString = IntPtr.Zero;
        try
        {
            // Convert SecureString to an unmanaged string
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            if (unmanagedString == IntPtr.Zero || unmanagedString == null) return "";
            // Convert the unmanaged string to a managed string
            return Marshal.PtrToStringUni(unmanagedString.Value) ?? "";
        }
        finally
        {
            // Free the unmanaged string
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString.Value);
        }
    }

    public static SecureString ToSecureString(this string insecure)
    {
        var chars = insecure.ToCharArray();
        var secure = new SecureString();
        chars.ToList().ForEach(secure.AppendChar);
        secure.MakeReadOnly();
        return secure;
    }
}
[TestFixture]
public class SecretStoreTests
{


    [Test]
    public void TestAddLookupAndDelete()
    {

        var hostname = "%--HOST--NAME--%";
        var username = "service-account";
        var password = "\uD83D\uDE01Password😃";
        SecretStore.AddOrReplacePassword(hostname, username, password.ToSecureString());

        var _ = SecretStore.TryGetPassword(hostname, username, out var password2);

        Assert.That(password2.ToPlainString(), Is.EqualTo(password));

        SecretStore.DeletePassword(hostname, username);

        var result = SecretStore.TryGetPassword(hostname, username, out password2);
        Assert.That(result, Is.False);
    }
}



public class RandomStringGenerator
{
    private static readonly Random random = new Random();

    public static string GenerateRandomString(int minLength = 10, int maxLength = 20)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        int length = random.Next(minLength, maxLength + 1);
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    public static string GenerateRandomString2(int minLength = 10, int maxLength = 20)
    {
        int length = random.Next(minLength, maxLength + 1);
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int codePoint = GetRandomCodePoint();
            result.Append(char.ConvertFromUtf32(codePoint));
        }

        return result.ToString();
    }

    private static int GetRandomCodePoint()
    {
        // Generate a random code point, including supplementary characters
        int codePoint;
        if (random.Next(0, 10) < 8)
        {
            // Most of the time, generate a BMP character (1-3 bytes)
            codePoint = random.Next(0x0000, 0xFFFF);
        }
        else
        {
            // Occasionally generate a supplementary character (4 bytes)
            codePoint = random.Next(0x10000, 0x10FFFF);
        }

        // Ensure the code point is valid
        while (char.GetUnicodeCategory((char)codePoint) == UnicodeCategory.OtherNotAssigned)
        {
            codePoint = random.Next(0x0000, 0x10FFFF);
        }

        return codePoint;
    }
}
