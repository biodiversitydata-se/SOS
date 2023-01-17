using System.Collections.Generic;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Artportalen project information.
    /// </summary>
    public class Project : ProjectInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Project()
        {

        }

        /// <summary>
        ///     Project parameters.
        /// </summary>
        public IEnumerable<ProjectParameter> ProjectParameters { get; set; }
        public override string ToString()
        {
            string strProjectParameters = ProjectParameters == null ? null : string.Join(", ", ProjectParameters);
            if (string.IsNullOrEmpty(strProjectParameters))
                return $"{Name} ({Id})";
            else
                return $"{Name} ({Id}) - {strProjectParameters}";
        }
    }
}