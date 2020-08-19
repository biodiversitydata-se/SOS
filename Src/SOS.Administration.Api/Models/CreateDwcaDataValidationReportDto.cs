using Microsoft.AspNetCore.Http;

namespace SOS.Administration.Api.Models
{
    /// <summary>
    ///     DTO for creating a data validation report for a DwC-A file.
    /// </summary>
    public class CreateDwcaDataValidationReportDto
    {
        /// <summary>
        ///     DwC-A file.
        /// </summary>
        public IFormFile DwcaFile { get; set; }
    }
}