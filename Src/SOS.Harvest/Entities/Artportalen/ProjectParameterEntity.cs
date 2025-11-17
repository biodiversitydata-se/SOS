namespace SOS.Harvest.Entities.Artportalen;

/// <summary>
///     Project parameter entity
/// </summary>
public class ProjectParameterEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public string? DataType { get; set; }
}