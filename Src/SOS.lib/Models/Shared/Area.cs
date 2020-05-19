using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class Area : IEntity<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaType"></param>
        public Area(AreaType areaType)
        {
            AreaType = areaType;
        }

        /// <summary>
        /// Area Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of area
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of area
        /// </summary>
        public AreaType AreaType { get; private set; }
        
        /// <summary>
        /// Feature Id.
        /// </summary>
        public string FeatureId { get; set; }
    }
}
