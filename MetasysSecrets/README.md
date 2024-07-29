# Metasys Secrets

A cli for adding metasys passwords to your operating system credential manager.
These credentials can then be retrieved by your applications using
[SecretStore](../MetasysServices/Credentials/Secrets.cs) and the password can be
securely passed to `MetasysClient.TryLogin` method of
[basic-services-dotnet](../README.md).

## Installation

You'll need a modern version of
[dotnet](https://dotnet.microsoft.com/en-us/download). Both .Net 6.0 and .Net
8.0 are supported.

```bash
dotnet tool install --global JohnsonControls.MetasysSecrets
```

## Usage

There are three subcommands `add`, `lookup` and `delete` as shown below. Each
takes a `hostName` and `userName` for arguments.

```bash
metasys-secrets add <hostName> <userName>
metasys-secrets lookup <hostName> <userName>
metasys-secrets delete <hostName> <userName>
```

### Examples

In these examples we'll assume that the `hostName` is
`my-ads-server.company.com` and the `userName` is `api-service-account`.

### Save a Password

To save the password

```bash
> metasys-secrets add my-ads-server.company.com api-service-account
Enter your password: *******
```

Notice you are prompted for your password. If you really want to do it all on
one line you can do it like this.

```bash
echo "thepassword" | metasys-secrets add my-ads-server.company.com api-service-account
```

> [!Warning]\
> Be careful with this approach as your password is now shown in plain text and will
> be stored in your shell history. It's not recommended for production environments.

### Delete a Password

To delete a password from your credential manager you can do

```bash
metasys-secrets delete <hostName> <userName>
```

### Display a Password

To read a password from the credential manager you can do

```bash
metasys-secrets lookup <hostName> <userName>
```

> [!Warning]\
> Be careful with this approach as your password is now shown in plain text.It's
> not recommended for production environments. You may choose to use the GUI tool
> for your operating system instead.

## Credential Managers

The credential manager that is used depends on the operating system you are
using.

### Windows

On Windows the passwords are saved in the Windows Credential Manager.

### macOS

On macOS the passwords are saved in the macOS Keychain

### Linux

On Linux the passwords are saved using `libsecret` and it relies on the linux
command line tool `secret-tool`. To install that tool depends on the
distribution you are using.

On Debian/Ubuntu you can install it like this

```bash
sudo apt install libsecret-tools
```
