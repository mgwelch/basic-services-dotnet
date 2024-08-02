using AutoMapper;
using JohnsonControls.Metasys.BasicServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using MetasysAttribute = JohnsonControls.Metasys.BasicServices.MetasysAttribute;

namespace JohnsonControls.Metasys.ComServices
{

    /// <summary>
    /// An COM HTTP client for consuming the most commonly used endpoints of the Metasys API.
    /// </summary>
    [ComVisible(true)]
    [Guid("0261C5FD-2473-4EC1-A78C-31A5C27C8163")]
    [ClassInterface(ClassInterfaceType.None)]
    public class LegacyMetasysClient : ILegacyMetasysClient
    {
        /// <summary>
        /// Metasys Basic Services client for underlying API calls
        /// </summary>
        protected IMetasysClient Client;
        /// <summary>
        /// Automapper instance for mapping Basic Services structures to registered COM DTO
        /// </summary>
        protected IMapper Mapper;

        /// <summary>
        /// Local instance of Trends service
        /// </summary>
        public ITrendService Trends;

        /// <summary>
        /// Creates a new LegacyMetasysClient.
        /// </summary>
        internal LegacyMetasysClient(IMetasysClient client)
        {
            Client = client;
            // Defines a Mapper From Basic Services structure to COM
            Mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MetasysObject, IComMetasysObject>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.Children, opt => opt.MapFrom(src => Mapper.Map<IComMetasysObject[]>(src.Children)));
                cfg.CreateMap<Variant, IComVariant>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.ArrayValue, opt => opt.MapFrom(src => Mapper.Map<IComVariant[]>(src.ArrayValue)));
                cfg.CreateMap<VariantMultiple, IComVariantMultiple>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.Values, opt => opt.MapFrom(src => Mapper.Map<IComVariant[]>(src.Values)));
                cfg.CreateMap<AccessToken, IComAccessToken>();
                cfg.CreateMap<Command, IComCommand>();
                cfg.CreateMap<MetasysObjectType, IComMetasysObjectType>();
                cfg.CreateMap<IComFilterAlarm, AlarmFilter>();
                cfg.CreateMap<IComAlarmFilterV4Plus, AlarmFilterV4Plus>();
                cfg.CreateMap<Alarm, IComAlarm>();
                cfg.CreateMap<Sample, IComSample>();
                cfg.CreateMap<IComTimeFilter, TimeFilter>();
                cfg.CreateMap<MetasysAttribute, IComMetasysAttribute>();
                cfg.CreateMap<PagedResult<Alarm>, IComPagedResult>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => Mapper.Map<IComAlarm[]>(src.Items)));
                cfg.CreateMap<PagedResult<Sample>, IComPagedResult>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => Mapper.Map<IComSample[]>(src.Items)));
                cfg.CreateMap<IComAuditFilter, AuditFilter>();
                cfg.CreateMap<Audit, IComAudit>();
                cfg.CreateMap<LegacyInfo, IComLegacyInfo>();
                cfg.CreateMap<AuditSignature, IComAuditSignature>();
                cfg.CreateMap<PagedResult<Audit>, IComPagedResult>()
                    // This is needed in order to correctly map to generic object reference to array, in order to correctly map to VBA
                    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => Mapper.Map<IComAudit[]>(src.Items)));
                cfg.CreateMap<MetasysPoint, IComMetasysPoint>();
                cfg.CreateMap<AlarmAnnotation, IComAlarmAnnotation>();
                cfg.CreateMap<AuditAnnotation, IComAuditAnnotation>();
                cfg.CreateMap<StreamMessage, IComStreamMessage>();
                cfg.CreateMap<MetasysEnumeration, IComMetasysEnumeration>();
                cfg.CreateMap<MetasysEnumValue, IComMetasysEnumValue>();
            }).CreateMapper();

            //Client.Streams.COVValueChanged += OnStreamCOVValueChanged;
            //Client.Streams.AlarmOccurred += OnStreamAlarmOccurred;
            //Client.Streams.AuditOccurred += OnStreamAuditOccurred;
        }

        // TryLogin -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComAccessToken TryLogin(string username, string password, bool refresh = true)
        {
            return Mapper.Map<IComAccessToken>(Client.TryLogin(username, password, refresh));
        }
        /// <inheritdoc/>
        public string TryLogin2(string username, string password, bool refresh = true)
        {
            string res;
            try
            {
                AccessToken accToken = Client.TryLogin(username, password, refresh);
                res = accToken.Token.ToString();
            }
            catch (Exception ex)
            {
                res = ex.StackTrace.ToString();
            }
            return res;
        }


        // TryLoginWithCredMan ------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComAccessToken TryLoginWithCredMan(string target, bool refresh = true)
        {
            return Mapper.Map<IComAccessToken>(Client.TryLogin(target, refresh));
        }

        // GetAccessToken -----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComAccessToken GetAccessToken()
        {
            return Mapper.Map<IComAccessToken>(Client.GetAccessToken());
        }

        // Refresh ------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComAccessToken Refresh()
        {
            return Mapper.Map<IComAccessToken>(Client.Refresh());
        }

        // GetServerTime ------------------------------------------------------------------------------------------------------------
        ///<inheritdoc/>
        public DateTime GetServerTime()
        {
            return Client.GetServerTime();
        }

        // Localize -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public string Localize(string resource, string cultureInfo = "en-US")
        {
            CultureInfo culture = new CultureInfo(cultureInfo);
            return Client.Localize(resource, culture);
        }


        #region "Activities" // =========================================================================================================
        //GetActivities -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetActivities(IComActivityFilter activityFilter)
        {
            var mapActivityFilter = Mapper.Map<ActivityFilter>(activityFilter);
            PagedResult<Activity> activityItems = Client.Activities.Get(mapActivityFilter);
            return Mapper.Map<IComPagedResult>(activityItems);
        }

        //ActivityActionMultiple ------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public string[] ActivityActionMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] requestParams)
        {
            // Note: MarshalAs decorator is needed when return type is void, otherwise will cause a VBA error on Automation type not supported when passing array
            var requests = new List<BatchRequestParam>();
            foreach (var request in requestParams)
            {
                string[] param = request.Split('|');
                if (param.GetUpperBound(0) >= 4)
                {
                    requests.Add(new BatchRequestParam { ObjectId = new Guid(param[0]), Resource = param[1], ActivityType = param[2], ActivityManagementStatus = param[4] });
                }
            }
            var response = Client.Activities.ActionMultiple(requests);

            string[] result = new string[response.Count()];
            int i = 0;
            foreach (var res in response)
            {
                result[i] = res.Id.ToString() + "|" + res.Status.ToString() + "|" + res.Annotation;
                i++;
            }
            return result;
        }

        #endregion


        #region "Alarms" // =========================================================================================================
        //GetAlarms -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetAlarms(IComFilterAlarm alarmFilter)
        {
            var mapAlarmFilter = Mapper.Map<AlarmFilter>(alarmFilter);
            PagedResult<Alarm> alarmItems = Client.Alarms.Get(mapAlarmFilter);
            return Mapper.Map<IComPagedResult>(alarmItems);
        }
        /// <inheritdoc />
        public IComPagedResult GetAlarms(IComAlarmFilterV4Plus alarmFilterV4Plus)
        {
            var mapAlarmFilterV4Plus = Mapper.Map<AlarmFilterV4Plus>(alarmFilterV4Plus);
            PagedResult<Alarm> alarmItems = Client.Alarms.Get(mapAlarmFilterV4Plus);
            return Mapper.Map<IComPagedResult>(alarmItems);
        }

        //GetSingleAlarm ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetSingleAlarm(string alarmId)
        {
            Guid guidAlarmId = Guid.Parse(alarmId);
            var alarmItem = Client.Alarms.FindById(guidAlarmId);
            return Mapper.Map<IComAlarm>(alarmItem);
        }

        //GetAlarmsForNetworkDevice -------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetAlarmsForNetworkDevice(string networkDeviceId, IComFilterAlarm alarmFilter)
        {
            Guid guidNetworkDeviceId = Guid.Parse(networkDeviceId);
            var mapAlarmFilterForObject = Mapper.Map<AlarmFilter>(alarmFilter);
            var alarmItems = Client.Alarms.GetForNetworkDevice(guidNetworkDeviceId, mapAlarmFilterForObject);
            return Mapper.Map<IComPagedResult>(alarmItems);
        }

        //GetAlarmsForObject --------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetAlarmsForObject(string objectId, IComFilterAlarm alarmFilter)
        {
            Guid guidObjectId = Guid.Parse(objectId);
            var mapAlarmFilterForAnObject = Mapper.Map<AlarmFilter>(alarmFilter);
            var alarmItems = Client.Alarms.GetForObject(guidObjectId, mapAlarmFilterForAnObject);
            return Mapper.Map<IComPagedResult>(alarmItems);
        }

        //GetAlarmAnnotations -------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetAlarmAnnotations(string alarmId)
        {
            Guid guidAlarmId = Guid.Parse(alarmId);
            var response = Client.Alarms.GetAnnotations(guidAlarmId);
            return Mapper.Map<IComAlarmAnnotation[]>(response);
        }

        //AcknowledgeAlarm -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void AcknowledgeAlarm(string alarmId, string annotationText = null)
        {
            Guid guid = Guid.Parse(alarmId);
            Client.Alarms.Acknowledge(guid, annotationText);
        }
        //DiscardAlarm -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void DiscardAlarm(string alarmId, string annotationText = null)
        {
            Guid guid = Guid.Parse(alarmId);
            Client.Alarms.Discard(guid, annotationText);
        }
        #endregion


        #region "Audits" // =========================================================================================================
        //GetAudits -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetAudits(IComAuditFilter auditFilter)
        {
            AuditFilter auditF = new AuditFilter
            {
                StartTime = DateTime.Parse(auditFilter.StartTime),
                EndTime = DateTime.Parse(auditFilter.EndTime)
            };
            if (!(auditFilter.Page is null)) auditF.Page = int.Parse(auditFilter.Page);
            if (!(auditFilter.PageSize is null)) auditF.PageSize = int.Parse(auditFilter.PageSize);
            auditF.Sort = auditFilter.Sort;
            auditF.ExcludeDiscarded = auditFilter.ExcludeDiscarded;

            PagedResult<Audit> auditItems = Client.Audits.Get(auditF);
            return Mapper.Map<IComPagedResult>(auditItems);
        }

        //GetSingleAudit ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetSingleAudit(string auditId)
        {
            Guid guidAuditId = Guid.Parse(auditId);
            var auditItem = Client.Audits.FindById(guidAuditId);
            return Mapper.Map<IComAudit>(auditItem);
        }

        //GetAuditsForObject --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComPagedResult GetAuditsForObject(string objectId, IComAuditFilter auditFilter)
        {
            Guid guidObjectId = Guid.Parse(objectId);
            var mapAuditFilterForAnObject = Mapper.Map<AuditFilter>(auditFilter);
            var auditItems = Client.Audits.GetForObject(guidObjectId, mapAuditFilterForAnObject);
            return Mapper.Map<IComPagedResult>(auditItems);
        }

        //GetAuditAnnotations -------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetAuditAnnotations(string auditId)
        {
            Guid guidAuditId = Guid.Parse(auditId);
            var response = Client.Audits.GetAnnotations(guidAuditId);
            return Mapper.Map<IComAuditAnnotation[]>(response);
        }

        //DiscardAudit --------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void DiscardAudit(string id, string annotationText)
        {
            Guid guid = new Guid(id);
            Client.Audits.Discard(guid, annotationText);
        }

        //DiscardAuditMultiple ------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public string[] DiscardAuditMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] requestParams)
        {
            // Note: MarshalAs decorator is needed when return type is void, otherwise will cause a VBA error on Automation type not supported when passing array
            var requests = new List<BatchRequestParam>();
            foreach (var request in requestParams)
            {
                string[] param = request.Split('|');
                requests.Add(new BatchRequestParam { ObjectId = new Guid(param[0]), Resource = param[1] });
            }
            var response = Client.Audits.DiscardMultiple(requests);

            string[] result = new string[response.Count()];
            int i = 0;
            foreach (var res in response)
            {
                result[i] = res.Id.ToString() + "|" + res.Status.ToString() + "|" + res.Annotation;
                i++;
            }
            return result;
        }

        //AddAuditAnnotations --------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void AddAuditAnnotation(string id, string text)
        {
            Guid guid = new Guid(id);
            Client.Audits.AddAnnotation(guid, text);
        }

        //AddAuditAnnotationMultiple ------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public string[] AddAuditAnnotationMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] requestParams)
        {
            // Note: MarshalAs decorator is needed when return type is void, otherwise will cause a VBA error on Automation type not supported when passing array
            var requests = new List<BatchRequestParam>();
            foreach (var request in requestParams)
            {
                string[] param = request.Split('|');
                requests.Add(new BatchRequestParam { ObjectId = new Guid(param[0]), Resource = param[1] });
            }
            var response = Client.Audits.AddAnnotationMultiple(requests);

            string[] result = new string[response.Count()];
            int i = 0;
            foreach (var res in response)
            {
                result[i] = res.Id.ToString() + "|" + res.Status.ToString() + "|" + res.Annotation;
                i++;
            }
            return result;
        }
        #endregion


        #region "Enumerations" // ===================================================================================================
        //GetEnumerations -----------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetEnumerations()
        {
            var response = Client.Enumerations.Get();
            return Mapper.Map<IComMetasysEnumeration[]>(response);
        }

        //CreateCustomEnumeration ---------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void CreateCustomEnumeration(string name, string[] values)
        {
            IEnumerable<String> ievalues = values;
            Client.Enumerations.Create(name, ievalues);
        }

        //GetEnumerationValues ------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetEnumerationValues(String id)
        {
            var response = Client.Enumerations.GetValues(id);
            return Mapper.Map<IComMetasysEnumValue[]>(response);
        }

        //EditCustomEnumeration -----------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void EditCustomEnumeration(string id, string name, string[] values)
        {
            IEnumerable<String> ievalues = values;
            Client.Enumerations.Edit(id, name, ievalues);
        }

        //ReplaceCustomEnumeration --------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void ReplaceCustomEnumeration(string id, string name, string[] values)
        {
            IEnumerable<String> ievalues = values;
            Client.Enumerations.Replace(id, name, ievalues);
        }

        //DeleteCustomEnumeration ---------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public void DeleteCustomEnumeration(string id)
        {
            Client.Enumerations.Delete(id);
        }
        #endregion


        #region "Equipments" // =====================================================================================================
        //GetEquipment --------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetEquipment(int page = 1, int pageSize = 100)
        {
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.Equipments.Get(page, pageSize);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetSingleEquipment --------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetSingleEquipment(string equipmentId)
        {
            Guid guid = Guid.Parse(equipmentId);
            var res = Client.Equipments.FindById(guid);
            return Mapper.Map<IComMetasysObject>(res);
        }

        //GetEquipmentsServedByEquipment --------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetEquipmentsServedByEquipment(string equipmentId)
        {
            Guid guid = new Guid(equipmentId);
            var res = Client.Equipments.GetServedByEquipment(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetEquipmentPoints --------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetEquipmentPoints(string equipmentId, bool ReadAttributeValue = true)
        {
            Guid guid = new Guid(equipmentId);
            var res = Client.Equipments.GetPoints(guid, ReadAttributeValue);
            return Mapper.Map<IComMetasysPoint[]>(res);
        }

        //GetEquipmentServingASpace -------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetEquipmentsServingASpace(string spaceId)
        {
            Guid guid = new Guid(spaceId);
            var res = Client.Equipments.GetServingASpace(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }
        /// <inheritdoc/>
        public object GetSpaceEquipment(string spaceId) //This method is deprecated and you should use 'GetEquipmentServingASpace()'
        {
            return GetEquipmentsServingASpace(spaceId);
        }

        //GetEquipmentHostedByNetworkDevice -----------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetEquipmentsHostedByNetworkDevice(string networkDeviceId)
        {
            Guid guid = new Guid(networkDeviceId);
            var res = Client.Equipments.GetHostedByNetworkDevice(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetEquipmentsServingAnEquipment -------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetEquipmentsServingAnEquipment(string equipmentId)
        {
            Guid guid = new Guid(equipmentId);
            var res = Client.Equipments.GetServingAnEquipment(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }
        #endregion


        #region "NetworkDevices" // =================================================================================================
        //GetNetworkDevices ---------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetNetworkDevices(string type = null)
        {
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.NetworkDevices.Get(type).ToList();
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetNetworkDeviceTypes -----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetNetworkDeviceTypes(string type = null)
        {
            var res = Client.NetworkDevices.GetTypes();
            return Mapper.Map<IComMetasysObjectType[]>(res);
        }

        //GetSingleNetworkDevice ----------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetSingleNetworkDevice(string networkDeviceId)
        {
            Guid guid = Guid.Parse(networkDeviceId);
            var res = Client.NetworkDevices.FindById(guid);
            return Mapper.Map<IComMetasysObject>(res);
        }

        //GetNetworkDevicesHostingAnEquipment ---------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetNetworkDevicesHostingAnEquipment(string equipmentId)
        {
            Guid guid = new Guid(equipmentId);
            var res = Client.NetworkDevices.GetHostingAnEquipment(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetNetworkDevicesChildren -------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetNetworkDevicesChildren(string networkDeviceId)
        {
            Guid guid = new Guid(networkDeviceId);
            var res = Client.NetworkDevices.GetChildren(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetNetworkDevicesServingASpace --------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetNetworkDevicesServingASpace(string spaceId)
        {
            Guid guid = new Guid(spaceId);
            var res = Client.NetworkDevices.GetServingASpace(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }
        #endregion


        #region "Objects" // ========================================================================================================
        //GetObjects ----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetObjects(string id, int levels = 1, bool includeInternalObjects = false, bool includeExtensions = false)
        {
            Guid guid = new Guid(id);
            var res = Client.GetObjects(guid, levels, includeInternalObjects, includeExtensions).ToList();
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        /// <inheritdoc/>
        public object GetObjects(string id, string type)
        {
            Guid guid = new Guid(id);
            var res = Client.GetObjects(guid, type).ToList();
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetObjectIdentifier -------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public string GetObjectIdentifier(string itemReference)
        {
            ObjectId? res = Client.GetObjectIdentifier(itemReference);
            return res?.ToString();
        }

        //GetCommands ---------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetCommands(string id)
        {
            Guid guid = new Guid(id);
            var res = Client.GetCommands(guid);
            return Mapper.Map<IComCommand[]>(res);
        }

        //GetObjectTypeEnumeration --------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public string GetObjectTypeEnumeration(string resource)
        {
            // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
            return Client.GetObjectTypeEnumeration(resource);
        }

        //GetCommandEnumeration -----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public string GetCommandEnumeration(string resource)
        {
            // Priority is the cultureInfo parameter if available, otherwise MetasysClient culture.
            return Client.GetCommandEnumeration(resource);
        }

        //ReadProperty --------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComVariant ReadProperty(string id, string attributeName)
        {
            // Parse Id and generate GUID
            Guid guid = new Guid(id);
            var res = Client.ReadProperty(guid, attributeName);
            return Mapper.Map<IComVariant>(res);
        }

        //ReadPropertyMultiple ------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object ReadPropertyMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] ids, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] attributeNames)
        {
            // Note: MarshalAs decorator is needed for arrays, otherwise will cause a VBA app crash
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.ReadPropertyMultiple(ids.Select(id => (ObjectId)id), attributeNames);
            return Mapper.Map<IComVariantMultiple[]>(res);
        }

        //WriteProperty -------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void WriteProperty(string id, string attributeName, string newValue)
        {
            Client.WriteProperty(new Guid(id), attributeName, newValue);
        }

        //WritePropertyMultiple -----------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void WritePropertyMultiple([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] ids, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] attributes, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] attributeValues)
        {
            // Note: MarshalAs decorator is needed when return type is void, otherwise will cause a VBA error on Automation type not supported when passing array
            // Convert positional arrays to Enumerable
            var valueList = new List<(string, object)>();
            for (int i = 0; i < attributes.Length; i++)
            {
                valueList.Add((attributes[i], attributeValues[i]));
            }
            Client.WritePropertyMultiple(ids.Select(id => (ObjectId)id), valueList);
        }

        //SendCommand ---------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void SendCommand(string id, string command, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] values = null)
        {
            // Note: MarshalAs decorator is needed when return type is void, otherwise will cause a VBA error on Automation type not supported when passing array
            Guid guid = new Guid(id);
            List<Object> objValues = null;
            if (values != null)
            {
                objValues = new List<object>();
                foreach (string s in values)
                {
                    if (double.TryParse(s, out double numericValue))
                    {
                        objValues.Add(numericValue);
                    }
                    else
                    {
                        objValues.Add(s);
                    }
                }
            }
            Client.SendCommand(guid, command, objValues);
        }
        #endregion


        #region "Spaces" // =========================================================================================================
        //GetSpaces -----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetSpaces(string type = null, int page = 1, int pageSize = 100, string sort = null)
        {
            SpaceTypeEnum? spaceType = null;
            if (type != null)
            {
                // Parse type string to SpaceTypeEnum
                spaceType = (SpaceTypeEnum)Enum.Parse(typeof(SpaceTypeEnum), type);
            }
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.Spaces.Get(spaceType, page, pageSize, sort);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetSpaceChildren ----------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetSpaceChildren(string id)
        {
            Guid guid = new Guid(id);
            var res = Client.Spaces.GetChildren(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetSpaceTypes -------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetSpaceTypes()
        {
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.Spaces.GetTypes();
            return Mapper.Map<IComMetasysObjectType[]>(res);
        }

        //GetSingleSpace ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetSingleSpace(string spaceId)
        {
            Guid guid = Guid.Parse(spaceId);
            var res = Client.Spaces.FindById(guid);
            return Mapper.Map<IComMetasysObject>(res);
        }

        //GetSpacesServedByEquipment ------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetSpacesServedByEquipment(string equipmentId)
        {
            Guid guid = new Guid(equipmentId);
            var res = Client.Spaces.GetServedByEquipment(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }

        //GetSpacesServedByNetworkDevice --------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetSpacesServedByNetworkDevice(string networkDeviceId)
        {
            Guid guid = new Guid(networkDeviceId);
            // Note: need a generic object as return type in order to map correctly to VBA type array
            var res = Client.Spaces.GetServedByNetworkDevice(guid);
            return Mapper.Map<IComMetasysObject[]>(res);
        }
        #endregion


        #region "Trends" // =========================================================================================================
        //GetTrendedAttributes ------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetTrendedAttributes(string id)
        {
            var res = Client.Trends.GetTrendedAttributes(new Guid(id));
            return Mapper.Map<IComMetasysAttribute[]>(res);
        }

        //GetSamples ----------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetSamples(string objectId, int attributeId, IComTimeFilter filter)
        {
            var mapTrendedAttributes = Mapper.Map<TimeFilter>(filter);
            PagedResult<Sample> samples = Client.Trends.GetSamples(new Guid(objectId), attributeId, mapTrendedAttributes);
            return Mapper.Map<IComPagedResult>(samples);
        }

        //GetSamples (2) ------------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetSamples(string objectId, String attributeName, IComTimeFilter filter)
        {
            var mapTrendedAttributes = Mapper.Map<TimeFilter>(filter);
            AttributeEnumSet attribute = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), attributeName);
            PagedResult<Sample> samples = Client.Trends.GetSamples(new Guid(objectId), attribute, mapTrendedAttributes);
            return Mapper.Map<IComPagedResult>(samples);
        }

        //GetNetDevTrendedAttributes ------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public object GetNetDevTrendedAttributes(string id)
        {
            var res = Client.Trends.GetNetDevTrendedAttributes(new Guid(id));
            return Mapper.Map<IComMetasysAttribute[]>(res);
        }

        //GetNetDevSamples ----------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetNetDevSamples(string networkDeviceId, int attributeId, IComTimeFilter filter)
        {
            var mapTrendedAttributes = Mapper.Map<TimeFilter>(filter);
            PagedResult<Sample> samples = Client.Trends.GetNetDevSamples(new Guid(networkDeviceId), attributeId, mapTrendedAttributes);
            return Mapper.Map<IComPagedResult>(samples);
        }

        //GetNetDevSamples (2) ------------------------------------------------------------------------------------------------------
        /// <inheritdoc />
        public IComPagedResult GetNetDevSamples(string networkDeviceId, String attributeName, IComTimeFilter filter)
        {
            var mapTrendedAttributes = Mapper.Map<TimeFilter>(filter);
            AttributeEnumSet attribute = (AttributeEnumSet)Enum.Parse(typeof(AttributeEnumSet), attributeName);
            PagedResult<Sample> samples = Client.Trends.GetNetDevSamples(new Guid(networkDeviceId), attribute, mapTrendedAttributes);
            return Mapper.Map<IComPagedResult>(samples);
        }
        #endregion


        #region "Streams" // ========================================================================================================
        //StartReadingStreamCOVValue ------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StartReadingStreamCOV(string id)
        {
            Guid guid = new Guid(id);
            Client.Streams.StartReadingCOVAsync(guid);
        }

        //StartReadingStreamCOVValues -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StartReadingStreamCOVs([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] ids)
        {
            var guids = new List<Guid>();
            foreach (var id in ids)
            {
                guids.Add(new Guid(id));
            }
            Client.Streams.StartReadingCOVAsync(guids);
        }

        //StopReadingStreamCOVValues ------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StopReadingStreamCOV(string requestId)
        {
            Guid guid = new Guid(requestId);
            Client.Streams.StopReadingCOV(guid);
        }

        //GetCOVStreamValues --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetStreamCOVList()
        {
            var res = Client.Streams.GetCOVList();
            return Mapper.Map<IComStreamMessage[]>(res);
        }

        //GetCOVStreamValue ---------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public IComStreamMessage GetStreamCOV()
        {
            var res = Client.Streams.GetCOV();
            return Mapper.Map<IComStreamMessage>(res);
        }

        /// <inheritdoc/>
        public string[] GetStreamRequestIds()
        {
            var response = Client.Streams.GetRequestIds();
            string[] result = new string[response.Count()];
            int i = 0;
            foreach (var res in response)
            {
                result[i] = res.ToString();
                i++;
            }
            return result;
        }

        /// <inheritdoc />
        public event EventHandler<StreamEventArgs> StreamCOVValueChanged;
        /// <inheritdoc />
        private void OnStreamCOVValueChanged(object sender, StreamEventArgs e)
        {
            StreamCOVValueChanged?.Invoke(this, e);
        }


        //StartCollectingStreamAlarms -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StartCollectingStreamAlarms()
        {
            Client.Streams.StartCollectingAlarmsAsync();
        }

        //StopCollectingStreamAlarms -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StopCollectingStreamAlarms(string requestId)
        {
            Guid guid = new Guid(requestId);
            Client.Streams.StopCollectingAlarms(guid);
        }

        //GetAlarmStreamEvents --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetAlarmStreamEvents()
        {
            var res = Client.Streams.GetAlarmEvents();
            return Mapper.Map<IComStreamMessage[]>(res);
        }

        /// <inheritdoc />
        public event EventHandler<StreamEventArgs> StreamAlarmOccurred;

        /// <inheritdoc />
        void OnStreamAlarmOccurred(object sender, StreamEventArgs e)
        {
            StreamAlarmOccurred?.Invoke(this, e);
        }


        //StartCollectingStreamAuditc -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StartCollectingStreamAudits()
        {
            Client.Streams.StartCollectingAuditsAsync();
        }

        //StopCollectingStreamAudits -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void StopCollectingStreamAudits(string requestId)
        {
            Guid guid = new Guid(requestId);
            Client.Streams.StopCollectingAudits(guid);
        }

        /// <inheritdoc />
        public event EventHandler<StreamEventArgs> StreamAuditOccurred;

        /// <inheritdoc />
        void OnStreamAuditOccurred(object sender, StreamEventArgs e)
        {
            StreamAuditOccurred?.Invoke(this, e);
        }



        //GetAuditStreamEvents --------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public object GetAuditStreamEvents()
        {
            var res = Client.Streams.GetAuditEvents();
            return Mapper.Map<IComStreamMessage[]>(res);
        }

        //KeepStreamAlive -----------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public void KeepStreamAlive()
        {
            Client.Streams.KeepAlive(null);
        }

        #endregion

    }
}
