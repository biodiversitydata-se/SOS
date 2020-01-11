using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Enums
{
    /// <summary>
    /// County Feature Id's
    /// </summary>
    public enum CountyFeatureId
    {
        /// <summary>
        /// Kalmar county.
        /// </summary>
        Kalmar = 8,

        /// <summary>
        /// Halland county.
        /// </summary>
        Halland = 13,

        /// <summary>
        /// Kalmar fastland
        /// Special county that represents a part of the County Kalmar
        /// </summary>
        KalmarFastland = 100,

        /// <summary>
        /// Oland
        /// Special county that represents a part of the county Kalmar (Öland)
        /// </summary>
        Oland = 101
    }
}