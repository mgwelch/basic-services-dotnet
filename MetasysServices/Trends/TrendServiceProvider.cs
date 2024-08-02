using Flurl.Http;
using JohnsonControls.Metasys.BasicServices.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Provider Class for Trend Service
    /// </summary>
    public class TrendServiceProvider : BasicServiceProvider, ITrendService
    {
        private CultureInfo _CultureInfo = new CultureInfo("en-US");

        /// <summary>
        /// Caching about read units.
        /// </summary>
        protected Dictionary<string, string> Units = new Dictionary<string, string>();

        /// <summary>
        /// Initialize a new instance given the Flurl client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="logClientErrors">Set this flag to false to disable logging of client errors.</param>
        public TrendServiceProvider(IFlurlClient client, ApiVersion version, bool logClientErrors = true) : base(client, version, logClientErrors)
        {
        }

        /// <inheritdoc/>
        public PagedResult<Sample> GetSamples(ObjectId objectId, int attributeId, TimeFilter filter)
        {
            CheckVersion(Version);
            if (Version > ApiVersion.v3)
            {
                //Convert the numeric attributeID to the equivalent enumerated text in order to use the new method valid only when version > v3
                String name = Enum.GetName(typeof(AttributeEnumSet), attributeId);
                AttributeEnumSet attributeName = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), name);
                return GetSamplesAsync(objectId, attributeName, filter).GetAwaiter().GetResult();
            }
            else
            {
                return GetSamplesAsync(objectId, attributeId, filter).GetAwaiter().GetResult();
            }
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Sample>> GetSamplesAsync(ObjectId objectId, int attributeId, TimeFilter filter)
        {
            //Note: this method is valid only for API version v2 and v3
            if (Version < ApiVersion.v2 | Version > ApiVersion.v3) { throw new MetasysUnsupportedApiVersion(Version.ToString()); }
            List<Sample> objectSamples = new List<Sample>();
            // Perform a generic call using objects resource valid for Network Devices as well
            var response = await GetPagedResultsAsync<JToken>("objects", ToDictionary(filter), objectId, "attributes", attributeId, "samples").ConfigureAwait(false);
            // Read full attribute from url
            foreach (JToken s in response.Items)
            {
                Sample sample = new Sample();
                string unitsUrl = string.Empty;
                try
                {
                    sample.Timestamp = s["timestamp"].Value<DateTime>();
                    sample.IsReliable = s["isReliable"].Value<Boolean>();
                    sample.Value = s["value"]["value"].Value<double>();
                    unitsUrl = s["value"]["units"].Value<string>();
                    sample.Unit = ResourceManager.Localize(unitsUrl, _CultureInfo);

                    //if (Version < ApiVersion.v3) {
                    //    sample.Timestamp = s["timestamp"].Value<DateTime>();
                    //    sample.IsReliable = s["isReliable"].Value<Boolean>();
                    //    sample.Value = s["value"]["value"].Value<double>();
                    //    unitsUrl = s["value"]["units"].Value<string>();
                    //    sample.Unit = ResourceManager.Localize(unitsUrl, _CultureInfo);
                    //} else {
                    //    // Note: for Api v3 the schema was changed then it came back to the same as previous versions.
                    //    sample.Timestamp = s["result"]["timestamp"].Value<DateTime>();
                    //    sample.IsReliable = s["result"]["isReliable"].Value<Boolean>();
                    //    sample.Value = s["result"]["value"]["value"]["value"].Value<double>();
                    //    sample.Unit = s["result"]["value"]["units"].Value<string>();
                    //    sample.Unit = ResourceManager.Localize(sample.Unit, _CultureInfo);
                    //}
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
                if (Version < ApiVersion.v3)
                {
                    // On Api v2 and v1 there was the url endpoint of the enum instead of the fully qualified enumeration string
                    var unitId = unitsUrl.Split('/').Last();
                    // Read full url if not cached previously
                    if (!Units.ContainsKey(unitId))
                    {
                        var unit = await GetWithFullUrl(unitsUrl).ConfigureAwait(false);
                        Units.Add(unitId, unit["description"].Value<string>());
                    }
                    sample.Unit = Units[unitId];
                }
                objectSamples.Add(sample);
            }
            // Type the response as Sample List
            return new PagedResult<Sample>
            {
                Items = objectSamples,
                CurrentPage = response.CurrentPage,
                PageCount = response.PageCount,
                PageSize = response.PageSize,
                Total = response.Total
            };
        }
        /// <inheritdoc/>
        public PagedResult<Sample> GetSamples(ObjectId objectId, AttributeEnumSet attributeName, TimeFilter filter)
        {
            if (Version > ApiVersion.v3)
            {
                return GetSamplesAsync(objectId, attributeName, filter).GetAwaiter().GetResult();
            }
            else
            {
                return GetSamplesAsync(objectId, (int)attributeName, filter).GetAwaiter().GetResult();
            }
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Sample>> GetSamplesAsync(ObjectId objectId, AttributeEnumSet attributeName, TimeFilter filter)
        {
            //Note: this method is valid for API version > v3
            if (Version < ApiVersion.v4) { throw new MetasysUnsupportedApiVersion(Version.ToString()); }
            List<Sample> objectSamples = new List<Sample>();
            // Perform a generic call using objects resource valid for Network Devices as well
            var response = await GetPagedResultsAsync<JToken>("objects", ToDictionary(filter), objectId, "trendedAttributes", attributeName.ToString(), "samples").ConfigureAwait(false);
            // Read full attribute from url
            foreach (JToken s in response.Items)
            {
                Sample sample = new Sample();
                string unitsUrl = string.Empty;
                try
                {
                    sample.Timestamp = s["timestamp"].Value<DateTime>();
                    sample.IsReliable = s["isReliable"].Value<Boolean>();
                    sample.Value = s["value"]["value"].Value<double>();
                    unitsUrl = s["value"]["units"].Value<string>();
                    sample.Unit = ResourceManager.Localize(unitsUrl, _CultureInfo);
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
                objectSamples.Add(sample);
            }
            // Type the response as Sample List
            return new PagedResult<Sample>
            {
                Items = objectSamples,
                CurrentPage = response.CurrentPage,
                PageCount = response.PageCount,
                PageSize = response.PageSize,
                Total = response.Total
            };
        }

        // GetTrendedAttributes --------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public List<MetasysAttribute> GetTrendedAttributes(ObjectId id)
        {
            return GetTrendedAttributesAsync(id).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<List<MetasysAttribute>> GetTrendedAttributesAsync(ObjectId id)
        {
            CheckVersion(Version);

            List<MetasysAttribute> objectAttributes = new List<MetasysAttribute>();
            // Perform a generic call using objects resource valid for Network Devices as well
            JToken attributes = (await GetRequestAsync("objects", null, id, "trendedAttributes").ConfigureAwait(false));
            // Read full attribute from url
            if (!(attributes["items"] is JArray))
            {
                // This structure applies since v3-pre release
                if (attributes["items"]["item"] != null)
                {
                    attributes["items"] = attributes["items"]["item"];
                }
            }
            foreach (var a in attributes["items"])
            {
                try
                {
                    MetasysAttribute metasysAttribute = new MetasysAttribute();
                    if (Version < ApiVersion.v3)
                    {
                        var attributeUrl = a["attributeUrl"].Value<string>();
                        var attribute = await GetWithFullUrl(attributeUrl).ConfigureAwait(false);
                        metasysAttribute.Id = attribute["id"].Value<int>();
                        metasysAttribute.Description = attribute["description"].Value<string>();
                    }
                    else
                    {
                        // Since Api v3 the schema has changed and contains the enum fully qualified name instead of the URL
                        metasysAttribute.Description = a["attribute"].Value<string>();
                        // Take the attribute ID from the samples url
                        var samplesUrl = a["samplesUrl"].Value<string>();
                        var attrId = samplesUrl.Split('/').Reverse().Skip(1).FirstOrDefault();
                        if (Version == ApiVersion.v3)
                        {
                            metasysAttribute.Id = int.Parse(attrId);
                        }
                        else
                        {
                            AttributeEnumSet attributeName = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), attrId);
                            metasysAttribute.Id = (int)attributeName;
                        }
                    }
                    objectAttributes.Add(metasysAttribute);
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
            }
            return objectAttributes;
        }

        // GetNetDevTrendedAttributes --------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public List<MetasysAttribute> GetNetDevTrendedAttributes(ObjectId id)
        {
            return GetNetDevTrendedAttributesAsync(id).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<List<MetasysAttribute>> GetNetDevTrendedAttributesAsync(ObjectId id)
        {
            CheckVersion(Version);

            List<MetasysAttribute> objectAttributes = new List<MetasysAttribute>();
            // Perform a generic call using objects resource valid for Network Devices as well
            JToken attributes = (await GetRequestAsync("networkDevices", null, id, "trendedAttributes").ConfigureAwait(false));
            // Read full attribute from url
            if (!(attributes["items"] is JArray))
            {
                // This structure applies since v3-pre release
                if (attributes["items"]["item"] != null)
                {
                    attributes["items"] = attributes["items"]["item"];
                }
            }
            foreach (var a in attributes["items"])
            {
                try
                {
                    MetasysAttribute metasysAttribute = new MetasysAttribute();
                    if (Version < ApiVersion.v3)
                    {
                        var attributeUrl = a["attributeUrl"].Value<string>();
                        var attribute = await GetWithFullUrl(attributeUrl).ConfigureAwait(false);
                        metasysAttribute.Id = attribute["id"].Value<int>();
                        metasysAttribute.Description = attribute["description"].Value<string>();
                    }
                    else
                    {
                        // Since Api v3 the schema has changed and contains the enum fully qualified name instead of the URL
                        metasysAttribute.Description = a["attribute"].Value<string>();
                        // Take the attribute ID from the samples url
                        var samplesUrl = a["samplesUrl"].Value<string>();
                        var attrId = samplesUrl.Split('/').Reverse().Skip(1).FirstOrDefault();
                        if (Version == ApiVersion.v3)
                        {
                            metasysAttribute.Id = int.Parse(attrId);
                        }
                        else
                        {
                            AttributeEnumSet attributeName = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), attrId);
                            metasysAttribute.Id = (int)attributeName;
                        }
                    }
                    objectAttributes.Add(metasysAttribute);
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
            }
            return objectAttributes;
        }

        // GetNetDevSamples
        /// <inheritdoc/>
        public PagedResult<Sample> GetNetDevSamples(ObjectId networkDeviceId, int attributeId, TimeFilter filter)
        {
            if (Version > ApiVersion.v3)
            {
                //Convert the numeric attributeID to the equivalent enumerated text in order to use the new method valid only when version > v3
                String name = Enum.GetName(typeof(AttributeEnumSet), attributeId);
                AttributeEnumSet attributeName = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), name);
                return GetNetDevSamplesAsync(networkDeviceId, attributeName, filter).GetAwaiter().GetResult();
            }
            else
            {
                return GetNetDevSamplesAsync(networkDeviceId, attributeId, filter).GetAwaiter().GetResult();
            }
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Sample>> GetNetDevSamplesAsync(ObjectId networkDeviceId, int attributeId, TimeFilter filter)
        {
            //Note: this method is valid only for API version v2 and v3
            if (Version < ApiVersion.v2 | Version > ApiVersion.v3) { throw new MetasysUnsupportedApiVersion(Version.ToString()); }
            List<Sample> objectSamples = new List<Sample>();
            // Perform a generic call using objects resource valid for Network Devices as well
            var response = await GetPagedResultsAsync<JToken>("networkDevices", ToDictionary(filter), networkDeviceId, "attributes", attributeId, "samples").ConfigureAwait(false);
            // Read full attribute from url
            foreach (JToken s in response.Items)
            {
                Sample sample = new Sample();
                string unitsUrl = string.Empty;
                try
                {
                    sample.Timestamp = s["timestamp"].Value<DateTime>();
                    sample.IsReliable = s["isReliable"].Value<Boolean>();
                    sample.Value = s["value"]["value"].Value<double>();
                    unitsUrl = s["value"]["units"].Value<string>();
                    sample.Unit = ResourceManager.Localize(unitsUrl, _CultureInfo);
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
                if (Version < ApiVersion.v3)
                {
                    // On Api v2 and v1 there was the url endpoint of the enum instead of the fully qualified enumeration string
                    var unitId = unitsUrl.Split('/').Last();
                    // Read full url if not cached previously
                    if (!Units.ContainsKey(unitId))
                    {
                        var unit = await GetWithFullUrl(unitsUrl).ConfigureAwait(false);
                        Units.Add(unitId, unit["description"].Value<string>());
                    }
                    sample.Unit = Units[unitId];
                }
                objectSamples.Add(sample);
            }
            // Type the response as Sample List
            return new PagedResult<Sample>
            {
                Items = objectSamples,
                CurrentPage = response.CurrentPage,
                PageCount = response.PageCount,
                PageSize = response.PageSize,
                Total = response.Total
            };
        }
        /// <inheritdoc/>
        public PagedResult<Sample> GetNetDevSamples(ObjectId networkDeviceId, AttributeEnumSet attributeName, TimeFilter filter)
        {
            if (Version > ApiVersion.v3)
            {
                return GetNetDevSamplesAsync(networkDeviceId, attributeName, filter).GetAwaiter().GetResult();
            }
            else
            {
                return GetNetDevSamplesAsync(networkDeviceId, (int)attributeName, filter).GetAwaiter().GetResult();
            }
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Sample>> GetNetDevSamplesAsync(ObjectId networkDeviceId, AttributeEnumSet attributeName, TimeFilter filter)
        {
            //Note: this method is valid for API version > v3
            if (Version < ApiVersion.v4) { throw new MetasysUnsupportedApiVersion(Version.ToString()); }

            List<Sample> objectSamples = new List<Sample>();
            // Perform a generic call using objects resource valid for Network Devices as well
            var response = await GetPagedResultsAsync<JToken>("networkDevices", ToDictionary(filter), networkDeviceId, "trendedAttributes", attributeName.ToString(), "samples").ConfigureAwait(false);
            // Read full attribute from url
            foreach (JToken s in response.Items)
            {
                Sample sample = new Sample();
                string unitsUrl = string.Empty;
                try
                {
                    sample.Timestamp = s["timestamp"].Value<DateTime>();
                    sample.IsReliable = s["isReliable"].Value<Boolean>();
                    sample.Value = s["value"]["value"].Value<double>();
                    unitsUrl = s["value"]["units"].Value<string>();
                    sample.Unit = ResourceManager.Localize(unitsUrl, _CultureInfo);
                }
                catch (ArgumentNullException e)
                {
                    // Something went wrong on object parsing
                    throw new MetasysObjectException(e);
                }
                if (Version < ApiVersion.v3)
                {
                    // On Api v2 and v1 there was the url endpoint of the enum instead of the fully qualified enumeration string
                    var unitId = unitsUrl.Split('/').Last();
                    // Read full url if not cached previously
                    if (!Units.ContainsKey(unitId))
                    {
                        var unit = await GetWithFullUrl(unitsUrl).ConfigureAwait(false);
                        Units.Add(unitId, unit["description"].Value<string>());
                    }
                    sample.Unit = Units[unitId];
                }
                objectSamples.Add(sample);
            }
            // Type the response as Sample List
            return new PagedResult<Sample>
            {
                Items = objectSamples,
                CurrentPage = response.CurrentPage,
                PageCount = response.PageCount,
                PageSize = response.PageSize,
                Total = response.Total
            };
        }

    }
}
