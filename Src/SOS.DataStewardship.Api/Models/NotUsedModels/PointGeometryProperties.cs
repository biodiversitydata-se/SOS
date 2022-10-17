using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models.NotUsedModels
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PointGeometryProperties
    {
        /// <summary>
        /// The estimated absolute positional accuracy of the (X,Y) coordinates of the building geometry, in the INSPIRE official Coordinate Reference System. Absolute positional accuracy is defined as the mean value of the positional uncertainties for a set of positions where the positional uncertainties are defined as the distance between a measured position and what is considered as the corresponding true position. (Source: INSPIRE). The positional accuracy is applied to all points within each object geometry, and if there are several points within an object with different positional accuracies, the greatest value for positional accuracy within the object is applied for the whole object geometry. Positional accuracy is given in metres.
        /// </summary>
        [DataMember(Name = "horizontalGeometryEstimatedAccuracy")]
        public decimal? HorizontalGeometryEstimatedAccuracy { get; set; }

        /// <summary>
        /// The estimated absolute positional accuracy of the Z coordinates of the building geometry, in the INSPIRE official Coordinate Reference System. Absolute positional accuracy is defined as the mean value of the positional uncertainties for a set of positions where the positional uncertainties are defined as the distance between a measured position and what is considered as the corresponding true position. (Source: INSPIRE) The positional accuracy is applied to all points within each object geometry, and if there are several points within an object with different positional accuracies, the greatest value for positional accuracy within the object is applied for the whole object geometry. Positional accuracy is given in metres.
        /// </summary>
        [DataMember(Name = "verticalGeometryEstimatedAccuracy")]
        public decimal? VerticalGeometryEstimatedAccuracy { get; set; }

        /// <summary>
        /// Coordinate system for the (X, Y) coordinates of the geometry.
        /// </summary>
        public enum HorizontalCoordinateSystemEnum
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

        /// <summary>
        /// Coordinate system for the (X, Y) coordinates of the geometry.
        /// </summary>
        [Required]
        [DataMember(Name = "horizontalCoordinateSystem")]
        public HorizontalCoordinateSystemEnum? HorizontalCoordinateSystem { get; set; }

        /// <summary>
        /// Coordinate system for the Z coordinates of the geometry.
        /// </summary>
        public enum VerticalCoordinateSystemEnum
        {
            /// <summary>
            /// EPSG:5613
            /// </summary>
            [EnumMember(Value = "EPSG:5613")]
            EPSG5613 = 0
        }

        /// <summary>
        /// Coordinate system for the Z coordinates of the geometry.
        /// </summary>
        [DataMember(Name = "verticalCoordinateSystem")]
        public VerticalCoordinateSystemEnum? VerticalCoordinateSystem { get; set; }

        /// <summary>
        /// States whether the object geometry is described by two or three dimensions.
        /// </summary>
        public enum DimensionEnum
        {
            /// <summary>
            /// 2
            /// </summary>
            [EnumMember(Value = "2")]
            _2 = 0,
            /// <summary>
            /// 3
            /// </summary>
            [EnumMember(Value = "3")]
            _3 = 1
        }

        /// <summary>
        /// States whether the object geometry is described by two or three dimensions.
        /// </summary>
        [Required]
        [DataMember(Name = "dimension")]
        public DimensionEnum? Dimension { get; set; }
    }
}
