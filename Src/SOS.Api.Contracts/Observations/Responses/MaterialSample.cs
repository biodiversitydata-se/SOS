namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// A physical result of a sampling (or subsampling) event. In biological collections, the material sample is typically
/// <br/>collected,
/// <br/>and either preserved or destructively processed.
/// </summary>
public class MaterialSample
{

    /// <summary>
    /// An identifier for the MaterialSample (as opposed to a particular digital record of the material sample).
    /// <br/>In the absence of a persistent global unique identifier, construct one from a combination of identifiers in the
    /// <br/>record
    /// <br/>that will most closely make the materialSampleID globally unique.
    /// </summary>
    public string? MaterialSampleId { get; set; }

}


