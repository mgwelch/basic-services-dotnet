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
public class LinuxTests
{

    [Test]
    public void TestAddLookupAndDelete()
    {

        var hostname = "%--HOST--NAME--%";
        var username = "service-account";
        var password = "\uD83D\uDE01Password😃";
        var secrets = new LinuxLibSecret();
        secrets.AddOrReplacePassword(hostname, username, password.ToSecureString());

        var _ = secrets.TryGetPassword(hostname, username, out var password2);

        Assert.That(password2.ToPlainString(), Is.EqualTo(password));

        secrets.DeletePassword(hostname, username);

        var result = secrets.TryGetPassword(hostname, username, out password2);
        Assert.That(result, Is.False);
    }
}


[TestFixture]
public class Tests
{

    private static readonly Keychain Keychain = new();

    [SetUp]
    public void Setup()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Ignore("Tests are ignored because they are not running on macOS.");
        }

        // Make sure we use random data that is not already in key chain
        ServiceNames = new string[5];
        UserNames = new string[5];
        for (var i = 0; i < 5; i++)
        {
            string randomServiceName;
            do
            {
                randomServiceName = RandomStringGenerator.GenerateRandomString();
            } while (Keychain.HasKeychainEntry(randomServiceName));
            ServiceNames[i] = randomServiceName;

        }
    }

    [TearDown]
    public void TearDown()
    {
        var keychain = new Keychain();
        if (ServiceNames != null)
        {
            foreach (var service in ServiceNames)
            {
                keychain.DeletePassword((string)service, "username");
            }
        }
    }

    public static object[] ServiceNames { get; set; }
    public static object[] GetServiceAccounts()
    {
        ServiceNames = new string[3];
        for (int i = 0; i < 1; i++)
        {
            string random;
            do
            {
                random = RandomStringGenerator.GenerateRandomString();
            } while (Keychain.HasKeychainEntry(random));
            ServiceNames[i] = new object[] { random };

        }

        return ServiceNames;
    }
    public static string[] UserNames { get; set; }

    public static IEnumerable<SecureString> Passwords
    {
        get
        {
            var insecurePassword = RandomStringGenerator.GenerateRandomString();
            var chars = insecurePassword.ToCharArray();


            var securePassword = new SecureString();
            chars.ToList().ForEach(securePassword.AppendChar);
            yield return securePassword;
        }
    }


    // [Test, TestCaseSource(nameof(GetServiceAccounts))]
    // public void AddAndRetrieve(string serviceName)
    // {
    //     string username = "username";
    //     SecureString securePassword = "password".ToSecureString();
    //     Keychain.AddPassword(serviceName, username, securePassword);

    //     Keychain.TryGetPassword(serviceName, username, out SecureString storedSecurePassword);

    //     Assert.That(securePassword.ToPlainString(), Is.EqualTo(storedSecurePassword.ToPlainString()));
    // }

    [Test]
    public void Test2()
    {


        var result = Keychain.TryGetPassword("welch12.go.johnsoncontrols.com", "api", out SecureString securePassword);
        var password = securePassword.ToPlainString();


        Keychain.AddOrReplacePassword("welch12.go.johnsoncontrols.com", "madeup", "badpassword".ToSecureString());
        result = Keychain.TryGetPassword("welch12.go.johnsoncontrols.com", "madeup", out securePassword);
        password = securePassword.ToPlainString();
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
