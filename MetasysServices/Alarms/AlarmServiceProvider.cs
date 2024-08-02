using Flurl;
using Flurl.Http;
using JohnsonControls.Metasys.BasicServices.Utils;
using log4net.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Provide alarm item for the endpoints of the Metasys Alarm API.
    /// </summary>
    public sealed class AlarmServiceProvider : BasicServiceProvider, IAlarmsService
    {
        private readonly CultureInfo _CultureInfo = new CultureInfo("en-US");

        /// <summary>
        /// Initializes a new instance of <see cref="AlarmServiceProvider"/> with supplied data.
        /// </summary>
        /// <param name="client">The FlurlClient to get response from URL.</param>
        /// <param name="version">The server's Api version.</param>
        /// <param name="logClientErrors">Set this flag to false to disable logging of client errors.</param>
        public AlarmServiceProvider(IFlurlClient client, ApiVersion version, bool logClientErrors = true) : base(client, version, logClientErrors)
        {
        }

        //Get ----------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public PagedResult<Alarm> Get(AlarmFilter alarmFilter)
        {
            return GetAsync(alarmFilter).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Alarm>> GetAsync(AlarmFilter alarmFilter)
        {
            CheckVersion(Version);
            // This method is valid only for API v2 or v3.
            if (Version == ApiVersion.v2 || Version == ApiVersion.v3)
            {
                List<Alarm> alarms = new List<Alarm>();
                var response = await GetPagedResultsAsync<Alarm>("alarms", ToDictionary(alarmFilter)).ConfigureAwait(false);
                if (Version > ApiVersion.v2)
                {
                    foreach (var item in response.Items)
                    {
                        alarms.Add(CreateItem(item));
                    }
                    response = new PagedResult<Alarm>
                    {
                        Items = alarms,
                        CurrentPage = response.CurrentPage,
                        PageCount = response.PageCount,
                        PageSize = response.PageSize,
                        Total = response.Total
                    };
                }
                return response;
            }
            else
            {
                throw new MetasysUnsupportedApiVersion(Version.ToString());
            };
        }

        /// <inheritdoc/>
        public PagedResult<Alarm> Get(AlarmFilterV4Plus alarmFilter)
        {
            return GetAsync(alarmFilter).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Alarm>> GetAsync(AlarmFilterV4Plus alarmFilter)
        {
            CheckVersion(Version);
            // This method is valid only from API v4 on.
            if (Version >= ApiVersion.v4)
            {
                List<Alarm> alarms = new List<Alarm>();
                var response = await GetPagedResultsAsync<Alarm>("alarms", ToDictionary(alarmFilter)).ConfigureAwait(false);
                if (Version > ApiVersion.v2)
                {
                    foreach (var item in response.Items)
                    {
                        alarms.Add(CreateItem(item));
                    }
                    response = new PagedResult<Alarm>
                    {
                        Items = alarms,
                        CurrentPage = response.CurrentPage,
                        PageCount = response.PageCount,
                        PageSize = response.PageSize,
                        Total = response.Total
                    };
                }
                return response;
            }
            else
            {
                throw new MetasysUnsupportedApiVersion(Version.ToString());
            };
        }


        //FindById ------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public Alarm FindById(Guid alarmId)
        {
            return FindByIdAsync(alarmId).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<Alarm> FindByIdAsync(Guid alarmId)
        {
            CheckVersion(Version);

            var response = await GetRequestAsync("alarms", null, alarmId).ConfigureAwait(false);
            if (response["items"] != null) response = response["items"];

            var alarmData = JsonConvert.DeserializeObject<Alarm>(response.ToString());
            if (Version > ApiVersion.v2)
            {
                alarmData = CreateItem(alarmData);
            }
            return alarmData;
        }

        /// <inheritdoc/>
        public PagedResult<Alarm> GetForObject(ObjectId objectId, AlarmFilter alarmFilter)
        {
            return GetForObjectAsync(objectId, alarmFilter).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Alarm>> GetForObjectAsync(ObjectId objectId, AlarmFilter alarmFilter)
        {
            CheckVersion(Version);

            List<Alarm> alarms = new List<Alarm>();
            var response = await GetPagedResultsAsync<Alarm>("objects", ToDictionary(alarmFilter), objectId, "alarms").ConfigureAwait(false);
            if (Version > ApiVersion.v2)
            {
                foreach (var item in response.Items)
                {
                    alarms.Add(CreateItem(item));
                }
                response = new PagedResult<Alarm>
                {
                    Items = alarms,
                    CurrentPage = response.CurrentPage,
                    PageCount = response.PageCount,
                    PageSize = response.PageSize,
                    Total = response.Total
                };
            }
            return response;
        }

        /// <inheritdoc/>
        public PagedResult<Alarm> GetForNetworkDevice(ObjectId networkDeviceId, AlarmFilter alarmFilter)
        {
            return GetForNetworkDeviceAsync(networkDeviceId, alarmFilter).GetAwaiter().GetResult();
        }
        /// <inheritdoc/>
        public async Task<PagedResult<Alarm>> GetForNetworkDeviceAsync(ObjectId networkDeviceId, AlarmFilter alarmFilter)
        {
            CheckVersion(Version);

            List<Alarm> alarms = new List<Alarm>();
            var response = await GetPagedResultsAsync<Alarm>("networkDevices", ToDictionary(alarmFilter), networkDeviceId, "alarms").ConfigureAwait(false);
            if (Version > ApiVersion.v2)
            {
                foreach (var item in response.Items)
                {
                    alarms.Add(CreateItem(item));
                }

                response = new PagedResult<Alarm>
                {
                    Items = alarms,
                    CurrentPage = response.CurrentPage,
                    PageCount = response.PageCount,
                    PageSize = response.PageSize,
                    Total = response.Total
                };
            }
            return response;
        }

        /// <inheritdoc/>
        private void Edit(Guid alarmId, ActivityManagementStatusEnum action, string annotationText = null)
        {
            EditAsync(alarmId, action, annotationText).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        private async Task EditAsync(Guid alarmId, ActivityManagementStatusEnum action, string annotationText = null)
        {
            CheckVersion(Version);

            if (Version > ApiVersion.v3)
            {
                JObject body = new JObject();
                body.Add(propertyName: "activityManagementStatus", value: action.ToString());
                if (annotationText != null)
                {
                    body.Add(propertyName: "annotationText", value: annotationText);
                }

                var response = await Client.Request(new Url("alarms")
                .AppendPathSegments(alarmId))
                .PatchJsonAsync(body)
                .ConfigureAwait(false);
            }
            else
            {
                throw new MetasysUnsupportedApiVersion(Version.ToString());
            }
        }

        /// <inheritdoc/>
        public void Discard(Guid alarmId, string annotationText = null)
        {
            DiscardAsync(alarmId, annotationText).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task DiscardAsync(Guid alarmId, string annotationText = null)
        {
            CheckVersion(Version);

            if (Version > ApiVersion.v3)
            {
                JObject body = new JObject();
                body.Add(propertyName: "activityManagementStatus", value: ActivityManagementStatusEnum.discarded.ToString());
                if (annotationText != null)
                {
                    body.Add(propertyName: "annotationText", value: annotationText);
                }

                var response = await Client.Request(new Url("alarms")
                .AppendPathSegments(alarmId))
                .PatchJsonAsync(body)
                .ConfigureAwait(false);
            }
            else
            {
                throw new MetasysUnsupportedApiVersion(Version.ToString());
            }
        }

        /// <inheritdoc/>
        public void Acknowledge(Guid alarmId, string annotationText = null)
        {
            AcknowledgeAsync(alarmId, annotationText).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task AcknowledgeAsync(Guid alarmId, string annotationText = null)
        {
            CheckVersion(Version);

            if (Version > ApiVersion.v3)
            {
                JObject body = new JObject();
                body.Add(propertyName: "activityManagementStatus", value: ActivityManagementStatusEnum.acknowledged.ToString());
                if (annotationText != null)
                {
                    body.Add(propertyName: "annotationText", value: annotationText);
                }

                var response = await Client.Request(new Url("alarms")
                .AppendPathSegments(alarmId))
                .PatchJsonAsync(body)
                .ConfigureAwait(false);
            }
            else
            {
                throw new MetasysUnsupportedApiVersion(Version.ToString());
            }
        }

        /// <inheritdoc/>
        public IEnumerable<AlarmAnnotation> GetAnnotations(Guid alarmId)
        {
            return GetAnnotationsAsync(alarmId).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlarmAnnotation>> GetAnnotationsAsync(Guid alarmId)
        {
            CheckVersion(Version);

            // Retrieve JSON collection of Annotation
            var annotations = await GetAllAvailablePagesAsync("alarms", null, alarmId.ToString(), "annotations").ConfigureAwait(false);
            List<AlarmAnnotation> annotationsList = new List<AlarmAnnotation>();
            // Convert to a collection of AlarmAnnotation
            foreach (var token in annotations)
            {
                annotationsList.Add(CreateAlarmAnnotation(token));
            }
            return annotationsList;
        }

        private Alarm CreateItem(Alarm item)
        {
            try
            {
                if (Version > ApiVersion.v2)
                {
                    var triggerValue = new TriggerValue
                    {
                        Units = item.TriggerValue.Units != null ? ResourceManager.Localize(item.TriggerValue.Units, _CultureInfo) : null,
                        Value = item.TriggerValue.Value
                    };
                    item.TriggerValue = triggerValue;
                }

            }
            catch (ArgumentNullException e)
            {
                // Something went wrong on object parsing
                throw new MetasysObjectException(e);
            }
            return item;
        }

        private AlarmAnnotation CreateAlarmAnnotation(JToken token)
        {
            // Build AlarmAnnotation object
            AlarmAnnotation res = new AlarmAnnotation();
            try
            {
                //res.Text = GetJTokenValue(token, "text");
                //res.User = GetJTokenValue(token, "User");
                //res.CreationTime = GetJTokenDate(token, "creationTime");
                //res.Action = GetJTokenValue(token, "action");
                //res.AlarmUrl = GetJTokenValue(token, "alarmUrl");

                res.Text = token["text"].Value<string>();
                res.User = token["user"].Value<string>();
                res.CreationTime = token["creationTime"].Value<DateTime>();
                res.Action = token["action"].Value<string>();
                res.AlarmUrl = token["alarmUrl"].Value<string>();
            }
            catch (Exception e)
            {
                throw new MetasysObjectException(token.ToString(), e);
            }
            return res;
        }

    }
}
