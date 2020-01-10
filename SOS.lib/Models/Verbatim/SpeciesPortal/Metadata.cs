using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Represents different metadata items
    /// </summary>
    public class Metadata
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public Metadata(int id)
        {
            Id = id;
            Translations = new List<MetadataTranslation>();
        }

        /// <summary>
        /// Id of item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of item
        /// </summary>
        public ICollection<MetadataTranslation> Translations { get; set; }
    }
}
