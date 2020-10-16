using Microsoft.AspNetCore.Http;

namespace SOS.Administration.Api.Models
{
    /// <summary>
    ///     DTO for importing EML file.
    /// </summary>
    public class ImportEmlFileDto
    {
        /// <summary>
        ///     Data provider id or identifier.
        /// </summary>
        public string DataProviderIdOrIdentifier { get; set; }

        /// <summary>
        ///     EML or DwC-A file.
        /// </summary>
        public IFormFile File { get; set; }
    }
}