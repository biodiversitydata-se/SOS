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
            OnGoingJobIds = new List<string>();
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
        /// On going export jobs
        /// </summary>
        public ICollection<string> OnGoingJobIds { get; set; }
    }
}
