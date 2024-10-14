namespace SOS.Lib.Enums.Artportalen
{
    /// <summary>
    /// Sighting state type.
    /// </summary>
    public enum SightingStateType
    {
        /// <summary>
        /// A sighting that is not yet reported.
        /// </summary>
        Temporary = 10,

        /// <summary>
        /// A reported, but not reviewed, sighting.
        /// </summary>
        Submitted = 20,

        /// <summary>
        /// A sighting that has been made public.
        /// </summary>
        Published = 30,

        /// <summary>
        /// A sighting that is suppressed by another sighting.
        /// </summary>
        Suppressed = 40,

        /// <summary>
        /// A sighting that has been set as deleted.
        /// </summary>
        Deleted = 50
    }
}