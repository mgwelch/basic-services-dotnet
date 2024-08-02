using JohnsonControls.Metasys.BasicServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Defines method to provide equipment infos for endpoints of the Metasys Equipments API.
    /// </summary>

    public interface IEquipmentService : IBasicService
    {
        // FindById ----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the specified equipment instance.
        /// </summary>
        MetasysObject FindById(ObjectId equipmentId);
        /// <inheritdoc cref="IEquipmentService.FindById(ObjectId)"/>
        Task<MetasysObject> FindByIdAsync(ObjectId equipmentId);

        // Get ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves a collection of equipment instances.
        /// </summary>
        /// <param name="page">Specifies the number of the page to be retrieved (optional).</param>
        /// <param name="pageSize">Specifies the number of items per page. Default is 1000. (optional).</param>
        /// <remarks> If the param 'page' is specified then it will return only the items associated to the specified page number. Otherwise all the items will be returned. </remarks>
        IEnumerable<MetasysObject> Get(int? page = null, int? pageSize = null);
        /// <inheritdoc cref="IEquipmentService.Get(int?, int?)"/>
        Task<IEnumerable<MetasysObject>> GetAsync(int? page = null, int? pageSize = null);

        // GetPoints -----------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of points that are defined by the specified equipment instance.
        /// </summary>
        /// <param name="equipmentId">The id of the equipment.</param>
        /// <param name="readAttributeValue">Set to false if you would not read Points Attribute Value.</param>
        /// <remarks> Reading the Attribute Value attribute could take time depending on the number of points. </remarks>
        /// <returns></returns>
        IEnumerable<MetasysPoint> GetPoints(ObjectId equipmentId, bool readAttributeValue = true);
        /// <inheritdoc cref="IEquipmentService.GetPoints(ObjectId, bool)"/>
        Task<IEnumerable<MetasysPoint>> GetPointsAsync(ObjectId equipmentId, bool readAttributeValue = true);



        // GetServingASpace ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of equipment that serve the specified space.
        /// </summary>
        /// <param name="spaceId"></param>
        IEnumerable<MetasysObject> GetServingASpace(ObjectId spaceId);
        /// <inheritdoc cref="IEquipmentService.GetServingASpace(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetServingASpaceAsync(ObjectId spaceId);

        // GetHostedByNetworkDevice ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of equipment instances that are hosted by the specified network device or its children.
        /// </summary>
        /// <param name="networkDeviceId"></param>
        IEnumerable<MetasysObject> GetHostedByNetworkDevice(ObjectId networkDeviceId);
        /// <inheritdoc cref="IEquipmentService.GetServingASpace(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetHostedByNetworkDeviceAsync(ObjectId networkDeviceId);


        // GetServedByEquipment ----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the equipment served by the specified equipment instance.
        /// </summary>
        IEnumerable<MetasysObject> GetServedByEquipment(ObjectId equipmentId);
        /// <inheritdoc cref="IEquipmentService.GetServedByEquipment(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetServedByEquipmentAsync(ObjectId equipmentId);

        // GetServingAnEquipment ------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of equipment that serve the specified equipment instance.
        /// </summary>
        /// <param name="equipmentId"></param>
        IEnumerable<MetasysObject> GetServingAnEquipment(ObjectId equipmentId);
        /// <inheritdoc cref="IEquipmentService.GetServingAnEquipment(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetServingAnEquipmentAsync(ObjectId equipmentId);

    }
}
