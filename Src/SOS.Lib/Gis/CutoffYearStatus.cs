namespace SOS.Lib.Gis;

public enum CutoffYearStatus
{
    /// <summary>
    /// At least one observation is present (obs.occurrence.isPositiveObservation==true)
    /// with end year (obs.event.endYear) equal to or larger than cutoff year.
    /// </summary>
    After = 0,

    /// <summary>
    /// When its not After, but there is at least one not rediscovered
    /// (obs.occurrence.isNotRediscoveredObservation==true) with end year (obs.event.endYear) 
    /// equal to or larger than cutoff year.
    /// </summary>
    AfterNotRecovered = 2,

    /// <summary>
    /// When its not After or AfterNotRecovered, but there is at least
    /// one present observation (obs.occurrence.isPositiveObservation==true) 
    /// with end year (obs.event.endYear) smaller than cutoff year.
    /// </summary>
    Until = 3,

    /// <summary>
    /// When its not After or AfterNotRecovered or Until, but there is at least one not rediscovered 
    /// (obs.occurrence.isNotRediscoveredObservation==true) with end year (obs.event.endYear) smaller than cutoff year.
    /// </summary>
    UntilNotRecovered = 4,

    /// <summary>    
    /// All observations is not present (obs.occurrence.isPositiveObservation!=true).
    /// </summary>
    NotPresent = 5,
}


