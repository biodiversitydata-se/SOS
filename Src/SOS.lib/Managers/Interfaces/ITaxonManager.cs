﻿using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using System.Collections.Generic;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     Taxon manager
    /// </summary>
    public interface ITaxonManager
    {
        /// <summary>
        ///     Taxon Tree
        /// </summary>
        TaxonTree<IBasicTaxon> TaxonTree { get; }

        /// <summary>
        /// Taxon lists taxon ids.
        /// </summary>
        public Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)> TaxonListSetById { get; }
    }
}