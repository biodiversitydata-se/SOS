using SOS.Lib.Models.Processed.DataStewardship.Enums;

namespace SOS.Lib.Models.Processed.DataStewardship.Common
{
    public class Project
    {
        /// <summary>
        /// Unique id for the project within which the dataset was collected.
        /// </summary>                    
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of the project within which the dataset was collected. Can sometimes be the same as the name of the dataset.
        /// </summary>                    
        public string ProjectCode { get; set; }

        /// <summary>
        /// Type of project that the dataset was collected within, e.g. delprogram, gemensamt delprogram.
        /// </summary>
        public ProjectType? ProjectType { get; set; }
    }
}
