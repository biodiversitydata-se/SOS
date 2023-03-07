using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Observed attributes and other variables linked to a certain individual or uniform group of individuals, e.g. sex, age, behavior.
    /// </summary>
    public class OrganismVariable
    {
        /// <summary>
        /// Sex of the observed organism.
        /// </summary>
        public Sex? Sex { get; set; }

        /// <summary>
        /// Age category or development stage of the observed organism.
        /// </summary>
        public LifeStage? LifeStage { get; set; }

        /// <summary>
        /// Activity or behavior of the observed organism.
        /// </summary>
        public Activity? Activity { get; set; }
    }
}