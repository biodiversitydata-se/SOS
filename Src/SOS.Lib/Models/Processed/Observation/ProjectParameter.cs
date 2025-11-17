
namespace SOS.Lib.Models.Processed.Observation;

/// <summary>
/// Artportalen project parameter.
/// </summary>
public class ProjectParameter : ProjectParameterBase
{
    /// <summary>
    /// Value of the data in string format.
    /// </summary>
    public string Value { get; set; }

    public override string ToString()
    {
        return $"[{Name}={Value}]";
    }
}