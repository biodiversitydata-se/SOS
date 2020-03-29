using Microsoft.AspNetCore.Http;

namespace SOS.Administration.Api.Models
{
    /// <summary>
    /// DTO for handling upload of a DwC-A file.
    /// </summary>
    public class UploadDwcArchiveModelDto
    {
        /// <summary>
        /// DwC-A file.
        /// </summary>
        public IFormFile DwcaFile { get; set; }

        //public int DatasetId { get; set; } // todo - handle this later?
    }
}