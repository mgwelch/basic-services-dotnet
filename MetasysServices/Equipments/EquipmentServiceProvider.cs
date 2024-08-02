using Flurl;
using Flurl.Http;
using JohnsonControls.Metasys.BasicServices.Utils;
using log4net.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Provide equipment item for the endpoints of the Metasys Equipments API.
    /// </summary>
    public sealed class EquipmentServiceProvider : BasicServiceProvider, IEquipmentService
    {
        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");

        /// <summary>
        /// Initializes a new instance of <see cref="NetworkDeviceServiceProvider"/> with supplied data.
        /// </summary>
        /// <param name="client">The FlurlClient to get response from URL.</param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="logClientErrors">Set this flag to false to disable logging of client errors.</param>
        public EquipmentServiceProvider(IFlurlClient client, ApiVersion version, bool logClientErrors = true) : base(client, version, logClientErrors)
        {
        }

        // FindById -----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public MetasysObject FindById(ObjectId equipmentId)
        {
            return FindByIdAsync(equipmentId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<MetasysObject> FindByIdAsync(ObjectId equipmentId)
        {
            CheckVersion(Version);

            var response = await GetRequestAsync("equipment", null, equipmentId).ConfigureAwait(false);
            return ToMetasysObject(response, Version, MetasysObjectTypeEnum.Equipment);
        }

        // Get ---------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> Get(int? page = null, int? pageSize = null)
        {
            return GetAsync(page, pageSize).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetAsync(int? page = null, int? pageSize = null)
        {
            CheckVersion(Version);

            Dictionary<string, string> parameters = null;
            if ((page != null && page > 0) | (pageSize != null && pageSize > 0)) parameters = new Dictionary<string, string>();

            if (page != null && page > 0 && parameters != null) parameters.Add("page", page.ToString());
            if (pageSize != null && pageSize > 0 && parameters != null) parameters.Add("pageSize", pageSize.ToString());

            var equipment = await GetAllAvailablePagesAsync("equipment", parameters).ConfigureAwait(false);
            return ToMetasysObject(equipment, Version, MetasysObjectTypeEnum.Equipment);
        }

        // GetPoints -------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysPoint> GetPoints(ObjectId equipmentId, bool readAttributeValue = true)
        {
            return GetPointsAsync(equipmentId, readAttributeValue).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysPoint>> GetPointsAsync(ObjectId equipmentId, bool readAttributeValue = true)
        {
            CheckVersion(Version);

            List<MetasysPoint> points = new List<MetasysPoint>() { }; List<Guid> guids = new List<Guid>();
            List<MetasysPoint> pointsWithAttribute = new List<MetasysPoint>() { };
            var response = await GetAllAvailablePagesAsync("equipment", null, equipmentId.ToString(), "points").ConfigureAwait(false);
            try
            {
                foreach (var item in response)
                {
                    MetasysPoint point = new MetasysPoint(item);
                    // Retrieve object Id from full URL
                    string objectId = point.ObjectUrl.Split('/').Last();
                    point.ObjectId = ParseObjectIdentifier(objectId);
                    // Retrieve attribute Id from full URL
                    string attributeId = (point.Attribute != null) ? point.Attribute.Split('.').Last() : String.Empty;
                    if (attributeId == String.Empty)
                    {
                        attributeId = (point.AttributeUrl != null) ? point.AttributeUrl.Split('/').Last() : String.Empty;
                    }
                    if (point.ObjectId != Guid.Empty) // Sometime can happen that there are empty Guids.
                    {
                        // Collect Guids to perform read property multiple in "one call" (supporting only presentValue so far)
                        if ((attributeId == "85" | attributeId == "presentValue") && readAttributeValue)
                        {
                            JToken resp = await Client.Request(new Url("objects")
                                                .AppendPathSegments(point.ObjectId, "attributes", "presentValue"))
                                                .GetJsonAsync<JToken>()
                                                .ConfigureAwait(false);
                            Variant result = new Variant(point.ObjectId, resp, "presentValue", Culture, Version);
                            point.PresentValue = result;
                            points.Add(point);
                        }
                        else
                        {
                            points.Add(point);
                        }
                    }
                }
            }
            catch (System.NullReferenceException e)
            {
                throw new MetasysHttpParsingException(response.ToString(), e);
            }
            return points;
        }
        // GetServingASpace --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetServingASpace(ObjectId spaceId)
        {
            return GetServingASpaceAsync(spaceId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetServingASpaceAsync(ObjectId spaceId)
        {
            CheckVersion(Version);

            var spaceEquipment = await GetAllAvailablePagesAsync("spaces", null, spaceId.ToString(), "equipment").ConfigureAwait(false);
            return ToMetasysObject(spaceEquipment, Version, MetasysObjectTypeEnum.Equipment);
        }

        // GetHostedByNetworkDevice --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetHostedByNetworkDevice(ObjectId networkDeviceId)
        {
            return GetHostedByNetworkDeviceAsync(networkDeviceId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetHostedByNetworkDeviceAsync(ObjectId networkDeviceId)
        {
            CheckVersion(Version);

            var spaceEquipment = await GetAllAvailablePagesAsync("networkDevices", null, networkDeviceId.ToString(), "equipment").ConfigureAwait(false);
            return ToMetasysObject(spaceEquipment, Version, MetasysObjectTypeEnum.Equipment);
        }

        // GetServedByEquipment -----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetServedByEquipment(ObjectId equipmentId)
        {
            return GetServedByEquipmentAsync(equipmentId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetServedByEquipmentAsync(ObjectId equipmentId)
        {
            CheckVersion(Version);

            var response = await GetAllAvailablePagesAsync("equipment", null, equipmentId.ToString(), "equipment").ConfigureAwait(false);
            return ToMetasysObject(response, Version, MetasysObjectTypeEnum.Equipment);
        }

        // GetServingAnEquipment --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetServingAnEquipment(ObjectId equipmentId)
        {
            return GetServingAnEquipmentAsync(equipmentId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetServingAnEquipmentAsync(ObjectId equipmentId)
        {
            CheckVersion(Version);

            var spaceEquipment = await GetAllAvailablePagesAsync("equipment", null, equipmentId.ToString(), "upstreamEquipment").ConfigureAwait(false);
            return ToMetasysObject(spaceEquipment, Version, MetasysObjectTypeEnum.Equipment);
        }

    }
}
