using Nest;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Substrate info.
    /// </summary>
    public class Substrate
    {
        /// <summary>
        ///     Description of substrate.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Substrate id.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        ///     Substrate.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue Name { get; set; }

        /// <summary>
        /// Quantity of substrate.
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        ///     Description of substrate species.
        /// </summary>
        public string SpeciesDescription { get; set; }

        /// <summary>
        ///     Substrate taxon id.
        /// </summary>
        public int? SpeciesId { get; set; }

        /// <summary>
        ///     Scientific name of substrate species.
        /// </summary>
        public string SpeciesScientificName { get; set; }

        /// <summary>
        ///     Vernacular name of substrate species.
        /// </summary>
        public string SpeciesVernacularName { get; set; }
    }
}