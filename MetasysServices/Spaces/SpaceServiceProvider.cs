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
    /// Provide space item for the endpoints of the Metasys Spaces API.
    /// </summary>
    public sealed class SpaceServiceProvider : BasicServiceProvider, ISpaceService
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NetworkDeviceServiceProvider"/> with supplied data.
        /// </summary>
        /// <param name="client">The FlurlClient to get response from URL.</param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="logClientErrors">Set this flag to false to disable logging of client errors.</param>
        public SpaceServiceProvider(IFlurlClient client, ApiVersion version, bool logClientErrors = true) : base(client, version, logClientErrors)
        {
        }

        // FindById -------------------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public MetasysObject FindById(ObjectId spaceId)
        {
            return FindByIdAsync(spaceId).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<MetasysObject> FindByIdAsync(ObjectId spaceId)
        {
            CheckVersion(Version);
            var response = await GetRequestAsync("spaces", null, spaceId).ConfigureAwait(false);
            return ToMetasysObject(response, Version, MetasysObjectTypeEnum.Space);
        }


        // Get ---------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> Get(SpaceTypeEnum? type = null, int? page = null, int? pageSize = null, string sort = null)
        {
            return GetAsync(type, page, pageSize, sort).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetAsync(SpaceTypeEnum? type = null, int? page = null, int? pageSize = null, string sort = null)
        {
            CheckVersion(Version);
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //Add parameters
            if (type != null) parameters.Add("type", ((int)type).ToString());
            if (page != null && page > 0) parameters.Add("page", page.ToString());
            if (pageSize != null && pageSize > 0 && pageSize <= 1000) parameters.Add("pageSize", pageSize.ToString());
            if (sort != null) parameters.Add("sort", sort);

            var spaces = await GetAllAvailablePagesAsync("spaces", parameters).ConfigureAwait(false);
            return ToMetasysObject(spaces, Version, type: MetasysObjectTypeEnum.Space);
        }

        /// <inheritdoc/>
        public IEnumerable<MetasysObject> Get(string type, int? page = null, int? pageSize = null, string sort = null)
        {
            return GetAsync(type, page, pageSize, sort).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetAsync(string type, int? page = null, int? pageSize = null, string sort = null)
        {
            CheckVersion(Version);
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            //Add parameters
            if (type != null) parameters.Add("type", type);
            if (page != null && page > 0 && parameters != null) parameters.Add("page", page.ToString());
            if (pageSize != null && pageSize > 0 && parameters != null) parameters.Add("pageSize", pageSize.ToString());
            if (sort != null) parameters.Add("sort", sort);

            var spaces = await GetAllAvailablePagesAsync("spaces", parameters).ConfigureAwait(false);
            return ToMetasysObject(spaces, Version, type: MetasysObjectTypeEnum.Space);
        }

        // GetChildren ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetChildren(ObjectId spaceId)
        {
            return GetChildrenAsync(spaceId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetChildrenAsync(ObjectId spaceId)
        {
            CheckVersion(Version);
            var spaceChildren = await GetAllAvailablePagesAsync("spaces", null, spaceId.ToString(), "spaces").ConfigureAwait(false);
            return ToMetasysObject(spaceChildren, Version, MetasysObjectTypeEnum.Space);
        }

        // GetTypes ---------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObjectType> GetTypes()
        {
            return GetTypesAsync().GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObjectType>> GetTypesAsync()
        {
            if (Version < ApiVersion.v4)
            {
                return await GetResourceTypesAsync("enumSets", "1766/members").ConfigureAwait(false);
            }
            else
            {
                return await GetResourceTypesAsync("enumerations", "spaceTypesEnumSet").ConfigureAwait(false);
            }
        }

        // GetServedByEquipment ------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetServedByEquipment(ObjectId equipmentId)
        {
            return GetServedByEquipmentAsync(equipmentId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetServedByEquipmentAsync(ObjectId equipmentId)
        {
            CheckVersion(Version);
            var response = await GetAllAvailablePagesAsync("equipment", null, equipmentId.ToString(), "spaces").ConfigureAwait(false);
            return ToMetasysObject(response, Version, MetasysObjectTypeEnum.Space);
        }

        // GetServedByNetworkDevice ------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetServedByNetworkDevice(ObjectId networkDeviceId)
        {
            return GetServedByNetworkDeviceAsync(networkDeviceId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetServedByNetworkDeviceAsync(ObjectId networkDeviceId)
        {
            CheckVersion(Version);
            var response = await GetAllAvailablePagesAsync("networkDevices", null, networkDeviceId.ToString(), "spaces").ConfigureAwait(false);
            return ToMetasysObject(response, Version, MetasysObjectTypeEnum.Space);
        }

    }
}
