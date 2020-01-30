using System;
using System.Collections;
using System.Linq;

namespace SOS.Lib.Models.Shared
{
    public class InputGeometry
    {
        /// <summary>
        /// Geometry coordinates
        /// </summary>
        public ArrayList Coordinates { get; set; }

        /// <summary>
        /// Simple check to check if geometry looks ok
        /// </summary>
        public bool IsValid => new []{ "point", "polygon", "multipolygon"}.Contains(Type?.ToLower()) ;

        /// <summary>
        /// Type of geometry (point or polygon)
        /// </summary>
        public string Type { get; set; }
    }
}
