using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Observed attributes and other variables linked to a certain individual or uniform group of individuals, e.g. sex, age, behavior.
    /// </summary>
    public class DsOrganismVariableDto
    {
        /// <summary>
        /// Sex of the observed organism.
        /// </summary>
        public DsSex? Sex { get; set; }

        /// <summary>
        /// Age category or development stage of the observed organism.
        /// </summary>
        public DsLifeStage? LifeStage { get; set; }

        /// <summary>
        /// Activity or behavior of the observed organism.
        /// </summary>
        public DsActivity? Activity { get; set; }
    }
}
