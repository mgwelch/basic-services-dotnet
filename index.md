# Metasys Basic Services Documentation

This site provides you with articles, guides, and API documentation to help you
write applications that integrate with Metasys.

## Authenticating

The Basic Services Library has support for common credential managers.

# [Windows](#tab/windows)

On Windows the Credential Manager store is supported.

You can record your passwords using `metasys-secrets` cli application.

### First install the application

```bash
dotnet tool install -g JohnsonControls.MetasysSecrets
```

Now save your password in Windows Credential Manager

```bash
# replace my-ads.company.com with the domain name of your server
# replace serviceAccount with the user name of the account you want to use
metasys-secrets store my-ads.company.com serviceAccount
```

You will then be prompted for your password.

Now in your application you can use `SecretStore` to retrieve your password.

[!code-csharp[](includes/TryLogin.cs?highlight=4)]

You see on line 4 we attempt to locate a password.

# [Linux](#tab/linux)

On Linux the libsecret store is supported.

# [macOS](#tab/macOS)

On macOS, the Keychain store is supported

---
