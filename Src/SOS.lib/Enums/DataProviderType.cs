namespace SOS.Lib.Enums
{
    // todo - extract Areas, Vocabularies and Taxa to a new enum ResourceDataType?
    /// <summary>
    ///     Data provider types
    /// </summary>
    public enum DataProviderType
    {
        Areas = 1,
        Vocabularies,
        Taxa,
        ArtportalenObservations,
        ClamPortalObservations,
        KULObservations,
        DwcA,
        NorsObservations,
        SersObservations,
        MvmObservations,
        SharkObservations,
        VirtualHerbariumObservations,
        FishDataObservations,
        ObservationDatabase
    }
}