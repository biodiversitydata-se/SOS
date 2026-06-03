using SOS.Lib.Models.Shared;
using SOS.Lib.Dtos;

namespace SOS.Lib.Extensions.Dto;

public static class TaxonListExtensions
{
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