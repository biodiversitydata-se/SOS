﻿namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Project & project parameter dictionaries.
    /// </summary>
    public class ProjectEntityDictionaries
    {
        public IDictionary<int, ProjectEntity>? ProjectEntityById { get; set; }
        public IDictionary<int, IEnumerable<ProjectEntity>>? ProjectEntitiesBySightingId { get; set; }
        public IDictionary<int, IEnumerable<ProjectParameterSightingEntity>>? ProjectParameterEntitiesBySightingId { get; set; }
    }
}