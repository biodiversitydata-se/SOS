namespace SOS.Observations.Api.Dtos.Health
{
    public class HealthEntryDto
    {
        /// <summary>
        /// Key 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Health status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
    }
}
