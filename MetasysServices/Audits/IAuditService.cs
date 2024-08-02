using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Defines method to provide audit infos for endpoints of the Metasys Audit API.
    /// </summary>
    public interface IAuditService : IBasicService
    {
        /// <summary>
        /// Retrieves the specified audit.
        /// </summary>
        /// <param name="auditId">The identifier of the audit.</param>
        /// <returns>The specified audit details.</returns>
        Audit FindById(Guid auditId);

        /// <inheritdoc cref="IAuditService.FindById(Guid)"/>
        Task<Audit> FindByIdAsync(Guid auditId);

        /// <summary>
        /// Retrieves a collection of audits.
        /// </summary>
        /// <param name="auditFilter">The audit model to filter audits.</param>
        /// <returns>The list of audits with details.</returns>
        PagedResult<Audit> Get(AuditFilter auditFilter);

        /// <inheritdoc cref="IAuditService.Get(AuditFilter)"/>
        Task<PagedResult<Audit>> GetAsync(AuditFilter auditFilter);

        /// <summary>
        /// Retrieve a collection of Audit Annotations.
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns></returns>
        IEnumerable<AuditAnnotation> GetAnnotations(Guid auditId);

        /// <inheritdoc cref="IAuditService.GetAnnotations(Guid)"/>
        Task<IEnumerable<AuditAnnotation>> GetAnnotationsAsync(Guid auditId);

        /// <summary>
        /// Retrieves a collection of audits for the specified object.
        /// </summary>
        /// <param name="objectId">The identifier of the object.</param>
        /// <param name="auditFilter">The filter to be applied to audit list.</param>
        /// <returns>The list of audit with details.</returns>
        PagedResult<Audit> GetForObject(ObjectId objectId, AuditFilter auditFilter);

        /// <inheritdoc cref="IAuditService.GetForObject(ObjectId, AuditFilter)"/>
        Task<PagedResult<Audit>> GetForObjectAsync(ObjectId objectId, AuditFilter auditFilter);

        /// <summary>
        /// Discard an Audit.
        /// </summary>
        /// <param name="id">The identifier of the Audit.</param>
        /// <param name="annotationText">Text of the annotation to report the reason of the discard.</param>
        /// <exception cref="MetasysUnsupportedApiVersion"></exception>
        void Discard(Guid id, string annotationText);

        /// <inheritdoc cref="IAuditService.Discard(Guid , string)"/>
        Task DiscardAsync(Guid id, string annotationText);

        /// <summary>
        /// Add an Annotation to the specified Audit.
        /// </summary>
        /// <param name="id">The identifier of the Audit.</param>
        /// <param name="text">The text of the Annotation.</param>
        /// <exception cref="MetasysUnsupportedApiVersion"></exception>
        void AddAnnotation(Guid id, string text);

        /// <inheritdoc cref="IAuditService.AddAnnotation(Guid, string)"/>
        Task AddAnnotationAsync(Guid id, string text);

        /// <summary>
        /// Add many Annotations given a list of requests containing the Id of the Audits and the text of the Annotations.
        /// </summary>
        /// <param name="requests">List of BatchRequestParam to specify the id of the audits and the text of the annotations to add.</param>
        /// <returns>
        /// A list of BatchRequestParam with all the specified attributes.
        /// </returns>
        IEnumerable<Result> AddAnnotationMultiple(IEnumerable<BatchRequestParam> requests);

        /// <summary>
        /// Add many Annotations given a list of requests containing the Id of the Audits and the text of the Annotations.
        /// </summary>
        /// <param name="requests">List of BatchRequestParam to specify the id of the audits and the text of the annotations to add.</param>
        /// <returns>
        /// A list of BatchRequestParam with all the specified attributes.
        /// </returns>
        Task<IEnumerable<Result>> AddAnnotationMultipleAsync(IEnumerable<BatchRequestParam> requests);

        /// <summary>
        /// Discard many Audit given a list of requests containing the Id of the Audits and the text for the Annotations.
        /// </summary>
        /// <param name="requests">List of BatchRequestParam to specify the id of the audits and the text of the annotations to discard.</param>
        /// <returns>
        /// A list of BatchRequestParam with all the specified attributes.
        /// </returns>
        IEnumerable<Result> DiscardMultiple(IEnumerable<BatchRequestParam> requests);

        /// <summary>
        /// Discard many Audit given a list of requests containing the Id of the Audits and the text for the Annotations.
        /// </summary>
        /// <param name="requests">List of BatchRequestParam to specify the id of the audits and the text of the annotations to discard.</param>
        /// <returns>
        /// A list of BatchRequestParam with all the specified attributes.
        /// </returns>
        Task<IEnumerable<Result>> DiscardMultipleAsync(IEnumerable<BatchRequestParam> requests);

    }
}
