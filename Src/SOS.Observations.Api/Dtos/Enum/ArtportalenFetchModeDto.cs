namespace SOS.Observations.Api.Dtos.Enum
{
    /// <summary>
    /// Artportalen fetch mode.
    /// </summary>
    public enum ArtportalenFetchModeDto
    {
        /// <summary>
        /// Always get the result from SOS database.
        /// </summary>
        Sos = 0,

        /// <summary>
        /// Always get the result from Artportalen API
        /// </summary>
        Artportalen = 1

        ///// <summary>
        ///// Compare modified date first between SOS and Artportalen Db.
        ///// If the Artportalen date is newer than the SOS date then fetch from Artportalen,
        ///// otherwise fetch from SOS.
        ///// </summary>
        //ArtportalenIfNewerThanSos = 2
    }
}