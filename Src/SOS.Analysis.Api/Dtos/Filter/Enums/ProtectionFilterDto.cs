namespace SOS.Analysis.Api.Dtos.Filter.Enums
{
    public enum ProtectionFilterDto
    {
        /// <summary>
        /// Public observations
        /// </summary>
        Public = 0,

        /// <summary>
        /// Sensitive observations
        /// </summary>
        Sensitive,

        /// <summary>
        /// Both public and sensitive observations
        /// </summary>
        BothPublicAndSensitive
    }
}
