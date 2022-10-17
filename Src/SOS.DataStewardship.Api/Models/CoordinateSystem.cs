using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// Coordinate system for the (X, Y) coordinates of the geometry.
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// EPSG:4326
        /// </summary>
        [EnumMember(Value = "EPSG:4326")]
        EPSG4326 = 0,
        /// <summary>
        /// EPSG:3006
        /// </summary>
        [EnumMember(Value = "EPSG:3006")]
        EPSG3006 = 1,
        /// <summary>
        /// EPSG:4619
        /// </summary>
        [EnumMember(Value = "EPSG:4619")]
        EPSG4619 = 2,
        /// <summary>
        /// EPSG:3857
        /// </summary>
        [EnumMember(Value = "EPSG:3857")]
        EPSG3857 = 3,
        /// <summary>
        /// EPSG:4258
        /// </summary>
        [EnumMember(Value = "EPSG:4258")]
        EPSG4258 = 4
    }
}
