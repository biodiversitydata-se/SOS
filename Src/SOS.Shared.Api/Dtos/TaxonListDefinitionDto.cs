﻿using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;

namespace SOS.Shared.Api.Dtos
{
    /// <summary>
    /// Taxon list definition.
    /// </summary>
    public class TaxonListDefinitionDto
    {
        /// <summary>
        /// Is the list allowed in signal search?
        /// </summary>
        public bool CanBeUsedInSignalSearch { get; set; }

        /// <summary>
        /// The Id of the taxon list.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The parent Id of the taxon list.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        ///     The name of the taxon list.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a new TaxonListDefinitionDto object.
        /// </summary>
        /// <param name="taxonList"></param>
        /// <param name="cultureCode"></param>
        /// <param name="signalSearchTaxonListIds"></param>
        /// <returns></returns>
        public static TaxonListDefinitionDto Create(TaxonList taxonList, string cultureCode, IEnumerable<int> signalSearchTaxonListIds)
        {
            if (taxonList == null)
            {
                return null;
            }
            return new TaxonListDefinitionDto
            {
                CanBeUsedInSignalSearch = signalSearchTaxonListIds.Contains(taxonList.Id),
                Id = taxonList.Id,
                ParentId = taxonList.ParentId,
                Name = taxonList.Names?.Translate(cultureCode)
            };
        }
    }
}