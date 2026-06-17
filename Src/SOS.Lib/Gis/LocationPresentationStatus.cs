namespace SOS.Lib.Gis;

public enum LocationPresentationStatus
{
    /// <summary>
    /// At least one observation is present (obs.occurrence.isPositiveObservation==true).
    /// </summary>
    Present = 0,

    /// <summary>
    /// All observations are present (obs.occurrence.isPositiveObservation==true) 
    /// and all are sensitive (obs.sensitive==true).
    /// </summary>
    PresentSensitive = 1,

    /// <summary>
    /// No observations are present (obs.occurrence.isPositiveObservation!=true) but at least one observation is treated
    /// (obs.artportalenInternal.invasiveSpeciesTreatment.id is not null and >= 0).
    /// </summary>
    Treated = 2,

    /// <summary>
    /// No observations are present (obs.occurrence.isPositiveObservation!=true) but at least one observation is not
    /// rediscovered (obs.occurrence.isNotRediscoveredObservation==true).
    /// </summary>
    NotRecovered = 3,

    /// <summary>
    /// No observations are present (obs.occurrence.isPositiveObservation!=true) and no observation is not rediscovered
    /// (obs.occurrence.isNotRediscoveredObservation!=true) but at least one observation is not present
    /// (obs.occurrence.isNeverFoundObservation==true).
    /// This is also used when cutoff year is used and all observations is not present (obs.occurrence.isPositiveObservation!=true).
    /// </summary>
    NotPresent = 4,

    /// <summary>
    /// When cutoff year is used and there are no present observations (obs.occurrence.isPositiveObservation!=true) with
    /// end year (obs.event.endYear) equal to or larger than cutoff year, but there is at least one present
    /// observation (obs.occurrence.isPositiveObservation==true) with end year (obs.event.endYear) before cutoff
    /// year.
    /// </summary>
    DividedByYearUntil = 5,

    /// <summary>
    /// Multiple locations are clustered together because they are close to each other. 
    /// </summary>
    Cluster = 6
}