using MongoDB.Bson;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class Area : IEntity<ObjectId>
    {
        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        public Area(AreaType areaType, string featureId)
        {
            AreaType = areaType;
            FeatureId = featureId;
        }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType AreaType { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Area Id
        /// </summary>
        public ObjectId Id { get; set; }
    }
}