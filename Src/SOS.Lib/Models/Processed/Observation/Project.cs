namespace SOS.Lib.Models.Processed.Observation;

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

    
    public override string ToString()
    {
        string strProjectParameters = ProjectParameters == null ? null : string.Join(", ", ProjectParameters);
        if (string.IsNullOrEmpty(strProjectParameters))
            return $"{Name} ({Id})";
        else
            return $"{Name} ({Id}) - {strProjectParameters}";
    }
}