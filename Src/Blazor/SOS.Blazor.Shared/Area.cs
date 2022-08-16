using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Blazor.Shared
{
    public class Area
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType AreaType { get; set; }

        public LatLonBoundingBox BoundingBox { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Area Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }

    public class LatLonBoundingBox
    {
        public LatLonCoordinate BottomRight { get; set; }
        public LatLonCoordinate TopLeft { get; set; }
    }

    public class LatLonCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
