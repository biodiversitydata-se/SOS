﻿using SOS.Lib.Models.Interfaces;
using System.Collections.Generic;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     This class contains basic taxon taxon information that can
    ///     be used for building a taxon tree.
    /// </summary>
    public class BasicTaxon : IEntity<int>, IBasicTaxon
    {
        /// <summary>
        /// Taxon attributes
        /// </summary>
        public TaxonAttributes Attributes { get; set; }
        /// <summary>
        ///     Object id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Secondary parents dyntaxa taxon ids.
        /// </summary>
        public IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
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
        public string ScientificNameAuthorship { get; set; }
        public string? SightingVernacularName { get; set; }
        public string? SightingScientificName { get; set; }
        public string? SightingScientificNameAuthorship { get; set; }
        public string VernacularName { get; set; }
    }
}