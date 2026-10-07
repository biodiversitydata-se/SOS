﻿using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions.Dto;

public static class TaxonExtensions
{
    extension(Taxon taxon)
    {
        public DsTaxonDto ToDto()
        {
            return new DsTaxonDto
            {
                ScientificName = taxon.ScientificName,
                TaxonID = taxon.Id.ToString(),
                TaxonRank = taxon.TaxonRank,
                VernacularName = taxon.VernacularName,
                VerbatimTaxonID = taxon.VerbatimId,
                VerbatimName = taxon.VerbatimName
            };
        }
    }
}
