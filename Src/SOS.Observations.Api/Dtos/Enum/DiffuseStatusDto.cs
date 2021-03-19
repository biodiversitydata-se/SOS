namespace SOS.Observations.Api.Dtos.Enum
{

    /// <summary>
    /// Diffuse status dto
    /// </summary>
    public enum DiffusionStatusDto
    {
        /// <summary>
        /// Observation is not diffused
        /// </summary>
        NotDiffused = 0,
        /// <summary>
        /// Observation is diffused by the system and the non diffused original exists in the protected index
        /// </summary>
        DiffusedBySystem,
        /// <summary>
        /// Observation is diffused by provider. No original data exists in the system
        /// </summary>
        DiffusedByProvider
    }
}
