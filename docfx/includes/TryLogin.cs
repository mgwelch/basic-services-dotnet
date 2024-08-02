using JohnsonControls.Metasys.BasicServices

var client = new MetasysClient("my-ads.company.com");
if (SecureStore.TryGetPassword("my-ads.company.com", "serviceAccount", out SecureString password)
{
    client.TryLogin("serviceAccount", password);
    var devices = client.GetNetworkDevices();
}
