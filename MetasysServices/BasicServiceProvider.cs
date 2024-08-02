using Flurl;
using Flurl.Http;
using JohnsonControls.Metasys.BasicServices;
using JohnsonControls.Metasys.BasicServices.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Resources;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Base abstract class to be extended on specific provider implementation.
    /// </summary>
    public abstract class BasicServiceProvider : ObjectUtil
    {
        /// <summary>The http client.</summary>
        protected IFlurlClient Client;

        /// <inheritdoc/>
        public ApiVersion Version { get; set; }

        /// <summary>
        /// Min API version supported by this SDK.
        /// </summary>
        public ApiVersion MinVersionSupported { get; } = ApiVersion.v2;

        /// <summary>
        /// Max API version supported by this SDK.
        /// </summary>
        public ApiVersion MaxVersionSupported { get; } = ApiVersion.v5;

        /// <summary>
        /// The current Culture Used for localization.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// The log initiliazer.
        /// </summary>
        protected LogInitializer Log;

        private bool logClientErrors;
        /// <summary>
        /// Set this flag to false to disable logging of client errors.
        /// </summary>
        public bool LogClientErrors
        {
            get
            {
                return logClientErrors;
            }
            set
            {
                logClientErrors = value;
                if (logClientErrors)
                {
                    // Init logger only when the flag is enabled
                    Log = new LogInitializer(typeof(BasicServiceProvider));
                }
            }
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        /// <param name="logErrors">Set this flag to false to disable logging of client errors.</param>
        /// <remarks> Assume Client is initialized by extended class.</remarks>
        public BasicServiceProvider(bool logErrors = true)
        {
            LogClientErrors = logErrors;
        }

        /// <summary>
        /// Constructor for dedicated services with Flurl client initialization already performed.
        /// </summary>
        /// <param name="client">The Flurl client.</param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="logErrors">Set this flag to false to disable logging of client errors.</param>
        public BasicServiceProvider(IFlurlClient client, ApiVersion version, bool logErrors = true)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client), "FlurlClient can not be null.");
            Version = version;
            LogClientErrors = logErrors;
        }

        /// <summary>
        /// Return Metasys Object representation from a generic JSON object tree.
        /// </summary>
        /// <returns></returns>
        protected List<MetasysObject> ToMetasysObject(IEnumerable<TreeObject> objects, ApiVersion version, MetasysObjectTypeEnum? objectType = null)
        {
            Version = version;
            if (objects == null)
            {
                // Exit condition for recursion
                return null;
            }
            List<MetasysObject> metasysObjects = new List<MetasysObject>();
            foreach (var o in objects)
            {
                metasysObjects.Add(new MetasysObject(o.Item, Version, ToMetasysObject(o.Children, Version), type: objectType));
            }
            return metasysObjects;
        }

        /// <summary>
        /// Return Metasys Object representation from a generic JSON object.
        /// </summary>
        /// <returns></returns>
        protected MetasysObject ToMetasysObject(JToken item, ApiVersion version, MetasysObjectTypeEnum? objectType = null)
        {
            Version = version;
            return new MetasysObject(item, Version, null, type: objectType);
        }
        /// <summary>
        /// Return Metasys Object representation from a generic JSON object List.
        /// </summary>
        /// <returns></returns>
        protected List<MetasysObject> ToMetasysObject(List<JToken> items, ApiVersion version, MetasysObjectTypeEnum? type = null)
        {
            Version = version;
            List<MetasysObject> objects = new List<MetasysObject>();
            foreach (var i in items)
            {
                objects.Add(ToMetasysObject(i, Version, objectType: type));
            }
            return objects;
        }

        /// <summary>
        /// Return Network Device representation from a generic JSON object.
        /// </summary>
        /// <returns></returns>
        protected NetworkDevice ToNetworkDevice(JToken item, ApiVersion version)
        {
            Version = version;
            return new NetworkDevice(item, Version);
        }

        /// <summary>
        /// Return Network Device representation from a generic JSON object List.
        /// </summary>
        /// <returns></returns>
        protected List<NetworkDevice> ToNetworkDevice(List<JToken> items, ApiVersion version)
        {
            Version = version;
            List<NetworkDevice> objects = new List<NetworkDevice>();
            foreach (var i in items)
            {
                objects.Add(ToNetworkDevice(i, Version));
            }
            return objects;
        }

        /// <summary>
        /// Gets all child objects given a parent id asynchronously by requesting each available page.
        /// Level indicates how deep to retrieve objects.
        /// </summary>
        /// <remarks>
        /// A level of 1 only retrieves immediate children of the parent object.
        /// </remarks>
        /// <param name="id">The id of the object.</param>
        /// <param name="parameters">Query string parameters in Key/Value format.</param>
        /// <param name="levels">The number of levels to retrieve children.</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        protected async Task<List<TreeObject>> GetObjectChildrenAsync(Object id, Dictionary<string, string> parameters = null, int levels = 1)
        {
            if (levels < 1)
            {
                return null;
            }
            List<TreeObject> objects = new List<TreeObject>() { }; // Contains the couple of parent/children JToken
            bool hasNext = true;
            int page = 1;
            if (parameters == null)
            {
                // Init dictionary to add page parameter when not already initialized in input parameters.
                parameters = new Dictionary<string, string>();
            }
            while (hasNext)
            {
                hasNext = false;
                parameters["page"] = page.ToString();
                var response = await GetPagedResultsAsync<JToken>("objects", parameters, id, "objects").ConfigureAwait(false);
                //List<JToken> listOfItems = response.Items;
                //if (Version > ApiVersion.v3)
                //{
                //    listOfItems = response.Items[0].Children("Items");
                //}
                foreach (var item in response.Items)
                {
                    List<TreeObject> children = null;
                    if (levels - 1 > 0)
                    {
                        if (item["id"] == null)
                        {
                            throw new MetasysObjectException(response.ToString(), null);
                        }
                        var objId = ParseObjectIdentifier(item["id"]);
                        children = await GetObjectChildrenAsync(objId, null, levels - 1).ConfigureAwait(false);
                    }
                    objects.Add(new TreeObject { Item = item, Children = children });
                }
                if (response.CurrentPage < response.PageCount)
                {
                    hasNext = true;
                    page++;
                }
            }
            return objects;
        }

        /// <summary>
        /// Gets all resource types asynchronously.
        /// </summary>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        public async Task<IEnumerable<MetasysObjectType>> GetResourceTypesAsync(string resource, string pathSegment)
        {
            List<MetasysObjectType> types = new List<MetasysObjectType>() { };
            try
            {
                var response = await Client.Request(new Url(resource)
                    .AppendPathSegment(pathSegment))
                    .GetJsonAsync<JToken>()
                    .ConfigureAwait(false);
                try
                {
                    if (Version < ApiVersion.v4)
                    {
                        // The response is a list of typeUrls, not the type data
                        var list = response["items"] as JArray;
                        foreach (var item in list)
                        {
                            try
                            {
                                JToken typeToken = item;
                                // Retrieve type token from url (when available) and construct Metasys Object Type
                                if (item["typeUrl"] != null)
                                {
                                    var url = item["typeUrl"].Value<string>();
                                    typeToken = await GetWithFullUrl(url).ConfigureAwait(false);
                                }
                                var type = GetType(typeToken);
                                types.Add(type);
                            }
                            catch (System.ArgumentNullException e)
                            {
                                throw new MetasysHttpParsingException(response.ToString(), e);
                            }
                        }
                    }
                    else
                    {
                        var item = response["item"];
                        var members = item["members"];
                        dynamic kvpList = JsonConvert.DeserializeObject<ExpandoObject>(members.ToString());
                        foreach (KeyValuePair<string, object> kvp in kvpList)
                        {
                            if (kvp.Key.Length > 0)
                            {
                                var itm = kvp.Value as IDictionary<string, object>;
                                String description = (itm.ContainsKey("name")) ? itm["name"].ToString() : String.Empty;
                                int id = int.Parse((itm.ContainsKey("value")) ? itm["value"].ToString() : Convert.ToString(-1));

                                var type = GetType(id, description, kvp.Key);
                                types.Add(type);
                            }
                        }
                    }
                }
                catch (System.NullReferenceException e)
                {
                    throw new MetasysHttpParsingException(response.ToString(), e);
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return types;
        }

        /// <summary>
        /// Gets the type from a token retrieved from a typeUrl
        /// </summary>
        /// <param name="typeToken"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysObjectTypeException"></exception>
        protected MetasysObjectType GetType(JToken typeToken)
        {
            try
            {
                if (typeToken != null || typeToken == null)
                {
                    //string description = (typeToken.Contains("description") && typeToken["description"] != null) ? typeToken["description"].Value<string>(): "";
                    //int id = (typeToken.Contains("id") && typeToken["id"] != null) ?  typeToken["id"].Value<int>() : -1;
                    //string key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
                    string description = typeToken["description"].Value<string>();
                    int id = typeToken["id"].Value<int>();
                    //string key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
                    string key = GetObjectTypeEnumeration(description);

                    if (key.Length > 0)
                    {
                        string translation = Localize(key);
                        if (translation != key)
                        {
                            // A translation was found
                            description = translation;
                        }
                    }
                    return new MetasysObjectType(id, key, description);
                }
                else
                {
                    return new MetasysObjectType(-1, "", "");
                }
            }
            catch (Exception e) when (e is System.ArgumentNullException
                || e is System.NullReferenceException || e is System.FormatException)
            {
                throw new MetasysObjectTypeException(typeToken.ToString(), e);
            }
        }

        /// <inheritdoc/>
        public string GetObjectTypeEnumeration(string resource)
        {
            // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
            return Utils.ResourceManager.GetObjectTypeEnumeration(resource);
        }

        /// <inheritdoc/>
        public string Localize(string resource, CultureInfo cultureInfo = null)
        {
            // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
            return Utils.ResourceManager.Localize(resource, cultureInfo ?? Culture);
        }


        /// <inheritdoc/>
        protected IEnumerable<MetasysEnumValue> GetEnumValues(String enumerationKey)
        {
            return GetEnumValuesAsync(enumerationKey).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        protected async Task<IEnumerable<MetasysEnumValue>> GetEnumValuesAsync(String enumerationKey)
        {
            List<MetasysEnumValue> enums = new List<MetasysEnumValue>() { };

            if (Version < ApiVersion.v4) { throw new MetasysUnsupportedApiVersion(Version.ToString()); }

            try
            {
                var response = await Client.Request(new Url("enumerations")
                    .AppendPathSegment(enumerationKey))
                    .GetJsonAsync<JToken>()
                    .ConfigureAwait(false);
                try
                {
                    var item = response["item"];
                    var members = item["members"];
                    dynamic kvpList = JsonConvert.DeserializeObject<ExpandoObject>(members.ToString());
                    foreach (KeyValuePair<string, object> kvp in kvpList)
                    {
                        if (kvp.Key.Length > 0)
                        {
                            var itm = kvp.Value as IDictionary<string, object>;
                            String key = kvp.Key;
                            String name = (itm.ContainsKey("name")) ? itm["name"].ToString() : String.Empty;
                            int value = int.Parse((itm.ContainsKey("value")) ? itm["value"].ToString() : Convert.ToString(-1));

                            var enumValue = new MetasysEnumValue(key, name, value);
                            enums.Add(enumValue);
                        }
                    }
                }
                catch (System.NullReferenceException e)
                {
                    throw new MetasysHttpParsingException(response.ToString(), e);
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return enums;
        }

        /// <summary>
        /// Gets the type from the values of the paramenters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="key"></param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysObjectTypeException"></exception>
        protected MetasysObjectType GetType(int id, String description, String key)
        {
            try
            {
                if (id >= 0 && description.Length > 0)
                {
                    if (String.IsNullOrEmpty(key))
                    {
                        key = description.Length > 0 ? GetObjectTypeEnumeration(description) : "";
                    }
                    if (key.Length > 0)
                    {
                        string translation = Localize(key);
                        if (translation != key)
                        {
                            // A translation was found
                            description = translation;
                        }
                    }
                    return new MetasysObjectType(id, key, description);
                }
                else
                {
                    return new MetasysObjectType(-1, "", "");
                }
            }
            catch (Exception e) when (e is System.ArgumentNullException
                || e is System.NullReferenceException || e is System.FormatException)
            {
                throw new MetasysObjectTypeException(id.ToString() + " " + description, e);
            }
        }

        /// <summary>
        /// Gets a resource given the full url asynchronously.
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="MetasysHttpException"></exception>
        protected async Task<JToken> GetWithFullUrl(string url)
        {
            string requestUrl = url.Replace(Client.BaseUrl, "");
            try
            {
                var item = await Client.Request(requestUrl)
                    .GetJsonAsync<JToken>()
                    .ConfigureAwait(false);
                return item;
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return null;
        }

        /// <summary>
        /// Parses a JToken containing a string and returns the value as an <see cref="ObjectId"/>.
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="MetasysGuidException"></exception>
        /// <returns>The <see cref="ObjectId"/> contained in the token.</returns>
        protected ObjectId ParseObjectIdentifier(JToken token)
        {
            string str = null;
            try
            {
                if (token.Type == JTokenType.String)
                {
                    str = token.Value<string>();
                    return str;
                }
            }
            catch (Exception e) when (e is System.ArgumentNullException ||
                e is System.ArgumentException || e is System.FormatException)
            {
                // This still throws a MetasysGuidException rather than a new type of
                // exception to support backward compatibility even though we don't
                // use Guids any more.
                throw new MetasysGuidException(str, e);
            }

            throw new MetasysGuidException(token.ToString(), null);
        }

        /// <summary>
        /// Generic request for the given resource asynchronously.
        /// </summary>
        /// <param name="resource">The main resource to read.</param>
        /// <param name="parameters">Query string parameters in Key/Value format.</param>
        /// <param name="pathSegments">Path segments to be used in combination with the main resource.</param>
        /// <returns></returns>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        protected async Task<JToken> GetRequestAsync(string resource, Dictionary<string, string> parameters = null, params object[] pathSegments)
        {

            JToken response = null;
            // Create URL with base resource
            Url url = new Url(resource);
            // Concatenate segments with base resource url
            url.AppendPathSegments(pathSegments);
            // Set query parameters according to the input dictionary
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    url.SetQueryParam(p.Key, p.Value);
                }
            }

            try
            {
                //Client.WithTimeout(300);
                response = await Client.Request(url)
                .GetJsonAsync<JToken>()
                .ConfigureAwait(false);
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return response;
        }

        /// <summary>
        /// Get typed items for the given resource asynchronously.
        /// </summary>
        /// <remarks>Optionally accepts query string parameters and additional path segments.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource">The main resource to read.</param>
        /// <param name="parameters">Query string parameters in Key/Value format.</param>
        /// <param name="pathSegments">Path segments to be used in combination with the main resource.</param>
        /// <returns></returns>
        protected async Task<PagedResult<T>> GetPagedResultsAsync<T>(string resource, Dictionary<string, string> parameters, params object[] pathSegments)
        {
            var response = await GetRequestAsync(resource, parameters, pathSegments).ConfigureAwait(false);
            return new PagedResult<T>(response);
        }


        /// <summary>
        /// Gets all items for the given resource asynchronously by requesting each available page.
        /// </summary>
        /// <param name="resource">The main resource to read.</param>
        /// <param name="parameters">Query string parameters in Key Value format.</param>
        /// <param name="pathSegments">Path segments to be used in combination with the main resource.</param>
        /// <returns></returns>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        protected async Task<List<JToken>> GetAllAvailablePagesAsync(string resource, Dictionary<string, string> parameters = null, params string[] pathSegments)
        {
            bool hasNext = true;
            bool buildAggregateResponse = true;
            List<JToken> aggregatedResponse = new List<JToken>();

            int page = 1;
            int pageSize = 1000;

            // Init our dictionary for paging
            if (parameters == null) parameters = new Dictionary<string, string>();

            if (!parameters.ContainsKey("page"))
            {
                parameters.Add("page", page.ToString());
            }
            else
            {
                Int32.TryParse(parameters["page"], out page);
                if (page > 0)
                    buildAggregateResponse = false; // This handles the case when it has been passed a specific Page number so the response will not be aggregate
            }
            if (!parameters.ContainsKey("pageSize"))
            {
                parameters.Add("pageSize", pageSize.ToString());
            }

            while (hasNext)
            {
                hasNext = false;
                // Just overwrite page parameter
                parameters["page"] = page.ToString();
                var response = await GetPagedResultsAsync<JToken>(resource, parameters, pathSegments).ConfigureAwait(false);
                var total = response.Total;
                if (total > 0)
                {
                    aggregatedResponse.AddRange(response.Items);
                    if ((response.CurrentPage < response.PageCount) & (buildAggregateResponse))
                    {
                        hasNext = true;
                        page++;
                    }
                    else
                    {
                        if (page > response.CurrentPage) aggregatedResponse.Clear(); //This case happens when the specified Page is higher than the tot number of pages
                    }
                }
            }
            return aggregatedResponse;
        }

        /// <summary>
        /// Throws a Metasys Exception from a Flurl.Http exception.
        /// </summary>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        protected void ThrowHttpException(FlurlHttpException e)
        {
            if (LogClientErrors)
            {
                // Perform logging only when enabled by BasicServiceProvider Settings.
                Log.Logger.Error(e.Message);
            }
            if (e.Call.Response != null && e.Call.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MetasysHttpNotFoundException(e);
            }
            if (e.GetType() == typeof(FlurlParsingException))
            {
                throw new MetasysHttpParsingException((FlurlParsingException)e);
            }
            else if (e.GetType() == typeof(FlurlHttpTimeoutException))
            {
                throw new MetasysHttpTimeoutException((FlurlHttpTimeoutException)e);
            }
            else
            {
                throw new MetasysHttpException(e);
            }
        }

        /// <summary>
        /// Convert a generic object to a dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            // Check and in case removes some items not allowed for a specific request
            String activityType;
            if (dictionary.TryGetValue("ActivityType", out activityType))
            {
                if (activityType == "audit")
                {
                    dictionary.Remove("IncludeAcknowledgementRequired");
                    dictionary.Remove("IncludeAcknowledgementNotRequired");
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Perform multiple requests (GET) to the Server with a single HTTP call asynchronously.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ids"></param>
        /// <param name="resources"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        protected async Task<JToken> GetBatchRequestAsync(string endpoint, IEnumerable<ObjectId> ids, IEnumerable<string> resources, params string[] paths)
        {
            // Create URL with base resource
            Url url = new Url(endpoint);
            // Concatenate batch segment to use batch request and prepare the list of requests
            url.AppendPathSegments("batch");
            var objectsRequests = new List<ObjectRequest>();
            // Concatenate batch segment to use batch request and prepare the list of requests
            foreach (var id in ids)
            {
                foreach (var r in resources)
                {
                    Url relativeUrl = new Url(id.ToString());
                    relativeUrl.AppendPathSegments(paths); // e.g. "00000000-0000-0000-0000-000000000001/attributes"
                    relativeUrl.AppendPathSegment(r); // e.g. "00000000-0000-0000-0000-000000000001/attributes/presentValue"
                    // Use the object id concatenated to the resource to uniquely identify each request
                    objectsRequests.Add(new ObjectRequest { Id = id.ToString() + '_' + r, RelativeUrl = relativeUrl });
                }
            }
            JToken responseToken = null;
            try
            {
                // Post the list of requests and return responses as JToken
                var response = await Client.Request(url)
                                            .PostJsonAsync(new BatchRequest { Method = "GET", Requests = objectsRequests })
                                            .ConfigureAwait(false);
                responseToken = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return responseToken;
        }

        /// <summary>
        /// Perform multiple requests (POST) to the Server with a single HTTP call asynchronously.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="requests"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        protected async Task<JToken> PostBatchRequestAsync(string endpoint, IEnumerable<BatchRequestParam> requests, params string[] paths)
        {
            // Create URL with base resource
            Url url = new Url(endpoint);
            // Concatenate batch segment to use batch request and prepare the list of requests
            url.AppendPathSegments("batch");
            var objectsRequests = new List<ObjectRequest>();
            // Concatenate batch segment to use batch request and prepare the list of requests
            foreach (var r in requests)
            {
                Url relativeUrl = new Url(r.ObjectId.ToString());
                relativeUrl.AppendPathSegments(paths); // e.g. "00000000-0000-0000-0000-000000000001/annotations"

                object body;
                switch (paths[0])
                {
                    case "annotations":
                        string text = r.Resource;
                        body = new { text };
                        break;
                    default:
                        body = null;
                        break;
                };

                // Use the object id concatenated to the resource to uniquely identify each request
                objectsRequests.Add(new ObjectRequest
                {
                    Id = r.ObjectId.ToString() + '_' + r.Resource,
                    RelativeUrl = relativeUrl,
                    Body = body
                });

                //Body = new ObjectBody {Text = r.Resource }
            }

            JToken responseToken = null;
            try
            {
                // Post the list of requests and return responses as JToken
                var response = await Client.Request(url)
                                            .PostJsonAsync(new BatchRequest { Method = "POST", Requests = objectsRequests })
                                            .ConfigureAwait(false);

                responseToken = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return responseToken;
        }

        /// <summary>
        /// Perform multiple requests (PUT or PATCH) to the Server with a single HTTP call asynchronously.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="requests"></param>
        /// <param name="paths"></param>
        /// <returns></returns>

        protected async Task<JToken> PutBatchRequestAsync(string endpoint, IEnumerable<BatchRequestParam> requests, params string[] paths)
        {
            Boolean isDiscard = false;
            // Create URL with base resource
            Url url = new Url(endpoint);
            // Concatenate batch segment to use batch request and prepare the list of requests
            url.AppendPathSegments("batch");
            var objectsRequests = new List<ObjectRequest>();
            // Concatenate batch segment to use batch request and prepare the list of requests
            foreach (var r in requests)
            {
                Url relativeUrl = new Url(r.ObjectId.ToString());
                relativeUrl.AppendPathSegments(paths); // e.g. "00000000-0000-0000-0000-000000000001/discard"

                object body;
                switch (paths[0])
                {
                    case "discard":
                        string annotationText = r.Resource;
                        body = new { annotationText };
                        isDiscard = true;
                        break;
                    default:
                        body = null;
                        break;
                };

                // Use the object id concatenated to the resource to uniquely identify each request
                objectsRequests.Add(new ObjectRequest
                {
                    Id = r.ObjectId.ToString() + '_' + r.Resource,
                    RelativeUrl = relativeUrl,
                    Body = body
                });

                //Body = new ObjectBody {Text = r.Resource }
            }

            JToken responseToken = null;
            try
            {
                if (isDiscard && Version == ApiVersion.v4)
                {
                    // Post the list of requests and return responses as JToken
                    var response = await Client.Request(url)
                                                .PostJsonAsync(new BatchRequest { Method = "PATCH", Requests = objectsRequests })
                                                .ConfigureAwait(false);

                    responseToken = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                }
                else
                {
                    // Post the list of requests and return responses as JToken
                    var response = await Client.Request(url)
                                                .PostJsonAsync(new BatchRequest { Method = "PUT", Requests = objectsRequests })
                                                .ConfigureAwait(false);

                    responseToken = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return responseToken;
        }

        /// <summary>
        /// Perform multiple requests (PATCH) to the Server with a single HTTP call asynchronously.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="requests"></param>
        /// <returns></returns>
        protected async Task<JToken> PatchBatchRequestAsync(string endpoint, IEnumerable<BatchRequestParam> requests)
        {
            // Create URL with base resource
            Url url = new Url(endpoint);
            // Concatenate batch segment to use batch request and prepare the list of requests
            url.AppendPathSegments("batch");
            var objectsRequests = new List<ObjectRequest>();
            // Concatenate batch segment to use batch request and prepare the list of requests
            foreach (var r in requests)
            {
                string activityManagementStatus = "";
                if (r.ActivityManagementStatus == "discarded" || r.ActivityManagementStatus == "acknowledged")
                {
                    activityManagementStatus = r.ActivityManagementStatus;
                }
                else
                {
                    //exception
                }

                string annotationText = r.Resource;

                //Define the specific relativeUrl (NOTE: since v5 it contains the 'full' base Url)
                Url relativeUrl = new Url(Client.BaseUrl);
                object body;
                switch (r.ActivityType)
                {
                    case "alarm":
                        relativeUrl.AppendPathSegments("alarms");
                        relativeUrl.AppendPathSegments(r.ObjectId.ToString());
                        break;
                    case "audit":
                        relativeUrl.AppendPathSegments("audits");
                        relativeUrl.AppendPathSegments(r.ObjectId.ToString());
                        break;
                    default:
                        break;
                };

                // Create the 'body' object
                if (activityManagementStatus.Length > 0 && annotationText.Length > 0)
                {
                    body = new { activityManagementStatus, annotationText };
                }
                else if (activityManagementStatus.Length > 0)
                {
                    body = new { activityManagementStatus };
                }
                else if (activityManagementStatus.Length > 0)
                {
                    body = new { annotationText };
                }
                else
                { body = null; }


                // Use the object id concatenated to the resource to uniquely identify each request
                objectsRequests.Add(new ObjectRequest
                {
                    Id = r.ObjectId.ToString() + '_' + r.Resource,
                    RelativeUrl = relativeUrl,
                    Body = body
                });
            }

            JToken responseToken = null;
            try
            {
                if (Version > ApiVersion.v4)
                {
                    // Post the list of requests and return responses as JToken
                    var response = await Client.Request(url)
                                                .PostJsonAsync(new BatchRequest { Method = "PATCH", Requests = objectsRequests })
                                                .ConfigureAwait(false);
                    responseToken = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                else
                {
                    //exception
                }
            }
            catch (FlurlHttpException e)
            {
                ThrowHttpException(e);
            }
            return responseToken;
        }


        /// <summary>
        /// Check if the selected API version is supported by the SDK
        /// </summary>
        protected void CheckVersion(ApiVersion version)
        {
            if (version < MinVersionSupported || version > MaxVersionSupported)
            { throw new MetasysUnsupportedApiVersion(version.ToString()); }
        }

    }
}
