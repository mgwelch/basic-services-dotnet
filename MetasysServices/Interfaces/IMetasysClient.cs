using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// An HTTP client for consuming the most commonly used endpoints of the Metasys API.
    /// </summary>
    public interface IMetasysClient : IBasicService
    {
        /// <summary>
        /// The hostname of Metasys API server.
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        /// The timeout of https request.
        /// </summary>
        int Timeout { get; set; }

        ///// <summary>
        ///// The Metasys server's Api version.
        ///// </summary>
        //ApiVersion Version { get; set; }

        ///// <summary>
        ///// The current Culture Used for Metasys client localization.
        ///// </summary>
        //CultureInfo Culture { get; set; }

        /// <summary>
        /// Services for Activities.
        /// </summary>
        IActivityService Activities { get; set; }

        /// <summary>
        /// Services for Alarms.
        /// </summary>
        IAlarmsService Alarms { get; set; }

        /// <summary>
        /// Services for Audits.
        /// </summary>
        IAuditService Audits { get; set; }

        /// <summary>
        /// Services for Enumerations.
        /// </summary>
        IEnumerationService Enumerations { get; set; }

        /// <summary>
        /// Services for Equipments.
        /// </summary>
        IEquipmentService Equipments { get; set; }

        /// <summary>
        /// Services for Network Devices.
        /// </summary>
        INetworkDeviceService NetworkDevices { get; set; }

        /// <summary>
        /// Services for Trends and Samples.
        /// </summary>
        ITrendService Trends { get; set; }

        /// <summary>
        /// Services for Spaces.
        /// </summary>
        ISpaceService Spaces { get; set; }

        /// <summary>
        /// Services for Streams.
        /// </summary>
        IStreamService Streams { get; set; }

        ///// <summary>
        ///// Localizes the specified resource key for the current MetasysClient locale or specified culture.
        ///// </summary>
        ///// <remarks>
        ///// The resource parameter must be the key of a Metasys enumeration resource,
        ///// otherwise no translation will be found.
        ///// </remarks>
        ///// <param name="resource">The key for the localization resource.</param>
        ///// <param name="cultureInfo">Optional culture specification.</param>
        ///// <returns>
        ///// Localized string if the resource was found, the default en-US localized string if not found,
        ///// or the resource parameter value if neither resource is found.
        ///// </returns>
        //string Localize(string resource, CultureInfo cultureInfo = null);

        /// <summary>
        /// Attempts to get the enumeration key of a given en-US localized command.
        /// </summary>
        /// <remarks>
        /// The resource parameter must be the value of a Metasys commandIdEnumSet en-US value,
        /// otherwise no key will be found.
        /// </remarks>
        /// <param name="resource">The en-US value for the localization resource.</param>
        /// <returns>
        /// The enumeration key of the en-US command if found, original resource if not.
        /// </returns>
        string GetCommandEnumeration(string resource);

        ///// <summary>
        ///// Attempts to get the enumeration key of a given en-US localized objectType.
        ///// </summary>
        ///// <remarks>
        ///// The resource parameter must be the value of a Metasys objectTypeEnumSet en-US value,
        ///// otherwise no key will be found.
        ///// </remarks>
        ///// <param name="resource">The en-US value for the localization resource.</param>
        ///// <returns>
        ///// The enumeration key of the en-US objectType if found, original resource if not.
        ///// </returns>
        //string GetObjectTypeEnumeration(string resource);


        /// <summary>
        /// Attempts to login to the given host and retrieve an access token.
        /// </summary>
        /// <returns>Access Token.</returns>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="refresh">Flag to set automatic access token refreshing to keep session active.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysTokenException"></exception>
        AccessToken TryLogin(string username, string password, bool refresh = true);
        /// <inheritdoc cref="IMetasysClient.TryLogin(string, string, bool)"/>
        Task<AccessToken> TryLoginAsync(string username, string password, bool refresh = true);


        /// <summary>
        /// Requests a new access token from the server before the current token expires.
        /// </summary>
        /// <returns>Access Token.</returns>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysTokenException"></exception>
        AccessToken Refresh();
        /// <inheritdoc cref="IMetasysClient.Refresh()"/>
        Task<AccessToken> RefreshAsync();


        /// <summary>
        /// Returns the current session access token.
        /// </summary>
        /// <returns>Current session's Access Token.</returns>
        AccessToken GetAccessToken();

        ///// <summary>
        ///// Set the current session access token.
        ///// </summary>
        ///// <returns>Current session's Access Token.</returns>
        //void SetAccessToken(AccessToken accessToken);


        /// <summary>
        /// Given the Item Reference of an object, returns the object identifier.
        /// </summary>
        /// <remarks>
        /// The itemReference will be automatically URL encoded.
        /// </remarks>
        /// <returns>An <see cref="ObjectId"/> for the specified object.</returns>
        /// <param name="itemReference"></param>
        /// <exception cref="MetasysHttpException"></exception>
        ObjectId GetObjectIdentifier(string itemReference);
        /// <inheritdoc cref="IMetasysClient.GetObjectIdentifier(string)"/>
        Task<ObjectId> GetObjectIdentifierAsync(string itemReference);

        /// <summary>
        /// Read one attribute value given the id of the object.
        /// </summary>
        /// <returns>
        /// Variant if the attribute exists, null if does not exist.
        /// </returns>
        /// <param name="id"></param>
        /// <param name="attributeName"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysPropertyException"></exception>
        Variant ReadProperty(ObjectId id, string attributeName);
        /// <inheritdoc cref="IMetasysClient.ReadProperty(ObjectId, string)"/>
        Task<Variant> ReadPropertyAsync(ObjectId id, string attributeName);


        /// <summary>
        /// Read many attribute values given the ids of the objects.
        /// </summary>
        /// <returns>
        /// A list of VariantMultiple with all the specified attributes (if existing).
        /// </returns>
        /// <param name="ids"></param>
        /// <param name="attributeNames"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysPropertyException"></exception>
        IEnumerable<VariantMultiple> ReadPropertyMultiple(IEnumerable<ObjectId> ids, IEnumerable<string> attributeNames);
        /// <inheritdoc cref="IMetasysClient.ReadPropertyMultiple(IEnumerable{ObjectId}, IEnumerable{string})"/>
        Task<IEnumerable<VariantMultiple>> ReadPropertyMultipleAsync(IEnumerable<ObjectId> ids, IEnumerable<string> attributeNames);

        /// <summary>
        /// Write a single attribute given the id of the object.
        /// </summary>
        /// <param name="id">The id of the attribute</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="newValue">The new value of the attribute</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WriteProperty(ObjectId id, string attributeName, object newValue);
        /// <inheritdoc cref="IMetasysClient.WriteProperty(ObjectId, string, object)"/>
        Task WritePropertyAsync(ObjectId id, string attributeName, object newValue);


        /// <summary>
        /// Write to many attribute values given the ids of the objects.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WritePropertyMultiple(IEnumerable<ObjectId> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <summary>
        /// Write to many attribute values given the ids of the objects.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WritePropertyMultiple(IEnumerable<ObjectId> ids, Dictionary<string, object> attributeValues);


        /// <summary>
        /// Write asynchronously to many attribute values given the ids of the objects.
        /// </summary>
        Task WritePropertyMultipleAsync(IEnumerable<ObjectId> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <inheritdoc cref="IMetasysClient.WritePropertyMultiple(IEnumerable{ObjectId}, Dictionary{string, object})"/>
        Task WritePropertyMultipleAsync(IEnumerable<ObjectId> ids, Dictionary<string, object> attributeValues);



        /// <summary>
        /// Get all available commands given the id of the object.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of Commands.</returns>
        IEnumerable<Command> GetCommands(ObjectId id);
        /// <inheritdoc cref="IMetasysClient.GetCommands(ObjectId)"/>
        Task<IEnumerable<Command>> GetCommandsAsync(ObjectId id);


        /// <summary>
        /// Send a command to an object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="values"></param>
        /// <exception cref="MetasysHttpException"></exception>
        void SendCommand(ObjectId id, string command, IEnumerable<object> values = null);
        /// <inheritdoc cref="IMetasysClient.SendCommand(ObjectId, string, IEnumerable{object})"/>
        Task SendCommandAsync(ObjectId id, string command, IEnumerable<object> values = null);

        /// <summary>
        /// <s>Gets all network devices.</s>
        /// </summary>
        /// <remarks>
        /// <b>Note this method has been deprecated.</b> Please use
        /// <see cref="GetNetworkDevices(NetworkDeviceTypeEnum)"/>
        /// or <see cref="GetNetworkDevicesAsync(NetworkDeviceTypeEnum)"/> instead.
        /// </remarks>
        /// <param name="type">Optional type number as a string</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        [Obsolete("Use GetNetworkDevices(NetworkDeviceTypeEnum) instead.")]
        IEnumerable<MetasysObject> GetNetworkDevices(string type = null);
        /// <inheritdoc cref="IMetasysClient.GetNetworkDevices(string)"/>
        [Obsolete("Use GetNetworkDevicesAsync(NetworkDeviceTypeEnum) instead.")]
        Task<IEnumerable<MetasysObject>> GetNetworkDevicesAsync(string type = null);


        /// <summary>
        /// Gets all network devices.
        /// </summary>
        /// <param name="networkDevicetype">Network Device Type enum</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetNetworkDevices(NetworkDeviceTypeEnum networkDevicetype);
        /// <inheritdoc cref="IMetasysClient.GetNetworkDevices(NetworkDeviceTypeEnum)"/>
        Task<IEnumerable<MetasysObject>> GetNetworkDevicesAsync(NetworkDeviceTypeEnum networkDevicetype);


        /// <summary>
        /// Gets all available network device types.
        /// </summary>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObjectType> GetNetworkDeviceTypes();
        /// <inheritdoc cref="IMetasysClient.GetNetworkDeviceTypes()"/>
        Task<IEnumerable<MetasysObjectType>> GetNetworkDeviceTypesAsync();


        /// <summary>
        /// Gets all child objects given a parent id.
        /// Level indicates how deep to retrieve objects.
        /// </summary>
        /// <remarks>
        /// A level of 1 only retrieves immediate children of the parent object.
        /// </remarks>
        /// <param name="id">The ID of the parent object.</param>
        /// <param name="levels">The depth of the children to retrieve.</param>
        /// <param name="includeInternalObjects">Set it to true to see also internal objects that are not displayed in the Metasys tree. </param>
        /// <param name="includeExtensions">Set it to true to get also the extensions of the object.</param>
        /// <remarks> The flag includeInternalObjects applies since Metasys API v3. </remarks>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetObjects(ObjectId id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false);
        /// <inheritdoc cref="IMetasysClient.GetObjects(ObjectId, int, bool, bool)"/>
        Task<IEnumerable<MetasysObject>> GetObjectsAsync(ObjectId id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false);

        /// <summary>
        /// Gets all child objects given a parent id and object type.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType">The object type enum set.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetObjects(ObjectId objectId, string objectType);

        /// <inheritdoc cref="IMetasysClient.GetObjects(ObjectId, string)"/>
        Task<IEnumerable<MetasysObject>> GetObjectsAsync(ObjectId objectId, string objectType);

        #region "SPACES" //==============================================================================================================
        // GetSpaces ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves a collection of spaces.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated</b> Please use
        /// <see cref="SpaceServiceProvider.Get(SpaceTypeEnum?, int?, int?, string)"/> or
        /// <see cref="SpaceServiceProvider.GetAsync(SpaceTypeEnum?, int?, int?, string)"/> instead.
        /// </remarks>
        /// <param name="type">Optional type ID belonging to SpaceTypeEnum.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        [Obsolete("Use SpaceServiceProvider.Get(SpaceTypeEnum?, int?, int?, string) instead.")]
        IEnumerable<MetasysObject> GetSpaces(SpaceTypeEnum? type = null);
        /// <inheritdoc cref="IMetasysClient.GetSpaces(SpaceTypeEnum?)"/>
        [Obsolete("SpaceServiceProvider.GetAsync(SpaceTypeEnum?, int?, int?, string) instead.")]
        Task<IEnumerable<MetasysObject>> GetSpacesAsync(SpaceTypeEnum? type = null);

        // GetSpaceChildren --------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves the collection of spaces that are located within the specified space.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated.</b> Please use
        /// <see cref="SpaceServiceProvider.GetChildren(ObjectId)"/> or
        /// <see cref="SpaceServiceProvider.GetChildrenAsync(ObjectId)"/> instead.
        /// </remarks>
        /// <param name="spaceId">The GUID of the parent space.</param>
        [Obsolete("Use SpaceServiceProvider.GetChildren(ObjectId) instead.")]
        IEnumerable<MetasysObject> GetSpaceChildren(Guid spaceId);
        /// <inheritdoc cref="IMetasysClient.GetSpaceChildren(Guid)"/>
        [Obsolete("Use SpaceServiceProvider.GetChildrenAsync(ObjectId) instead.")]
        Task<IEnumerable<MetasysObject>> GetSpaceChildrenAsync(Guid spaceId);


        // GetSpaceTypes -----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves the collection of all spaces types.</s>
        /// <remarks>
        /// <b>This method is deprecated.</b> Please use
        /// <see cref="SpaceServiceProvider.GetTypes"/> or
        /// <see cref="SpaceServiceProvider.GetTypesAsync"/> instead.
        /// </remarks>
        /// </summary>
        IEnumerable<MetasysObjectType> GetSpaceTypes();
        /// <inheritdoc cref="IMetasysClient.GetSpaceTypes()"/>
        Task<IEnumerable<MetasysObjectType>> GetSpaceTypesAsync();
        #endregion //=====================================================================================================================

        #region "EQUIPMENTS" //==========================================================================================================
        // GetEquipment -----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves a collection of equipment instances.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated.</b> Please use
        /// <see cref="EquipmentServiceProvider.Get(int?, int?)"/> or
        /// <see cref="EquipmentServiceProvider.GetAsync(int?, int?)"/> instead.
        /// </remarks>
        [Obsolete("Use EquipmentServiceProvider.Get(int?, int?) instead.")]
        IEnumerable<MetasysObject> GetEquipment();
        /// <inheritdoc cref="IMetasysClient.GetEquipment()"/>
        [Obsolete("Use EquipmentServiceProvider.GetAsync(int?, int?) instead.")]
        Task<IEnumerable<MetasysObject>> GetEquipmentAsync();

        // GetEquipmentPoints -----------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves the collection of points that are defined by the specified equipment instance</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated</b> Please use
        /// <see cref="EquipmentServiceProvider.GetPoints(ObjectId, bool)"/> or
        /// <see cref="EquipmentServiceProvider.GetPointsAsync(ObjectId, bool)"/> instead.
        /// </remarks>
        /// <param name="equipmentId">The Guid of the equipment.</param>
        /// <param name="readAttributeValue">Set to false if you would not read Points Attribute Value.</param>
        /// <remarks> Reading the Attribute Value attribute could take time depending on the number of points. </remarks>
        /// <returns></returns>
        [Obsolete("Use EquipmentServiceProvider.GetPoints(ObjectId, bool) instead.")]
        IEnumerable<MetasysPoint> GetEquipmentPoints(Guid equipmentId, bool readAttributeValue = true);
        /// <inheritdoc cref="IMetasysClient.GetEquipmentPoints(Guid, bool)"/>
        [Obsolete("Use EquipmentServiceProvider.GetPointsAsync(ObjectId, bool) instead.")]
        Task<IEnumerable<MetasysPoint>> GetEquipmentPointsAsync(Guid equipmentId, bool readAttributeValue = true);

        // GetSpaceEquipment ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// <s>Retrieves the collection of equipment that serve the specified space.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated</b> Please use
        /// <see cref="EquipmentServiceProvider.GetServingASpace(ObjectId)"/> or
        /// <see cref="EquipmentServiceProvider.GetServingASpaceAsync(ObjectId)"/> instead.
        /// </remarks>
        /// <param name="spaceId"></param>
        [Obsolete("Use EquipmentServiceProvider.GetServingASpace(ObjectId) instead.")]
        IEnumerable<MetasysObject> GetSpaceEquipment(Guid spaceId);
        /// <inheritdoc cref="IMetasysClient.GetSpaceEquipment(Guid)"/>
        [Obsolete("Use EquipmentServiceProvider.GetServingASpaceAsync(ObjectId) instead.")]
        Task<IEnumerable<MetasysObject>> GetSpaceEquipmentAsync(Guid spaceId);
        #endregion

        /// <summary>
        /// Attempts to login to the given host using Credential Manager and retrieve an access token.
        /// </summary>
        /// <param name="credManTarget">The Credential Manager target where to pick the credentials.</param>
        /// <param name="refresh">Flag to set automatic access token refreshing to keep session active.</param>
        /// <remarks> This method can be overridden by extended class with other Credential Manager implementations. </remarks>
        AccessToken TryLogin(string credManTarget, bool refresh = true);

        /// <inheritdoc cref="IMetasysClient.TryLogin(string, bool)"/>
        Task<AccessToken> TryLoginAsync(string credManTarget, bool refresh = true);

        /// <summary>
        /// Returns the current server time in UTC format.
        /// </summary>
        /// <returns>The current Server time.</returns>
        /// <exception cref="MetasysHttpException">Occurs when it's not possible to read date time from HTTP response.</exception>
        DateTime GetServerTime();

        /// <inheritdoc cref="IMetasysClient.GetServerTime()"/>
        Task<DateTime> GetServerTimeAsync();

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        ///
        /// <para>
        /// This method currently only supports 1 value per header rather than multiple. In a future revision, this is planned to be addressed.
        /// </para>
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="completionOption"> When the operation should complete (as soon as a response is available or after reading the whole response content).</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);

        // Deprecated Methods ===========================================================================================
        #region Deprecated Methods due to Guid -> ObjectId change
        /// <summary>
        /// <s>Read many attribute values given the Guids of the objects.</s>
        /// </summary>
        /// <remarks>
        /// This method is deprecated. Please use <see cref="ReadPropertyMultiple(IEnumerable{ObjectId}, IEnumerable{string})"/>
        /// or <see cref="ReadPropertyMultipleAsync(IEnumerable{ObjectId}, IEnumerable{string})"/> instead.
        /// </remarks>
        /// <returns>
        /// A list of VariantMultiple with all the specified attributes (if existing).
        /// </returns>
        /// <param name="ids"></param>
        /// <param name="attributeNames"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysPropertyException"></exception>
        [Obsolete("Use ReadPropertyMultiple(IEnumerable<ObjectId>, IEnumerable<string>) instead.")]
        IEnumerable<VariantMultiple> ReadPropertyMultiple(IEnumerable<Guid> ids, IEnumerable<string> attributeNames);
        /// <inheritdoc cref="IMetasysClient.ReadPropertyMultiple(IEnumerable{Guid}, IEnumerable{string})"/>
        [Obsolete("Use ReadPropertyMultipleAsync(IEnumerable<ObjectId>, IEnumerable<string>) instead.")]
        Task<IEnumerable<VariantMultiple>> ReadPropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<string> attributeNames);


        /// <summary>
        /// <s>Write to many attribute values given the Guids of the objects.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated.</b> Please use one of the following instead:
        /// <see cref="WritePropertyMultiple(IEnumerable{ObjectId}, Dictionary{string, object})"/>,
        /// <see cref="WritePropertyMultiple(IEnumerable{ObjectId}, IEnumerable{ValueTuple{string, object}})"/>,
        /// <see cref="WritePropertyMultipleAsync(IEnumerable{ObjectId}, Dictionary{string, object})"/>,
        /// or <see cref="WritePropertyMultipleAsync(IEnumerable{ObjectId}, IEnumerable{ValueTuple{string, object}})"/>.
        /// </remarks>
        /// <param name="ids"></param>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <exception cref="MetasysHttpException"></exception>
        [Obsolete("Use WritePropertyMultiple(IEnumerable<ObjectId>, IEnumerable<string, object>) instead.")]
        void WritePropertyMultiple(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <inheritdoc cref="WritePropertyMultiple(IEnumerable{Guid}, IEnumerable{ValueTuple{string, object}})"/>
        [Obsolete("Use WritePropertyMultiple(IEnumerable<ObjectId>, Dictionary<string, object>) instead.")]
        void WritePropertyMultiple(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues);

        /// <summary>
        /// <s>Write asynchronously to many attribute values given the Guids of the objects.</s>
        /// </summary>
        /// <remarks>
        /// <b>This method is deprecated.</b> Please use
        /// <see cref="WritePropertyMultipleAsync(IEnumerable{ObjectId}, Dictionary{string, object})"/> or
        /// <see cref="WritePropertyMultipleAsync(IEnumerable{ObjectId}, IEnumerable{ValueTuple{string, object}})"/>
        /// instead.
        /// </remarks>
        [Obsolete("Use WritePropertyMultipleAsync(IEnumerable<ObjectId>, IEnumerable<string, object>) instead.")]
        Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <inheritdoc cref="IMetasysClient.WritePropertyMultiple(IEnumerable{Guid}, Dictionary{string, object})"/>
        [Obsolete("Use WritePropertyMultiple(IEnumerable<ObjectId>, Dictionary<string, object>) instead.")]
        Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues);

        #endregion


    }
}
