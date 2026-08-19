namespace SOS.Api.Contracts.Observations.Responses;
/// <summary>
/// Geological information, such as stratigraphy, that qualifies a region or place.
/// </summary>
public class GeologicalContext
{

    /// <summary>
    /// The full name of the lithostratigraphic bed from which
    /// <br/>the cataloged item was collected.
    /// </summary>
    public string? Bed { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>age or lowest chronostratigraphic stage attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? EarliestAgeOrLowestStage { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic eon
    /// <br/>or lowest chrono-stratigraphic eonothem or the informal
    /// <br/>name ("Precambrian") attributable to the stratigraphic
    /// <br/>horizon from which the cataloged item was collected.
    /// </summary>
    public string? EarliestEonOrLowestEonothem { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>epoch or lowest chronostratigraphic series attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? EarliestEpochOrLowestSeries { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>era or lowest chronostratigraphic erathem attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? EarliestEraOrLowestErathem { get; set; }

    /// <summary>
    /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time
    /// <br/>periods at the lowest possible level in a standardized hierarchy. Use this
    /// <br/>property to point to the earliest possible geological time period from which
    /// <br/>the cataloged item was collected.
    /// </summary>
    public string? EarliestGeochronologicalEra { get; set; }

    /// <summary>
    /// The full name of the earliest possible geochronologic
    /// <br/>period or lowest chronostratigraphic system attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? EarliestPeriodOrLowestSystem { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic formation from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string? Formation { get; set; }

    /// <summary>
    /// An identifier for the set of information associated
    /// <br/>with a GeologicalContext (the location within a geological
    /// <br/>context, such as stratigraphy). May be a global unique
    /// <br/>identifier or an identifier specific to the data set.
    /// </summary>
    public string? GeologicalContextId { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic group from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// The full name of the highest possible geological
    /// <br/>biostratigraphic zone of the stratigraphic horizon
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string? HighestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>age or highest chronostratigraphic stage attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? LatestAgeOrHighestStage { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic eon
    /// <br/>or highest chrono-stratigraphic eonothem or the informal
    /// <br/>name ("Precambrian") attributable to the stratigraphic
    /// <br/>horizon from which the cataloged item was collected.
    /// </summary>
    public string? LatestEonOrHighestEonothem { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>epoch or highest chronostratigraphic series attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? LatestEpochOrHighestSeries { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>era or highest chronostratigraphic erathem attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? LatestEraOrHighestErathem { get; set; }

    /// <summary>
    /// Use to link a dwc:GeologicalContext instance to chronostratigraphic time periods at the lowest possible
    /// <br/>level in a standardized hierarchy. Use this property to point to the latest possible geological time period
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string? LatestGeochronologicalEra { get; set; }

    /// <summary>
    /// The full name of the latest possible geochronologic
    /// <br/>period or highest chronostratigraphic system attributable
    /// <br/>to the stratigraphic horizon from which the cataloged
    /// <br/>item was collected.
    /// </summary>
    public string? LatestPeriodOrHighestSystem { get; set; }

    /// <summary>
    /// The combination of all litho-stratigraphic names for
    /// <br/>the rock from which the cataloged item was collected.
    /// </summary>
    public string? LithostratigraphicTerms { get; set; }

    /// <summary>
    /// The full name of the lowest possible geological
    /// <br/>biostratigraphic zone of the stratigraphic horizon
    /// <br/>from which the cataloged item was collected.
    /// </summary>
    public string? LowestBiostratigraphicZone { get; set; }

    /// <summary>
    /// The full name of the lithostratigraphic member from
    /// <br/>which the cataloged item was collected.
    /// </summary>
    public string? Member { get; set; }

}


