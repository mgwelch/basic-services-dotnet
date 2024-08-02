using JohnsonControls.Metasys.BasicServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Defines method to provide space infos for endpoints of the Metasys Spaces API.
    /// </summary>
    public interface ISpaceService : IBasicService
    {
        // FindById ----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the specified space.
        /// </summary>
        /// <param name="spaceId">The identifier of the space.</param>
        /// <returns>The specified alarm details.</returns>
        MetasysObject FindById(ObjectId spaceId);
        /// <inheritdoc cref="ISpaceService.FindById(ObjectId)"/>
        Task<MetasysObject> FindByIdAsync(ObjectId spaceId);


        // Get ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves a collection of spaces.
        /// </summary>
        /// <param name="type">Optional, type ID (numeric for v2, v3 - enumset key string for v4).</param>
        /// <param name="page">Optional, the page numer of items to return (default = 1).</param>
        /// <param name="pageSize">Optional, the maximum number of items to return in the response (default = 100, max = 1000).</param>
        /// <param name="sort">Optional, the criteria to use when sorting results (default = name).</param>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        IEnumerable<MetasysObject> Get(SpaceTypeEnum? type = null, int? page = null, int? pageSize = null, string sort = null);
        /// <inheritdoc cref="ISpaceService.Get(SpaceTypeEnum?, int?, int?, string)"/>
        Task<IEnumerable<MetasysObject>> GetAsync(SpaceTypeEnum? type = null, int? page = null, int? pageSize = null, string sort = null);


        // Get ---------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves a collection of spaces.
        /// </summary>
        /// <param name="type">Optional, type ID (numeric for v2, v3 - enumset key string for v4).</param>
        /// <param name="page">Optional, the page numer of items to return (default = 1).</param>
        /// <param name="pageSize">Optional, the maximum number of items to return in the response (default = 100, max = 1000).</param>
        /// <param name="sort">Optional, the criteria to use when sorting results (default = name).</param>
        IEnumerable<MetasysObject> Get(string type, int? page = null, int? pageSize = null, string sort = null);
        /// <inheritdoc cref="ISpaceService.Get(string, int?, int?, string)"/>
        Task<IEnumerable<MetasysObject>> GetAsync(string type, int? page = null, int? pageSize = null, string sort = null);

        // GetSpaceChildren ---------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of spaces that are located within the specified space.
        /// </summary>
        /// <param name="spaceId">The id of the parent space.</param>
        IEnumerable<MetasysObject> GetChildren(ObjectId spaceId);
        /// <inheritdoc cref="ISpaceService.GetChildren(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetChildrenAsync(ObjectId spaceId);

        // GetSpaceTypes -----------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of all spaces types.
        /// </summary>
        IEnumerable<MetasysObjectType> GetTypes();
        /// <inheritdoc cref="ISpaceService.GetTypes()"/>
        Task<IEnumerable<MetasysObjectType>> GetTypesAsync();

        // GetServedByNetworkDevice -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of spaces served by the specified network device.
        /// </summary>
        IEnumerable<MetasysObject> GetServedByNetworkDevice(ObjectId networkDeviceId);
        /// <inheritdoc cref="ISpaceService.GetServedByNetworkDevice(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetServedByNetworkDeviceAsync(ObjectId networkDeviceId);

        // GetServedByEquipment ----------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieves the collection of spaces served by the specified network device.
        /// </summary>
        IEnumerable<MetasysObject> GetServedByEquipment(ObjectId equipmentId);
        /// <inheritdoc cref="ISpaceService.GetServedByEquipment(ObjectId)"/>
        Task<IEnumerable<MetasysObject>> GetServedByEquipmentAsync(ObjectId equipmentId);

    }
}
