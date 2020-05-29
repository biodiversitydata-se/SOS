using System;

namespace SOS.Observations.Services.Interfaces
{
    /// <summary>
    ///     Interface for blob storage service
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        ///     Get file download link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetDOIDownloadUrl(Guid id);
    }
}