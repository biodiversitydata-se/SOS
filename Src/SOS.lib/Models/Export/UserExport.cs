using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Export
{
    /// <summary>
    /// Keep control of user exports
    /// </summary>
    public class UserExport : IEntity<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserExport()
        {
            Jobs = new List<ExportJobInfo>();
        }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Exports limit
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Jobs information
        /// </summary>
        public ICollection<ExportJobInfo> Jobs { get; set; }
    }   
}
