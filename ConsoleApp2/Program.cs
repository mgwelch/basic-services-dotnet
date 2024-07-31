// See https://aka.ms/new-console-template for more information
using JohnsonControls.Metasys.BasicServices;

Console.WriteLine("Hello, World!");

SecretStore.TryGetPassword("welch12.go.johnsoncontrols.com", "api", out var password);

var client = new MetasysClient("welch12.go.johnsoncontrols.com", false, ApiVersion.v5);

client.TryLogin("api", password);

var resutl = client.GetNetworkDevices();
Console.WriteLine(resutl.Count());
