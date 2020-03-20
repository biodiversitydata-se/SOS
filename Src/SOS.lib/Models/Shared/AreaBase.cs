using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class AreaBase : IEntity<int>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaType"></param>
        public AreaBase(AreaType areaType)
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
        public int FeatureId { get; set; }
        
        /// <summary>
        /// Parent Id.
        /// </summary>
        public int? ParentId { get; set; }
    }
}
