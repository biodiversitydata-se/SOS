using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Interfaces
{
    /// <summary>
    /// Basic taxon information that can be used to build a taxon tree
    /// and calculate higher classification string.
    /// </summary>
    public interface IBasicTaxon
    {
        /// <summary>
        /// Object id
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Dyntaxa taxon id.
        /// </summary>
        int DyntaxaTaxonId { get; set; }

        /// <summary>
        /// Main parent Dyntaxa taxon id.
        /// </summary>
        int? ParentDyntaxaTaxonId { get; set; }

        /// <summary>
        /// Secondary parents dyntaxa taxon ids.
        /// </summary>
        IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
        /// Darwin Core term name: scientificName.
        /// The full scientific name, with authorship and date
        /// information if known. When forming part of an
        /// Identification, this should be the name in lowest level
        /// taxonomic rank that can be determined.
        /// This term should not contain identification qualifications,
        /// which should instead be supplied in the
        /// IdentificationQualifier term.
        /// Currently scientific name without author is provided.
        /// </summary>
        string ScientificName { get; set; }
    }
}
