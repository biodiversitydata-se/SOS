using SOS.Observations.Api.Dtos.Enum;

namespace SOS.Observations.Api.Dtos
{
    public class AreaBaseDto
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaTypeDto AreaType { get; set; }

        /// <summary>
        /// Feature id
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }
    }
}