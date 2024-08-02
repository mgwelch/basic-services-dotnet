using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JohnsonControls.Metasys.BasicServices.Utils;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    ///
    /// </summary>
    public interface ITrendService : IBasicService
    {
        /// <summary>
        /// Get the list of trended attributes for the given object.
        /// </summary>
        /// <param name="id">The identifier of the object.</param>
        /// <returns>The list of trended attributes.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        List<MetasysAttribute> GetTrendedAttributes(ObjectId id);
        /// <inheritdoc cref="ITrendService.GetTrendedAttributes(ObjectId)"/>
        Task<List<MetasysAttribute>> GetTrendedAttributesAsync(ObjectId id);

        /// <summary>
        /// Retrieves available samples for the given object attribute, filtered by startTime and endTime.
        /// </summary>
        /// <remarks>StartTime and EndTime are mandatory parameters.</remarks>
        /// <param name="objectId">The identifier of the object.</param>
        /// <param name="attributeId">The numeric identifier of the attribute.</param>
        /// <param name="filter">The filter that specifies: startTime, endTime, page, pageSize, sort.</param>
        /// <returns>The list of samples.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        PagedResult<Sample> GetSamples(ObjectId objectId, int attributeId, TimeFilter filter);
        /// <inheritdoc cref="ITrendService.GetSamples(ObjectId, int, TimeFilter)"/>
        Task<PagedResult<Sample>> GetSamplesAsync(ObjectId objectId, int attributeId, TimeFilter filter);

        /// <summary>
        /// Retrieves available samples for the given object attribute, filtered by startTime and endTime.
        /// </summary>
        /// <remarks>StartTime and EndTime are mandatory parameters.</remarks>
        /// <param name="objectId">The identifier of the object.</param>
        /// <param name="attributeName">The textual identifier of the attribute.</param>
        /// <param name="filter">The filter that specifies: startTime, endTime, page, pageSize, sort.</param>
        /// <returns>The list of samples.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        PagedResult<Sample> GetSamples(ObjectId objectId, AttributeEnumSet attributeName, TimeFilter filter);
        /// <inheritdoc cref="ITrendService.GetSamples(ObjectId, AttributeEnumSet, TimeFilter)"/>
        Task<PagedResult<Sample>> GetSamplesAsync(ObjectId objectId, AttributeEnumSet attributeName, TimeFilter filter);

        /// <summary>
        /// Get the list of trended attributes for the given Network Device.
        /// </summary>
        /// <param name="id">The identifier of the network device.</param>
        /// <returns>The list of trended attributes.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        List<MetasysAttribute> GetNetDevTrendedAttributes(ObjectId id);
        /// <inheritdoc cref="ITrendService.GetNetDevTrendedAttributes(ObjectId)"/>
        Task<List<MetasysAttribute>> GetNetDevTrendedAttributesAsync(ObjectId id);

        /// <summary>
        /// Retrieves available samples for the given network device attribute, filtered by startTime and endTime.
        /// </summary>
        /// <remarks>StartTime and EndTime are mandatory parameters.</remarks>
        /// <param name="networkDeviceId">The identifier of the network device.</param>
        /// <param name="attributeId">The numeric identifier of the attribute.</param>
        /// <param name="filter">The filter that specifies: startTime, endTime, page, pageSize, sort.</param>
        /// <returns>The list of samples.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        PagedResult<Sample> GetNetDevSamples(ObjectId networkDeviceId, int attributeId, TimeFilter filter);
        /// <inheritdoc cref="ITrendService.GetNetDevSamples(ObjectId, int, TimeFilter)"/>
        Task<PagedResult<Sample>> GetNetDevSamplesAsync(ObjectId networkDeviceId, int attributeId, TimeFilter filter);

        /// <summary>
        /// Retrieves available samples for the given network device attribute, filtered by startTime and endTime.
        /// </summary>
        /// <remarks>StartTime and EndTime are mandatory parameters.</remarks>
        /// <param name="networkDeviceId">The identifier of the network device.</param>
        /// <param name="attributeName">The textual identifier of the attribute.</param>
        /// <param name="filter">The filter that specifies: startTime, endTime, page, pageSize, sort.</param>
        /// <returns>The list of samples.</returns>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysHttpException"></exception>
        /// <exception cref="MetasysHttpTimeoutException"></exception>
        /// <exception cref="MetasysHttpParsingException"></exception>
        /// <exception cref="MetasysHttpNotFoundException"></exception>
        /// <exception cref="MetasysObjectException"></exception>
        PagedResult<Sample> GetNetDevSamples(ObjectId networkDeviceId, AttributeEnumSet attributeName, TimeFilter filter);
        /// <inheritdoc cref="ITrendService.GetNetDevSamples(ObjectId, AttributeEnumSet, TimeFilter)"/>
        Task<PagedResult<Sample>> GetNetDevSamplesAsync(ObjectId networkDeviceId, AttributeEnumSet attributeName, TimeFilter filter);

    }
}
