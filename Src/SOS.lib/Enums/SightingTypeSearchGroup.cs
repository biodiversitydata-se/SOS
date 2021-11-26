using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Enums
{
    /// <summary>
    /// All possible values in table SightingTypeSearchGroup
    /// </summary>
    public enum SightingTypeSearchGroup
    {
        /// <summary>
        /// Unknown sighting type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type ordinary.
        /// </summary>
        Ordinary = 1,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type assessment parent.
        /// Artportalen: (Underliggande fynd -> Bedömning av följande fynd...).
        /// </summary>
        Assessment = 2,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type aggregated parent.
        /// Artportalen: (Underliggande fynd -> Ihopslagning av följande fynd...)
        /// </summary>
        Aggregated = 4,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type aggregated child.
        /// Artportalen: (Ingår i fynd -> Fyndet ingår i följande fynd...)
        /// </summary>
        AggregatedChild = 8,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type assessment child.
        /// Artportalen: (Ingår i fynd -> Fyndet ingår i följande fynd...)
        /// </summary>
        AssessmentChild = 16,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type replacement parent.
        /// Artportalen: (Underliggande fynd -> Ersättning av följande fynd...)
        /// </summary>
        Replacement = 32,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type replacement child.
        /// Artportalen: (Ingår i fynd -> Fyndet ingår i följande fynd...)
        /// </summary>
        ReplacementChild = 64,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type own assessment. (Underliggande fynd...)
        /// </summary>
        OwnBreedingAssessment = 128,

        /// <summary>
        /// Gets sighting type search group id for a sighting of type own assessment child. 
        /// Artportalen: (Ingår i fynd -> Fyndet ingår i följande fynd...)
        /// </summary>
        OwnBreedingAssessmentChild = 256
    }
}
