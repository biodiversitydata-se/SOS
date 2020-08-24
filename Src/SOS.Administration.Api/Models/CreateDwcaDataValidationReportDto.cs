using System.ComponentModel;
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

        /// <summary>
        /// Max number of observations to read and process.
        /// </summary>
        [DefaultValue(100000)]
        public int MaxNrObservationsToRead { get; set; } = 100000;

        /// <summary>
        /// Max number of valid observations to include in report.
        /// </summary>
        [DefaultValue(100)]
        public int NrValidObservationsInReport { get; set; } = 100;

        /// <summary>
        /// Max number of invalid observations to include in report.
        /// </summary>
        [DefaultValue(100)]
        public int NrInvalidObservationsInReport { get; set; } = 100;
    }
}