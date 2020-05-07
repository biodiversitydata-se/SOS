namespace SOS.Lib.Enums
{
    /// <summary>
    /// Id of observation providers, must be binary unique
    /// </summary>
    public enum ObservationProvider
    {
        Artportalen = 1,
        ClamPortal = 2,
        KUL = 4,
        NORS = 8,
        SERS = 16,
        SHARK = 32,
        Dwca = 64,
        VirtualHerbarium = 128,
        MVM = 256
    }
}
