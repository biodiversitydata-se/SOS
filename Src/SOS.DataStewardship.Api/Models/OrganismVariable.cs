using SOS.DataStewardship.Api.Models.Enums;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Observed attributes and other variables linked to a certain individual or uniform group of individuals, e.g. sex, age, behavior.
    /// </summary>
    [DataContract]
    public class OrganismVariable
    { 
        /// <summary>
        /// Sex of the observed organism.
        /// </summary>
        [DataMember(Name="sex")]
        public Sex? Sex { get; set; }

        /// <summary>
        /// Age category or development stage of the observed organism.
        /// </summary>
        [DataMember(Name="lifeStage")]
        public LifeStage? LifeStage { get; set; }

        /// <summary>
        /// Activity or behavior of the observed organism.
        /// </summary>
        [DataMember(Name="activity")]
        public Activity? Activity { get; set; }
    }
}
