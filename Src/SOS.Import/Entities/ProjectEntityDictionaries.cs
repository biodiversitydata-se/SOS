using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Import.Entities
{
    /// <summary>
    /// Project & project parameter dictionaries.
    /// </summary>
    public class ProjectEntityDictionaries
    {
        public IDictionary<int, ProjectEntity> ProjectEntityById { get; set; }
        public IDictionary<int, IEnumerable<ProjectEntity>> ProjectEntitiesBySightingId { get; set; }
        public IDictionary<int, IEnumerable<ProjectParameterEntity>> ProjectParameterEntitiesBySightingId { get; set; }
    }
}
