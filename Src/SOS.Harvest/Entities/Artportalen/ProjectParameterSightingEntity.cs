namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Project parameter entity
    /// </summary>
    public class ProjectParameterSightingEntity : ProjectParameterProjectEntity
    {
        public int SightingId { get; set; }
        public string? Value { get; set; }
    }
}