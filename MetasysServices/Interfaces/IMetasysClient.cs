using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
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
        /// <remarks><b>It's recommended that you use one of the other variants that do not
        /// pass a password in plaintext.</b> You can use <see cref="SecretStore"/> to save and
        /// retrieve passwords from your operating systems keystore.
        /// </remarks>
        /// <seealso cref="TryLogin(string, SecureString)"/>
        /// <seealso cref="TryLoginAsync(string, SecureString)"/>
        /// <seealso cref="TryLogin(string, bool)"/>
        /// <seealso cref="TryLoginAsync(string, bool)"/>
        /// <seealso cref="SecretStore"/>
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
        /// Attempts to login to the given host and retrieve an access token.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        AccessToken TryLogin(string username, SecureString password);
        /// <inheritdoc cref="TryLogin(string, SecureString)"/>
        Task<AccessToken> TryLoginAsync(string username, SecureString password);



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
        /// <returns>A Guid representing the id, or an empty Guid if errors occurred.</returns>
        /// <param name="itemReference"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysGuidException"></exception>
        Guid GetObjectIdentifier(string itemReference);
        /// <inheritdoc cref="IMetasysClient.GetObjectIdentifier(string)"/>
        Task<Guid> GetObjectIdentifierAsync(string itemReference);


        /// <summary>
        /// Read one attribute value given the Guid of the object.
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
        Variant ReadProperty(Guid id, string attributeName);
        /// <inheritdoc cref="IMetasysClient.ReadProperty(Guid, string)"/>
        Task<Variant> ReadPropertyAsync(Guid id, string attributeName);


        /// <summary>
        /// Read many attribute values given the Guids of the objects.
        /// </summary>
        /// <returns>
        /// A list of VariantMultiple with all the specified attributes (if existing).
        /// </returns>
        /// <param name="ids"></param>
        /// <param name="attributeNames"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysPropertyException"></exception>
        IEnumerable<VariantMultiple> ReadPropertyMultiple(IEnumerable<Guid> ids, IEnumerable<string> attributeNames);
        /// <inheritdoc cref="IMetasysClient.ReadPropertyMultiple(IEnumerable{Guid}, IEnumerable{string})"/>
        Task<IEnumerable<VariantMultiple>> ReadPropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<string> attributeNames);


        /// <summary>
        /// Write a single attribute given the Guid of the object.
        /// </summary>
        /// <param name="id">The id of the attribute</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="newValue">The new value of the attribute</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WriteProperty(Guid id, string attributeName, object newValue);
        /// <inheritdoc cref="IMetasysClient.WriteProperty(Guid, string, object)"/>
        Task WritePropertyAsync(Guid id, string attributeName, object newValue);


        /// <summary>
        /// Write to many attribute values given the Guids of the objects.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WritePropertyMultiple(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <summary>
        /// Write to many attribute values given the Guids of the objects.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="attributeValues">The (attribute, value) pairs.</param>
        /// <exception cref="MetasysHttpException"></exception>
        void WritePropertyMultiple(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues);


        /// <summary>
        /// Write asyncronously to many attribute values given the Guids of the objects.
        /// </summary>
        Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, IEnumerable<(string Attribute, object Value)> attributeValues);
        /// <inheritdoc cref="IMetasysClient.WritePropertyMultiple(IEnumerable{Guid}, Dictionary{string, object})"/>
        Task WritePropertyMultipleAsync(IEnumerable<Guid> ids, Dictionary<string, object> attributeValues);


        /// <summary>
        /// Get all available commands given the Guid of the object.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of Commands.</returns>
        IEnumerable<Command> GetCommands(Guid id);
        /// <inheritdoc cref="IMetasysClient.GetCommands(Guid)"/>
        Task<IEnumerable<Command>> GetCommandsAsync(Guid id);


        /// <summary>
        /// Send a command to an object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="values"></param>
        /// <exception cref="MetasysHttpException"></exception>
        void SendCommand(Guid id, string command, IEnumerable<object> values = null);
        /// <inheritdoc cref="IMetasysClient.SendCommand(Guid, string, IEnumerable{object})"/>
        Task SendCommandAsync(Guid id, string command, IEnumerable<object> values = null);


        /// <summary>
        /// Gets all network devices.
        /// </summary>
        /// <remarks>
        /// Note this method has been deprecated.
        /// </remarks>
        /// <param name="type">Optional type number as a string</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetNetworkDevices(string type = null);
        /// <inheritdoc cref="IMetasysClient.GetNetworkDevices(string)"/>
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
        /// Gets all child objects given a parent Guid.
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
        IEnumerable<MetasysObject> GetObjects(Guid id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false);
        /// <inheritdoc cref="IMetasysClient.GetObjects(Guid, int, bool, bool)"/>
        Task<IEnumerable<MetasysObject>> GetObjectsAsync(Guid id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false);

        /// <summary>
        /// Gets all child objects given a parent Guid and object type.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="objectType">The object type enum set.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetObjects(Guid objectId, string objectType);

        /// <inheritdoc cref="IMetasysClient.GetObjects(Guid, string)"/>
        Task<IEnumerable<MetasysObject>> GetObjectsAsync(Guid objectId, string objectType);

        #region "SPACES" //==============================================================================================================
        // GetSpaces ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves a collection of spaces.
        /// </summary>
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Spaces service).
        /// </remarks>
        /// <param name="type">Optional type ID belonging to SpaceTypeEnum.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> GetSpaces(SpaceTypeEnum? type = null);
        /// <inheritdoc cref="IMetasysClient.GetSpaces(SpaceTypeEnum?)"/>
        Task<IEnumerable<MetasysObject>> GetSpacesAsync(SpaceTypeEnum? type = null);

        // GetSpaceChildren --------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of spaces that are located within the specified space.
        /// </summary>
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Spaces service).
        /// </remarks>
        /// <param name="spaceId">The GUID of the parent space.</param>
        IEnumerable<MetasysObject> GetSpaceChildren(Guid spaceId);
        /// <inheritdoc cref="IMetasysClient.GetSpaceChildren(Guid)"/>
        Task<IEnumerable<MetasysObject>> GetSpaceChildrenAsync(Guid spaceId);

        // GetSpaceTypes -----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of all spaces types.
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Spaces service).
        /// </remarks>
        /// </summary>
        IEnumerable<MetasysObjectType> GetSpaceTypes();
        /// <inheritdoc cref="IMetasysClient.GetSpaceTypes()"/>
        Task<IEnumerable<MetasysObjectType>> GetSpaceTypesAsync();
        #endregion //=====================================================================================================================

        #region "EQUIPMENTS" //==========================================================================================================
        // GetEquipment -----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves a collection of equipment instances.
        /// </summary>
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Equipments service).
        /// </remarks>
        IEnumerable<MetasysObject> GetEquipment();
        /// <inheritdoc cref="IMetasysClient.GetEquipment()"/>
        Task<IEnumerable<MetasysObject>> GetEquipmentAsync();

        // GetEquipmentPoints -----------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of points that are defined by the specified equipment instance
        /// </summary>
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Equipments service).
        /// </remarks>
        /// <param name="equipmentId">The Guid of the equipment.</param>
        /// <param name="readAttributeValue">Set to false if you would not read Points Attribute Value.</param>
        /// <remarks> Reading the Attribute Value attribute could take time depending on the number of points. </remarks>
        /// <returns></returns>
        IEnumerable<MetasysPoint> GetEquipmentPoints(Guid equipmentId, bool readAttributeValue = true);
        /// <inheritdoc cref="IMetasysClient.GetEquipmentPoints(Guid, bool)"/>
        Task<IEnumerable<MetasysPoint>> GetEquipmentPointsAsync(Guid equipmentId, bool readAttributeValue = true);

        // GetSpaceEquipment ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of equipment that serve the specified space.
        /// </summary>
        /// <remarks>
        /// This method is deprecated (use the corresponding one in Equipments service).
        /// </remarks>
        /// <param name="spaceId"></param>
        IEnumerable<MetasysObject> GetSpaceEquipment(Guid spaceId);
        /// <inheritdoc cref="IMetasysClient.GetSpaceEquipment(Guid)"/>
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
    }
}
