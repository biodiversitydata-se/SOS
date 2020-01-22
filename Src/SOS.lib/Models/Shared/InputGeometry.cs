using System;

namespace SOS.Lib.Models.Shared
{
    public class InputGeometry
    {
        /// <summary>
        /// Geometry coordinates
        /// </summary>
        public double[][] Coordinates { get; set; }

        /// <summary>
        /// Simple check to check if geometry looks ok
        /// </summary>
        public bool IsValid => Type.Equals("Point", StringComparison.CurrentCultureIgnoreCase) && (Coordinates?.Length.Equals(1) ?? false) ||
                               Type.Equals("Polygon", StringComparison.CurrentCultureIgnoreCase) && (Coordinates?.Length ?? 0) > 1;

        /// <summary>
        /// Type of geometry (point or polygon)
        /// </summary>
        public string Type { get; set; }
    }
}
