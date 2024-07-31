// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using System.Security;
using JohnsonControls.Metasys.BasicServices;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    Console.WriteLine("running on linux");
}
if (LinuxLibSecret.IsSecretToolAvailable())
{
    Console.WriteLine("secret tool available");
}



if (args.Length != 3)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("metasys-secret add {host} {username}");
    Console.WriteLine("metasys-secret lookup {host} {username}");
    return;
}

switch (args[0])
{
    case "add":
        var password = GetPassword();
        SecretStore.AddOrReplacePassword(args[1], args[2], password);
        break;
    case "lookup":
        if (SecretStore.TryGetPassword(args[1], args[2], out SecureString securePassword))
        {
            Console.WriteLine(ConvertToPlainText(securePassword));
        }
        break;
}

string ConvertToPlainText(SecureString secureString)
{
    IntPtr unmanagedString = IntPtr.Zero;
    try
    {
        unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
        return Marshal.PtrToStringUni(unmanagedString) ?? "";
    }
    finally
    {
        Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
    }
}

//
SecureString GetPassword()
{
    SecureString password = new SecureString();
    if (Console.IsInputRedirected)
    {
        var input = Console.ReadLine();
        input?.ToCharArray().ToList().ForEach(password.AppendChar);
        return password;
    }
    while (true)
    {

        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            break;
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.RemoveAt(password.Length - 1);
                // backup, write a space, and back up again
                Console.Write("\b \b");
            }
        }
        else
        {
            password.AppendChar(key.KeyChar);
            Console.Write("*");
        }
    }
    password.MakeReadOnly();
    return password;
}
