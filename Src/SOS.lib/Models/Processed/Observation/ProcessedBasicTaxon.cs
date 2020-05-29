using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     This class contains basic taxon taxon information that can
    ///     be used for building a taxon tree.
    /// </summary>
    public class ProcessedBasicTaxon : IEntity<int>, IBasicTaxon
    {
        /// <summary>
        ///     Dyntaxa taxon id.
        /// </summary>
        public int DyntaxaTaxonId { get; set; }

        /// <summary>
        ///     Main parent Dyntaxa taxon id.
        /// </summary>
        public int? ParentDyntaxaTaxonId { get; set; }

        /// <summary>
        ///     Secondary parents dyntaxa taxon ids.
        /// </summary>
        public IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
        ///     Darwin Core term name: scientificName.
        ///     The full scientific name, with authorship and date
        ///     information if known. When forming part of an
        ///     Identification, this should be the name in lowest level
        ///     taxonomic rank that can be determined.
        ///     This term should not contain identification qualifications,
        ///     which should instead be supplied in the
        ///     IdentificationQualifier term.
        ///     Currently scientific name without author is provided.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public int Id { get; set; }
    }
}