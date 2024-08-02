using JohnsonControls.Metasys.BasicServices;
using JohnsonControls.Metasys.BasicServices.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WeatherForecastApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Add support for JSON Config File
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
            .Build();
            // Define the Metasys API version
            ApiVersion apiVersion = ApiVersion.v4;
            // Read hostname from credential manager first
            var hostnameTarget = config.GetSection("CredentialManager:Targets:MetasysServer").Value;
            // Hostname is stored in the Credential Manager using password field
            var secureHostname = CredentialUtil.GetCredential(hostnameTarget);
            MetasysClient metasysClient = new MetasysClient(CredentialUtil.convertToUnSecureString(secureHostname.Password), true, apiVersion);
            // Retrieve Metasys Credentials to login
            var credTarget = config.GetSection("CredentialManager:Targets:MetasysCredentials").Value;
            // Use TryLogin overload that accepts Credential Manager Target
            metasysClient.TryLogin(credTarget);
            // Forecast container target is securely stored in Credential manager
            var containerTarget = config.GetSection("CredentialManager:Targets:ForecastContainer").Value;
            var secureContainer = CredentialUtil.GetCredential(containerTarget);

            // Get parent object of Weather Forecast to retrieve related children (securely stored in Credential Manager)
            var parentObjectId = metasysClient.GetObjectIdentifier(CredentialUtil.convertToUnSecureString(secureContainer.Password));
            IEnumerable<MetasysObject> weatherForecast = metasysClient.GetObjects(parentObjectId, 1);
            // Retrieve latitude and longitude to get weather forecast
            MetasysObject latitudePoint = weatherForecast.FindByName("Latitude");
            MetasysObject longitudePoint = weatherForecast.FindByName("Longitude");
            double latitude = metasysClient.ReadProperty(latitudePoint.Id, "presentValue").NumericValue;
            double longitude = metasysClient.ReadProperty(longitudePoint.Id, "presentValue").NumericValue;

            //double latitude = 45.46134; // Instead of reading the coordinates from a couple of points I hardcoded them
            //double longitude = 9.13696;
            // Forecast API key is securely stored in Credential manager
            var apiKeyTarget = config.GetSection("CredentialManager:Targets:OpenWeather").Value;
            var secureApiKey = CredentialUtil.GetCredential(apiKeyTarget);
            // Get Next Five Days forecast every 3 hours
            OpenWeatherMapClient weatherMapclient = new OpenWeatherMapClient(CredentialUtil.convertToUnSecureString(secureApiKey.Password));
            ForecastResult forecastResult = weatherMapclient.GetForecast(latitude, longitude).GetAwaiter().GetResult();
            // Get the closest forecast to be written
            Forecast forecast = forecastResult.list.First();
            // Read Metasys points to write the response back
            MetasysObject Day = weatherForecast.FindByName("Day");
            MetasysObject Month = weatherForecast.FindByName("Month");
            MetasysObject Year = weatherForecast.FindByName("Year");
            MetasysObject Hour = weatherForecast.FindByName("Hour");
            MetasysObject Minute = weatherForecast.FindByName("Minute");
            MetasysObject Temperature = weatherForecast.FindByName("Temperature");
            MetasysObject Humidity = weatherForecast.FindByName("Humidity");
            MetasysObject Rain = weatherForecast.FindByName("Rain");
            MetasysObject Snow = weatherForecast.FindByName("Snow");
            // Use commands to write the results
            string adjustCommand = "Adjust";
            if (apiVersion > ApiVersion.v3)
            {
                adjustCommand = "adjustCommand";
            }
            var date = OpenWeatherMapClient.UnixTimeStampToDateTime(forecast.dt);
            metasysClient.SendCommand(Day.Id, adjustCommand, new List<object> { date.Day });
            metasysClient.SendCommand(Month.Id, adjustCommand, new List<object> { date.Month });
            metasysClient.SendCommand(Year.Id, adjustCommand, new List<object> { date.Year });
            metasysClient.SendCommand(Hour.Id, adjustCommand, new List<object> { date.Hour });
            metasysClient.SendCommand(Minute.Id, adjustCommand, new List<object> { date.Minute });
            metasysClient.SendCommand(Temperature.Id, adjustCommand, new List<object> { forecast.main.temp });
            metasysClient.SendCommand(Humidity.Id, adjustCommand, new List<object> { forecast.main.humidity });
            if (forecast.rain != null)
            {
                metasysClient.SendCommand(Rain.Id, adjustCommand, new List<object> { forecast.rain.ThreeHours });
            }
            else
            {
                // Reset values in case there is no rain
                metasysClient.SendCommand(Rain.Id, adjustCommand, new List<object> { 0 });
            }
            if (forecast.snow != null)
            {
                metasysClient.SendCommand(Snow.Id, adjustCommand, new List<object> { forecast.snow.ThreeHours });
            }
            else
            {
                // Reset values in case there is no snow
                metasysClient.SendCommand(Rain.Id, adjustCommand, new List<object> { 0 });
            }
        }
    }
}
