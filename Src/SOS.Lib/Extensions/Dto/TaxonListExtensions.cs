using SOS.Api.Contracts.TaxonLists.Responses;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Extensions.Dto;

public static class TaxonListExtensions
{
    extension(TaxonList taxonList)
    {
        public TaxonListDto ToTaxonListDto(string cultureCode)
        {
            if (taxonList == null) return null!;

            return new TaxonListDto
            {
                Id = taxonList.Id,
                ParentId = taxonList.ParentId,
                TaxonListServiceId = taxonList.TaxonListServiceId,
                Name = taxonList.Names.Translate(cultureCode),
                Taxa = taxonList.Taxa.Select(m => m.ToTaxonListTaxonInformationDto()).ToList()
            };
        }

        public TaxonListDefinitionDto ToTaxonListDefinitionDto(string cultureCode, IEnumerable<int> signalSearchTaxonListIds)
        {
            if (taxonList == null) return null!;

            return new TaxonListDefinitionDto
            {
                CanBeUsedInSignalSearch = signalSearchTaxonListIds.Contains(taxonList.Id),
                Id = taxonList.Id,
                ParentId = taxonList.ParentId,
                Name = taxonList.Names?.Translate(cultureCode)!
            };
        }
    }

    extension(TaxonListTaxonInformation taxonInformation)
    {
        public TaxonListTaxonInformationDto ToTaxonListTaxonInformationDto(
)
        {
            return new TaxonListTaxonInformationDto
            {
                Id = taxonInformation.Id,
                ScientificName = taxonInformation.ScientificName,
                SwedishName = taxonInformation.SwedishName
            };
        }
    }
}
