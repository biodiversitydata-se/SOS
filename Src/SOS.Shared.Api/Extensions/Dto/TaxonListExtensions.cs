using SOS.Lib.Models.Shared;
using SOS.Shared.Api.Dtos;

namespace SOS.Shared.Api.Extensions.Dto
{
    public static class TaxonListExtensions
    {
        public static TaxonListTaxonInformationDto ToTaxonListTaxonInformationDto(
            this TaxonListTaxonInformation taxonInformation)
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