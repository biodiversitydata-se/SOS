using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Enums
{
    /// <summary>
    /// Enumeration of Swedish provinces.
    /// </summary>
    public enum ProvinceFeatureId
    {
        /// <summary>
        /// Oland.
        /// </summary>
        Oland = 4,

        /// <summary>
        /// Lule lappmark, province part of Lappland
        /// </summary>
        LuleLappmark = 25,

        /// <summary>
        /// Lycksele lappmark, province part of Lappland
        /// </summary>
        LyckseleLappmark = 26,

        /// <summary>
        /// Pite lappmark, province part of Lappland
        /// </summary>
        PiteLappmark = 27,

        /// <summary>
        /// Torne lappmark, province part of Lappland
        /// </summary>
        TorneLappmark = 28,

        /// <summary>
        /// Åsele lappmark, province part of Lappland
        /// </summary>
        AseleLappmark = 29,

        /// <summary>
        /// Special province Lappland that have province parts.
        /// This province exists in the table Province in database SwedishSpeciesObservationResources
        /// </summary>
        Lappland = 100
    }
}
