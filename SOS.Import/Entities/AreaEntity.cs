using Microsoft.SqlServer.Types;

namespace SOS.Import.Entities
{
    public class AreaEntity 
    {
        /// <summary>
        /// Type of area
        /// </summary>
        public int AreaDatasetId { get; set; }

        /// <summary>
        /// Area Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Area geometry
        /// </summary>
        public SqlGeometry Polygon { get; set; }

        /// <summary>
        /// Name of area
        /// </summary>
        public string Name { get; set; }
    }
}
        