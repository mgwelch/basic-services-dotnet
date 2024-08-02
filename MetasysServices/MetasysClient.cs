using Flurl;
using Flurl.Http;
using JohnsonControls.Metasys.BasicServices.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// An HTTP client for consuming the most commonly used endpoints of the Metasys API.
    /// </summary>
    public class MetasysClient : BasicServiceProvider, IMetasysClient
    {

        /// <summary>The flag used to control automatic session refreshing.</summary>
        protected bool RefreshToken;

        /// <summary>
        /// Stores retrieved Ids and serves as an in-memory caching layer.
        /// </summary>
        protected Dictionary<string, ObjectId> IdentifiersDictionary = new Dictionary<string, ObjectId>();

        /// <inheritdoc/>
        public IActivityService Activities { get; set; }

        /// <inheritdoc/>
		public IAlarmsService Alarms { get; set; }

        /// <inheritdoc/>
        public IAuditService Audits { get; set; }

        /// <inheritdoc/>
        public IEnumerationService Enumerations { get; set; }

        /// <inheritdoc/>
        public IEquipmentService Equipments { get; set; }

        /// <inheritdoc/>
        public INetworkDeviceService NetworkDevices { get; set; }

        /// <inheritdoc/>
        public ISpaceService Spaces { get; set; }

        /// <inheritdoc/>
        public IStreamService Streams { get; set; }

        /// <inheritdoc/>
        public ITrendService Trends { get; set; }

        private string hostname;
        /// <inheritdoc/>
        public string Hostname
        {
            get
            {
                return hostname;
            }
            set
            {
                // No need to init base url on first call (already done on Version set)
                if (hostname != null && hostname != value) // it's the same hostname: no changes
                {
                    // reset the base client according to settings
                    InitFlurlClient(value);
                }
                hostname = value;
            }
        }

        private int timeout = 300;
        /// <inheritdoc/>
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }


        private ApiVersion? version;

        private System.Timers.Timer _timer;

        private DateTime RefreshDateTime;

        /// <inheritdoc/>
        public new ApiVersion Version
        {
            get
            {
                return version.Value;
            }
            set
            {
                // When version is null we need to do the first init
                if (version != null && version == value)
                {
                    return; // it's the same version: no changes
                }
                version = value;
                // set base url and all related services to the new value
                InitFlurlClient(Hostname);
                if (Activities != null) { Activities.Version = version.Value; }
                if (Alarms != null) { Alarms.Version = version.Value; }
                if (Audits != null) { Audits.Version = version.Value; }
                if (Enumerations != null) { Enumerations.Version = version.Value; }
                if (Equipments != null) { Equipments.Version = version.Value; }
                if (NetworkDevices != null) { NetworkDevices.Version = version.Value; }
                if (Spaces != null) { Spaces.Version = version.Value; }
                if (Streams != null) { Streams.Version = version.Value; }
                if (Trends != null) { Trends.Version = version.Value; }
            }
        }

        /// <summary>The current session token.</summary>
        protected AccessToken AccessToken;

        /// <summary>
        /// An optional boolean flag for ignoring certificate errors by bypassing certificate verification steps.
        /// </summary>
        protected bool IgnoreCertificateErrors { get; set; }

        /// <summary>The current Culture Used for localization.</summary>
        private CultureInfo culture;

        /// <summary>
        /// The current Culture Used for localization.
        /// </summary>
        public new CultureInfo Culture
        {
            get
            {
                return culture;
            }
            set
            {
                // When version is null we need to do the first init
                if (culture != null && culture == value)
                {
                    return; // it's the same version: no changes
                }
                culture = value;
                // set all related services to the new value
                if (Activities != null) { Activities.Culture = culture; }
                if (Alarms != null) { Alarms.Culture = culture; }
                if (Audits != null) { Audits.Culture = culture; }
                if (Enumerations != null) { Enumerations.Culture = culture; }
                if (Equipments != null) { Equipments.Culture = culture; }
                if (NetworkDevices != null) { NetworkDevices.Culture = culture; }
                if (Spaces != null) { Spaces.Culture = culture; }
                if (Streams != null) { Streams.Culture = culture; }
                if (Trends != null) { Trends.Culture = culture; }
            }
        }

        /// <summary>
        /// Initialize the HTTP client with a base URL.
        /// </summary>
        protected void InitFlurlClient(string hostname)
        {
            if (IgnoreCertificateErrors)
            {
                HttpClientHandler httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                HttpClient httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri($"https://{hostname}"
                    .AppendPathSegments("api", Version)),
                    Timeout = TimeSpan.FromSeconds(timeout)
                };

                FlurlHttp.Configure(settings => settings.Timeout = TimeSpan.FromSeconds(timeout));
                Client = new FlurlClient(httpClient);
            }
            else
            {
                FlurlHttp.Configure(settings => settings.Timeout = TimeSpan.FromSeconds(timeout));
                Client = new FlurlClient($"https://{hostname}"
                    .AppendPathSegments("api", Version));
            }
            // reset Access Token
            AccessToken = null;
        }

        /// <summary>
        /// Creates a new MetasysClient.
        /// </summary>
        /// <remarks>
        /// Takes an optional boolean flag for ignoring certificate errors by bypassing certificate verification steps.
        /// Verification bypass is not recommended due to security concerns. If your Metasys server is missing a valid certificate it is
        /// recommended to add one to protect your private data from attacks over external networks.
        /// Takes an optional flag for the api version of your Metasys server.
        /// Takes an optional CultureInfo which is useful for formatting numbers and localization of strings. If not specified,
        /// the machine's current culture is used.
        /// Takes an optional flag for logging client errors. If not specified, it is enabled by default according to log4Net.config file.
        /// </remarks>
        /// <param name="hostname">The hostname of the Metasys server.</param>
        /// <param name="ignoreCertificateErrors">Use to bypass server certificate verification.</param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="cultureInfo">Localization culture for Metasys enumeration translations.</param>
        /// <param name="logClientErrors">Set this flag to false to disable logging of client errors.</param>
        /// <param name="timeout">Set the Timeout (in seconds) of the https request.</param>
        public MetasysClient(string hostname, bool ignoreCertificateErrors = false, ApiVersion version = ApiVersion.v2, CultureInfo cultureInfo = null, bool logClientErrors = true, int timeout = 300)
        {
            try
            {
                IgnoreCertificateErrors = ignoreCertificateErrors;
                Hostname = hostname;
                Timeout = timeout;
                // Set Metasys culture if specified, otherwise use current machine Culture.
                Culture = cultureInfo ?? CultureInfo.CurrentCulture;
                // Set preferences about logging
                LogClientErrors = logClientErrors;
                Version = version;
                // Init related services
                Activities = new ActivityServiceProvider(Client, version, logClientErrors);
                Alarms = new AlarmServiceProvider(Client, version, logClientErrors);
                Audits = new AuditServiceProvider(Client, version, logClientErrors);
                Enumerations = new EnumerationServiceProvider(Client, version, logClientErrors);
                Equipments = new EquipmentServiceProvider(Client, version, logClientErrors);
                NetworkDevices = new NetworkDeviceServiceProvider(Client, version, logClientErrors);
                Spaces = new SpaceServiceProvider(Client, version, logClientErrors);
                Trends = new TrendServiceProvider(Client, version, logClientErrors);
                if (Version > ApiVersion.v3) Streams = new StreamServiceProvider(Client, version, logClientErrors);


                base.Version = version;
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
        }

        #region "LOGIN" // ==========================================================================================================
        // TryLogin -------------------------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public AccessToken TryLogin(string username, string password, bool refresh = true)
        {
            return TryLoginAsync(username, password, true).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<AccessToken> TryLoginAsync(string username, string password, bool refresh = true)
        {
            try
            {
                var response = await Client.Request("login")
                    .PostJsonAsync(new { username, password })
                    .ReceiveJson<JToken>()
                    .ConfigureAwait(false);

                this.RefreshToken = true;

                CreateAccessToken(Hostname, username, response);
                if (Streams != null)
                {
                    Streams.AccessToken = this.AccessToken; ;
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return this.AccessToken;
        }

        // TryLogin (2) -------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public virtual AccessToken TryLogin(string credManTarget, bool refresh = true)
        {
            return TryLoginAsync(credManTarget, true).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public virtual async Task<AccessToken> TryLoginAsync(string credManTarget, bool refresh = true)
        {
            // Retrieve credentials first
            var credentials = CredentialUtil.GetCredential(credManTarget);
            // Get the control back to TryLogin method
            return await TryLoginAsync(CredentialUtil.convertToUnSecureString(credentials.Username), CredentialUtil.convertToUnSecureString(credentials.Password), true).ConfigureAwait(false);
        }

        // GetAccessToken -----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public AccessToken GetAccessToken()
        {
            return this.AccessToken;
        }

        //// SetAccessToken -----------------------------------------------------------------------------------------------------------
        ///// <inheritdoc/>
        //public void SetAccessToken(AccessToken accessToken)
        //{
        //    this.AccessToken = accessToken;
        //}

        // Refresh ------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public AccessToken Refresh()
        {
            return RefreshAsync().GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<AccessToken> RefreshAsync()
        {
            try
            {
                var response = await Client.Request("refreshToken")
                                                    .GetJsonAsync<JToken>()
                                                    .ConfigureAwait(false);
                // Since it's a refresh, get issue info from the current token
                CreateAccessToken(AccessToken.Issuer, AccessToken.IssuedTo, response);
                // Set the new value of the Token to the StreamClient
                if (Streams != null)
                {
                    Streams.AccessToken = this.AccessToken;
                    _ = Streams.KeepAlive(this.AccessToken);
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return this.AccessToken;
        }

        // Refresh2 ------------------------------------------------------------------------------------------------------------------

        private AccessToken Refresh2()
        {
            return Refresh2Async().GetAwaiter().GetResult();
        }

        private async Task<AccessToken> Refresh2Async()
        {
            try
            {
                var response = await Client.Request("refreshToken")
                                                    .GetJsonAsync<JToken>()
                                                    .ConfigureAwait(false);

                var accessTokenValue = response["accessToken"];
                var expires = response["expires"];
                var accessToken = $"Bearer {accessTokenValue.Value<string>()}";
                var date = expires.Value<DateTime>();
                this.AccessToken = new AccessToken(AccessToken.Issuer, AccessToken.IssuedTo, accessToken, date);
                Client.Headers.Remove("Authorization");
                Client.Headers.Add("Authorization", this.AccessToken.Token);

                DateTime now = DateTime.UtcNow;
                TimeSpan lifePeriod = (AccessToken.Expires - now);
                TimeSpan halfLifePeriod = new TimeSpan(lifePeriod.Ticks / 2);
                DateTime halfLife = now.Add(halfLifePeriod);
                this.RefreshDateTime = halfLife;


                // Set the new value of the Token to the StreamClient
                if (Streams != null)
                {
                    Streams.AccessToken = this.AccessToken;
                    _ = Streams.KeepAlive(this.AccessToken);
                }

            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return this.AccessToken;
        }


        #endregion


        #region "ALARMS" // =========================================================================================================
        // The corresponding methods are defined in 'AlarmServiceProvider'
        #endregion=


        #region "AUDITS" // =========================================================================================================
        // The corresponding methods are defined in 'AuditServiceProvider'
        #endregion


        #region "ENUMERATIONS" // ===================================================================================================
        // The corresponding methods are defined in 'EnumerationServiceProvider'
        #endregion


        #region "EQUIPMENTS" // =====================================================================================================
        // note: these methods are deprecated and kept only for backward compatibility with previous SDK version

        // GetEquipment -------------------------------------------------------------------------------------------------------------
        // Retrieves a collection of equipment instances.
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetEquipment()
        {
            return Equipments.Get();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetEquipmentAsync()
        {
            return await Equipments.GetAsync();
        }

        // GetEquipmentPoints -------------------------------------------------------------------------------------------------------
        // Retrieves the collection of points that are defined by the specified equipment instance
        /// <inheritdoc/>
        public IEnumerable<MetasysPoint> GetEquipmentPoints(Guid equipmentId, bool readAttributeValue = true)
        {
            return Equipments.GetPoints(equipmentId, readAttributeValue);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysPoint>> GetEquipmentPointsAsync(Guid equipmentId, bool readAttributeValue = true)
        {
            return await Equipments.GetPointsAsync(equipmentId, readAttributeValue);
        }

        // GetSpaceEquipment --------------------------------------------------------------------------------------------------------
        // Retrieves the collection of equipment that serve the specified space.
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetSpaceEquipment(Guid spaceId)
        {
            return Equipments.GetServingASpace(spaceId);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetSpaceEquipmentAsync(Guid spaceId)
        {
            return await Equipments.GetServingASpaceAsync(spaceId);
        }
        #endregion


        #region "NETWORK DEVICES" // ================================================================================================
        // note: these methods are deprecated and kept only for backward compatibility with previous SDK version

        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetNetworkDevices(string type = null)
        {
            return NetworkDevices.Get(type);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetNetworkDevicesAsync(string type = null)
        {
            return await NetworkDevices.GetAsync(type);
        }

        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetNetworkDevices(NetworkDeviceTypeEnum networkDevicetype)
        {
            return NetworkDevices.Get(networkDevicetype);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetNetworkDevicesAsync(NetworkDeviceTypeEnum networkDevicetype)
        {
            return await NetworkDevices.GetAsync(networkDevicetype);
        }

        /// <inheritdoc/>
        public IEnumerable<MetasysObjectType> GetNetworkDeviceTypes()
        {
            return NetworkDevices.GetTypes();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObjectType>> GetNetworkDeviceTypesAsync()
        {
            return await NetworkDevices.GetTypesAsync();
        }
        #endregion


        #region "OBJECTS" // ========================================================================================================

        // GetObjects ---------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetObjects(ObjectId id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false)
        {
            return GetObjectsAsync(id, levels, includeInternalObjects, includeExtensions).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetObjectsAsync(ObjectId id, int levels, bool includeInternalObjects = false, bool includeExtensions = false)
        {
            Dictionary<string, string> parameters = null;
            if (Version == ApiVersion.v3)
            {
                // Since API v3 we could use the includeInternalObjects parameter
                parameters = new Dictionary<string, string>
                {
                    { "includeInternalObjects", includeInternalObjects.ToString() }
                };
            }
            if (Version > ApiVersion.v3)
            {
                // Since API v3 we could use the includeInternalObjects parameter
                parameters = new Dictionary<string, string>
                {
                    { "depth", levels.ToString() },
                    { "flatten", "true".ToString() }, //This parameter is needed to get the data in a 'flat' way and keep consistency in the logic to retrieve the objects
                    { "includeExtensions", includeExtensions.ToString() },
                    { "includeInternal", includeInternalObjects.ToString() } //This param has different name when version > v3
                };
            }

            var objects = await GetObjectChildrenAsync(id, parameters, levels).ConfigureAwait(false);
            if (Version > ApiVersion.v3 && objects.Count > 0)
            {
                //Due to in this case the API returns also the parent object then remove it
                objects.Remove(objects.First());
            }
            return ToMetasysObject(objects, Version);
        }

        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetObjects(ObjectId id, string objectType)
        {
            return GetObjectsAsync(id, objectType).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetObjectsAsync(ObjectId objectId, string objectType)
        {
            Dictionary<string, string> parameters = null;

            if (Version > ApiVersion.v3)
            {
                parameters = new Dictionary<string, string>
                {
                    { "flatten", "true".ToString() },
                    { "includeExtensions", "true".ToString() },
                    { "depth", "-1".ToString() },
                    { "objectType", objectType }
                };
            }

            var objects = await GetObjectChildrenAsync(objectId, parameters).ConfigureAwait(false);

            return ToMetasysObject(objects, Version);
        }

        // GetObjectIdentifier ------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public ObjectId GetObjectIdentifier(string itemReference)
        {
            return GetObjectIdentifierAsync(itemReference).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<ObjectId> GetObjectIdentifierAsync(string itemReference)
        {
            // Sanitize given itemReference
            var normalizedItemReference = itemReference.Trim().ToUpper();
            // Returns cached value when available, otherwise perform request
            if (!IdentifiersDictionary.ContainsKey(normalizedItemReference))
            {
                JToken response = null;
                try
                {
                    response = await Client.Request("objectIdentifiers")
                        .SetQueryParam("fqr", itemReference)
                        .GetJsonAsync<JToken>()
                        .ConfigureAwait(false);
                    // Stores value for caching and return
                    IdentifiersDictionary[normalizedItemReference] = ParseObjectIdentifier(response);
                }
                catch (FlurlHttpException e)
                {
                    if (e.Call.HttpStatus == HttpStatusCode.BadRequest && response != null && response["message"] != null
                        && response["message"].Value<string>().Contains("not found"))
                    {
                        // Metasys respond with "Identifier is not found." message, so make exception explicit
                        throw new MetasysHttpNotFoundException(e);
                    }
                    ThrowHttpException(e);
                }
            }
            return IdentifiersDictionary[normalizedItemReference];
        }

        // GetCommands --------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<Command> GetCommands(ObjectId id)
        {
            return GetCommandsAsync(id).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<Command>> GetCommandsAsync(ObjectId id)
        {
            try
            {
                var token = await Client.Request(new Url("objects")
                    .AppendPathSegments(id, "commands"))
                    .GetJsonAsync<JToken>()
                    .ConfigureAwait(false);

                if (!(token is JArray) && token["items"] != null)
                {
                    // Since API v3 response is wrapped in items property
                    token = token["items"];
                }
                List<Command> commands = new List<Command>();

                if (token is JArray array)
                {
                    foreach (JObject command in array.Cast<JObject>())
                    {
                        try
                        {
                            Command c = new Command(command, Culture, Version);
                            commands.Add(c);
                        }
                        catch (Exception e)
                        {
                            throw new MetasysCommandException(command.ToString(), e);
                        }
                    }
                }

                return commands;
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return null;
        }

        // GetObjectTypeEnumeration -------------------------------------------------------------------------------------------------
        // note: this method has been moved to 'BasicServiceProvider'
        ///// <inheritdoc/>
        //public string GetObjectTypeEnumeration(string resource)
        //{
        //    // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
        //    return Utils.ResourceManager.GetObjectTypeEnumeration(resource);
        //}

        // GetCommandEnumeration ----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public string GetCommandEnumeration(string resource)
        {
            // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
            return Utils.ResourceManager.GetCommandEnumeration(resource);
        }

        // ReadProperty -------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public Variant ReadProperty(ObjectId id, string attributeName)
        {
            return ReadPropertyAsync(id, attributeName).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<Variant> ReadPropertyAsync(ObjectId id, string attributeName)
        {
            JToken response = null; Variant result = new Variant();
            try
            {
                response = await Client.Request(new Url("objects")
                    .AppendPathSegments(id, "attributes", attributeName))
                    .GetJsonAsync<JToken>()
                    .ConfigureAwait(false);
                result = new Variant(id, response, attributeName, Culture, Version);
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            catch (System.NullReferenceException e)
            {
                throw new MetasysPropertyException(response.ToString(), e);
            }
            return result;
        }

        // ReadPropertyMultiple -----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<VariantMultiple> ReadPropertyMultiple(IEnumerable<ObjectId> ids, IEnumerable<string> attributeNames)
        {
            return ReadPropertyMultipleAsync(ids, attributeNames).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<VariantMultiple>> ReadPropertyMultipleAsync(IEnumerable<ObjectId> ids, IEnumerable<string> attributeNames)
        {
            if (ids == null || attributeNames == null)
            {
                return null;
            }
            List<VariantMultiple> results = new List<VariantMultiple>();
            if (Version > ApiVersion.v2)
            {
                var response = await GetBatchRequestAsync("objects", ids, attributeNames, "attributes").ConfigureAwait(false);
                return ToVariantMultiples(response);
            }
            var taskList = new List<Task<Variant>>();
            // Prepare Tasks to Read attributes list. In Metasys 11 this will be implemented server side.
            foreach (var id in ids)
            {
                foreach (string attributeName in attributeNames)
                {
                    // Much faster reading single property than the entire object, even though we have more server calls.
                    taskList.Add(ReadPropertyAsync(id, attributeName, true)); // Using internal signature with exception suppress.
                }
            }
            await Task.WhenAll(taskList).ConfigureAwait(false);
            foreach (var id in ids)
            {
                // Get attributes of the specific Id
                List<Task<Variant>> attributeList = taskList.Where(w =>
                    (w.Result != null && w.Result.Id == id)).ToList();
                List<Variant> variants = new List<Variant>();
                foreach (var t in attributeList)
                {
                    if (t.Result != null) // Something went wrong if the result is unknown.
                    {
                        variants.Add(t.Result); // Prepare variants list
                    }
                }
                if (variants.Count > 0 || attributeNames.Count() == 0)
                {
                    // Aggregate results only when objects was found or no attributes specified.
                    results.Add(new VariantMultiple(id, variants));
                }
            }
            return results.AsEnumerable();
        }

        // WriteProperty ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void WriteProperty(ObjectId id, string attributeName, object newValue)
        {
            WritePropertyAsync(id, attributeName, newValue).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task WritePropertyAsync(ObjectId id, string attributeName, object newValue)
        {
            List<(string Attribute, object Value)> list = new List<(string Attribute, object Value)>
            {
                (Attribute: attributeName, Value: newValue)
            };
            var item = GetWritePropertyBody(list);
            await WritePropertyRequestAsync(id, item).ConfigureAwait(false);
        }

        // WritePropertyMultiple ----------------------------------------------------------------------------------------------------
        ///<inheritdoc/>
        public void WritePropertyMultiple(IEnumerable<ObjectId> ids, Dictionary<string, object> attributeValues)
        {
            WritePropertyMultipleAsync(ids, attributeValues).GetAwaiter().GetResult();
        }
        ///<inheritdoc/>
        public async Task WritePropertyMultipleAsync(IEnumerable<ObjectId> ids, Dictionary<string, object> attributeValues)
        {
            if (ids == null || attributeValues == null)
            {
                throw new ArgumentNullException("ids and/or attributeValues.");
            }
            // convert dictionary to a list of tuples and use existing overload
            await WritePropertyMultipleAsync(ids, attributeValues.Select(x => (x.Key, x.Value))).ConfigureAwait(false);
        }

        // WritePropertyMultiple (2) ------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void WritePropertyMultiple(IEnumerable<ObjectId> ids, IEnumerable<(string Attribute, object Value)> attributeValues)
        {
            WritePropertyMultipleAsync(ids, attributeValues).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task WritePropertyMultipleAsync(IEnumerable<ObjectId> ids, IEnumerable<(string Attribute, object Value)> attributeValues)
        {
            if (ids == null || attributeValues == null)
            {
                throw new ArgumentNullException("ids and/or attributeValues.");
            }

            var item = GetWritePropertyBody(attributeValues);
            var taskList = new List<Task>();

            foreach (var id in ids)
            {
                taskList.Add(WritePropertyRequestAsync(id, item));
            }
            await Task.WhenAll(taskList).ConfigureAwait(false);
        }

        // SendCommand --------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void SendCommand(ObjectId id, string command, IEnumerable<object> values = null)
        {
            SendCommandAsync(id, command, values).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task SendCommandAsync(ObjectId id, string command, IEnumerable<object> values = null)
        {
            if (values == null)
            {
                await SendCommandRequestAsync(id, command, new string[0]).ConfigureAwait(false);
            }
            else
            {
                await SendCommandRequestAsync(id, command, values).ConfigureAwait(false);
            }
        }
        #endregion


        #region "SPACES" //==========================================================================================================
        // note: these methods are deprecated and kept only for backward compatibility with previous SDK version

        // GetSpaces ----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetSpaces(SpaceTypeEnum? type = null)
        {
            return Spaces.Get(type);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetSpacesAsync(SpaceTypeEnum? type = null)
        {
            return await Spaces.GetAsync(type);
        }

        // GetSpaceChildren --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObject> GetSpaceChildren(Guid spaceId)
        {
            return Spaces.GetChildren(spaceId);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObject>> GetSpaceChildrenAsync(Guid spaceId)
        {
            return await Spaces.GetChildrenAsync(spaceId);
        }

        // GetSpaceTypes ----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IEnumerable<MetasysObjectType> GetSpaceTypes()
        {
            return Spaces.GetTypes();
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<MetasysObjectType>> GetSpaceTypesAsync()
        {
            return await Spaces.GetTypesAsync();
        }
        #endregion //==============================================================================================================


        #region "TRENDS" // =========================================================================================================
        // The corresponding methods are defined in 'TrendServiceProvider'
        #endregion


        #region "MISCELLANEA" // ====================================================================================================
        // GetServerTime ------------------------------------------------------------------------------------------------------------
        ///<inheritdoc/>
        public DateTime GetServerTime()
        {
            return GetServerTimeAsync().GetAwaiter().GetResult();
        }
        ///<inheritdoc/>
        public async Task<DateTime> GetServerTimeAsync()
        {
            // Using the RefreshToken call to read the HTTP header
            DateTime? serverTime = null;
            try
            {
                HttpResponseMessage response = await Client.Request("refreshToken").GetAsync().ConfigureAwait(false);
                var date = response.Headers.Date ?? throw new MetasysHttpException("Cannot read date time from HTTP response of Metasys Server.", response.ToString());
                serverTime = date.UtcDateTime;
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return serverTime.Value;
        }

        #endregion

        /// <summary>
        /// Creates a new AccessToken from a JToken and sets the client's authorization header if successful.
        /// On failure leaves the AccessToken and authorization header in previous state.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="issuer"></param>
        /// <param name="issuedTo"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysTokenException"></exception>
        private void CreateAccessToken(string issuer, string issuedTo, JToken token)
        {
            try
            {
                var accessTokenValue = token["accessToken"];
                var expires = token["expires"];
                var accessToken = $"Bearer {accessTokenValue.Value<string>()}";
                var date = expires.Value<DateTime>();
                this.AccessToken = new AccessToken(issuer, issuedTo, accessToken, date);
                Client.Headers.Remove("Authorization");
                Client.Headers.Add("Authorization", this.AccessToken.Token);

                if (RefreshToken)
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan lifePeriod = (AccessToken.Expires - now);
                    TimeSpan halfLifePeriod = new TimeSpan(lifePeriod.Ticks / 2);
                    DateTime halfLife = now.Add(halfLifePeriod);
                    this.RefreshDateTime = halfLife;

                    ScheduleRefresh();
                }
            }
            catch (Exception e) when (e is System.ArgumentNullException
                || e is System.NullReferenceException || e is System.FormatException)
            {
                throw new MetasysTokenException(token.ToString(), e);
            }
        }

        /// <summary>
        /// Will call Refresh() a minute before the token expires.
        /// </summary>
        private void ScheduleRefresh()
        {
            var delayms = new TimeSpan(0, 1, 0).TotalMilliseconds;
            _timer = new System.Timers.Timer(delayms)
            {
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                DateTime now = DateTime.UtcNow;
                if (now > this.RefreshDateTime)
                {
                    Refresh2();
                }
            };
        }

        ///// <summary>
        ///// Will call Refresh() a minute before the token expires.
        ///// </summary>
        //private void ScheduleRefresh()
        //{
        //    DateTime now = DateTime.UtcNow;
        //    TimeSpan delay = AccessToken.Expires - now.AddSeconds(-1); // minimum renew gap of 1 sec in advance
        //    // Renew one minute before expiration if there is more than one minute time
        //    if (delay > new TimeSpan(0, 1, 0))
        //    {
        //        delay.Subtract(new TimeSpan(0, 1, 0));
        //    }
        //    if (delay <= TimeSpan.Zero)
        //    {
        //        // Token already expired
        //        return;
        //    }
        //    int delayms;
        //    if (delay.TotalMilliseconds > int.MaxValue)
        //    {
        //        // Delay is set to int MaxValue to do not go negative with double to int conversion.
        //        delayms = int.MaxValue;
        //    }
        //    else
        //    {
        //        delayms = (int)delay.TotalMilliseconds;
        //    }
        //    System.Threading.Tasks.Task.Delay(delayms).ContinueWith(_ => Refresh2());
        //}


        /// <summary>
        /// Overload of ReadPropertyAsync for internal use where Exception suppress is needed, e.g. ReadPropertyMultiple
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attributeName"></param>
        /// <param name="suppressNotFoundException"></param>
        /// <returns></returns>
        private async Task<Variant> ReadPropertyAsync(ObjectId id, string attributeName, bool suppressNotFoundException = true)
        {
            try
            {
                return await ReadPropertyAsync(id, attributeName).ConfigureAwait(false);
            }
            catch (MetasysHttpNotFoundException) when (suppressNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates the body for the WriteProperty and WritePropertyMultiple requests as a dictionary.
        /// </summary>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <returns>Dictionary of the attribute, value pairs.</returns>
        private Dictionary<string, object> GetWritePropertyBody(IEnumerable<(string Attribute, object Value)> attributeValues)
        {
            Dictionary<string, object> pairs = new Dictionary<string, object>();
            foreach (var attribute in attributeValues)
            {
                pairs.Add(attribute.Attribute, attribute.Value);
            }
            return pairs;
        }

        /// <summary>
        /// Write one or many attribute values in the provided json given the Guid of the object asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <returns>Asynchronous Task Result.</returns>
        private async Task WritePropertyRequestAsync(ObjectId id, Dictionary<string, object> body)
        {
            var json = new { item = body };

            try
            {
                var response = await Client.Request(new Url("objects")
                    .AppendPathSegment(id))
                    .PatchJsonAsync(json)
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
        }

        ///// <summary>
        ///// Gets the type from a token retrieved from a typeUrl
        ///// </summary>
        ///// <param name="typeToken"></param>
        ///// <exception cref="MetasysHttpException"></exception>
        ///// <exception cref="MetasysObjectTypeException"></exception>
        //private MetasysObjectType GetType(JToken typeToken)
        //{
        //    try
        //    {
        //        if (typeToken != null || typeToken == null)
        //        {
        //            //string description = (typeToken.Contains("description") && typeToken["description"] != null) ? typeToken["description"].Value<string>(): "";
        //            //int id = (typeToken.Contains("id") && typeToken["id"] != null) ?  typeToken["id"].Value<int>() : -1;
        //            //string key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
        //            string description = typeToken["description"].Value<string>();
        //            int id = typeToken["id"].Value<int>();
        //            //string key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
        //            string key = GetObjectTypeEnumeration(description);

        //            if (key.Length > 0)
        //            {
        //                string translation = Localize(key);
        //                if (translation != key)
        //                {
        //                    // A translation was found
        //                    description = translation;
        //                }
        //            }
        //            return new MetasysObjectType(id, key, description);
        //        }
        //        else
        //        {
        //            return new MetasysObjectType(-1, "", "");
        //        }
        //    }
        //    catch (Exception e) when (e is System.ArgumentNullException
        //        || e is System.NullReferenceException || e is System.FormatException)
        //    {
        //        throw new MetasysObjectTypeException(typeToken.ToString(), e);
        //    }
        //}

        ///// <summary>
        ///// Gets the type from the values of the paramenters
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="description"></param>
        ///// <param name="key"></param>
        ///// <exception cref="MetasysHttpException"></exception>
        ///// <exception cref="MetasysObjectTypeException"></exception>
        //private MetasysObjectType GetType(int id, String description, String key)
        //{
        //    try
        //    {
        //        if (id >= 0 && description.Length > 0)
        //        {
        //            if (String.IsNullOrEmpty(key))
        //            {
        //                key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
        //            }
        //            if (key.Length > 0)
        //            {
        //                string translation = Localize(key);
        //                if (translation != key)
        //                {
        //                    // A translation was found
        //                    description = translation;
        //                }
        //            }
        //            return new MetasysObjectType(id, key, description);
        //        }
        //        else
        //        {
        //            return new MetasysObjectType(-1, "", "");
        //        }
        //    }
        //    catch (Exception e) when (e is System.ArgumentNullException
        //        || e is System.NullReferenceException || e is System.FormatException)
        //    {
        //        throw new MetasysObjectTypeException(id.ToString() + " " + description, e);
        //    }
        //}

        /// <summary>
        /// Convert a JToken batch request response into VariantMultiple.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private IEnumerable<VariantMultiple> ToVariantMultiples(JToken response)
        {
            List<VariantMultiple> multiples = new List<VariantMultiple>();
            foreach (var r in response["responses"])
            {
                var respIds = r["id"].Value<string>().Split('_');
                var objId = new ObjectId(respIds[0]);
                string attr = respIds[1];
                List<Variant> values = new List<Variant>();
                if (r["status"].Value<int>() == 200)
                {
                    values.Add(new Variant(objId, r["body"], attr, Culture, Version));
                } // Don't add the variant to the list if the response is not successful
                var m = multiples.SingleOrDefault(s => s.Id == objId);
                if (m == null)
                {
                    // Add a new multiple for the current object
                    multiples.Add(new VariantMultiple(objId, values));
                }
                else
                {
                    // Variant multiple already exists, just add Values
                    var newList = m.Values.ToList();
                    newList.AddRange(values);
                    m.Values = newList;
                }
            }
            return multiples;
        }

        /// <summary>
        /// Send a command to an object asynchronously.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="values"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <returns>Asynchronous Task Result.</returns>
        private async Task SendCommandRequestAsync(ObjectId id, string command, IEnumerable<object> values)
        {
            try
            {
                if (Version > ApiVersion.v3)
                {
                    JArray arrayValues = new JArray(values);
                    JProperty propParams = new JProperty("parameters", arrayValues);
                    JObject jsonValues = new JObject(propParams);

                    var response = await Client.Request(new Url("objects")
                                                                .AppendPathSegments(id, "commands", command))
                                                                .PutJsonAsync(jsonValues)
                                                                .ConfigureAwait(false);
                }
                else
                {
                    var response = await Client.Request(new Url("objects")
                                                                .AppendPathSegments(id, "commands", command))
                                                                .PutJsonAsync(values)
                                                                .ConfigureAwait(false);
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
        }

        #region "ad-hoc calls" // =========================================================================================================
        // Support Ad-Hoc calls -----------------------------------------------------------------------------------------------------------
        ///<inheritdoc/>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
        {
            var response = new HttpResponseMessage();
            try
            {
                var flurlRequest = Client.Request();
                flurlRequest.Url = GetUrlFromHttpRequest(requestMessage);

                // Flurl.Http 2.4.2 can only work with 1 value per header
                // Once upgraded to Flurl 3.0.1, then multiple values can be supported
                var headers = requestMessage.Headers.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value.First());
                flurlRequest.WithHeaders(headers);

                response = await flurlRequest.SendAsync(requestMessage.Method, requestMessage.Content, cancellationToken, completionOption).ConfigureAwait(false);
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return response;
        }

        private Url GetUrlFromHttpRequest(HttpRequestMessage requestMessage)
        {
            var baseUri = new Uri(Client.BaseUrl);
            var requestUri = requestMessage.RequestUri.ToString();
            if (Uri.IsWellFormedUriString(requestUri, UriKind.Absolute))
            {
                if (Uri.Compare(baseUri, requestMessage.RequestUri, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new UriFormatException("HTTP request can not be made. Scheme or Host is invalid.");
                }
                return new Url(requestUri);
            }
            else
            {
                return new Url(Url.Combine(baseUri.GetLeftPart(UriPartial.Authority), "/api/", requestUri));
            }
        }
        #endregion

        #region Deprecated Methods Due to Guid -> ObjectId
        // ReadPropertyMultiple -----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        [Obsolete("Use ReadPropertyMultiple(IEnumerable<ObjectId>, IEnumerable<string>) instead.")]
        public IEnumerable<VariantMultiple> ReadPropertyMultiple(IEnumerable<Guid> ids, IEnumerable<string> attributeNames)
        {
            return ReadPropertyMultipleAsync(ids, attributeNames).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        [Obsolete("Use ReadPropertyMultipleAsync(IEnumerable<ObjectId>, IEnumerable<string>) instead.")]
        public Task<IEnumerable<VariantMultiple>> ReadPropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<string> attributeNames)
        {
            return ReadPropertyMultipleAsync(ids.Cast<ObjectId>(), attributeNames);
        }

        // WritePropertyMultiple ----------------------------------------------------------------------------------------------------
        ///<inheritdoc/>
        [Obsolete("Use WritePropertyMultiple(IEnumerable<ObjectId>, Dictionary<string, object>) instead.")]
        public void WritePropertyMultiple(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues)
        {
            WritePropertyMultipleAsync(ids, attributeValues).GetAwaiter().GetResult();
        }
        ///<inheritdoc/>
        [Obsolete("Use WritePropertyMultipleAsync(IEnumerable<ObjectId>, Dictionary<string, object>) instead.")]
        public Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues)
        {
            return WritePropertyMultipleAsync(ids.Cast<ObjectId>(), attributeValues);
        }

        // WritePropertyMultiple (2) ------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        [Obsolete("Use WritePropertyMultiple(IEnumerable<ObjectId> ids, IEnumerable<ValueTuple<string, Value>>) instead.")]
        public void WritePropertyMultiple(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues)
        {
            WritePropertyMultipleAsync(ids, attributeValues).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        [Obsolete("Use WritePropertyMultipleAsync(IEnumerable<ObjectId> ids, IEnumerable<ValueTuple<string, Value>>) instead.")]
        public Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues)
        {
            return WritePropertyMultipleAsync(ids.Cast<ObjectId>(), attributeValues);
        }

        #endregion
    }

}
