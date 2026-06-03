using Hangfire;
using SOS.Lib.Dtos.Filter;

namespace SOS.Lib.Jobs.Export;

/// <summary>
///     Interface for DOI creation
/// </summary>
public interface ICreateDoiJob
{
    /// <summary>
    /// Creates a Darwin Core Archive (DwC-A) file based on the specified filter criteria and assigns a Digital Object
    /// Identifier (DOI) to the generated file.
    /// </summary>
    /// <remarks>The method sends notifications regarding the export status to the specified email address.
    /// The operation can be cancelled at any time using the provided cancellation token.</remarks>
    /// <param name="userId">The optional identifier of the user initiating the export operation. If null, the export proceeds without
    /// associating the request with a specific user.</param>
    /// <param name="exportFilter">The filter criteria that determine which data are included in the DwC-A file. This parameter must be configured
    /// to accurately reflect the desired export scope.</param>
    /// <param name="emailAddress">The email address to which notifications about the export process will be sent. This must be a valid email
    /// address.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the export operation. This enables cooperative cancellation if the operation
    /// needs to be terminated before completion.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the DwC-A file was created and the
    /// DOI assigned successfully; otherwise, false.</returns>
    [JobDisplayName("Create a DwC-A file using passed filter and give it a DOI")]
    [Queue("medium")]
    Task<bool> RunAsync(int? userId, ExportFilterDto exportFilter, string emailAddress, IJobCancellationToken cancellationToken);
}