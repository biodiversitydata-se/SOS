using SOS.Lib.Models.Processed.DataStewardship.Common;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class OrganisationExtensions
    {
        public static OrganisationDto ToDto(this Organisation organisation)
        {
            if (organisation == null) return null;
            return new OrganisationDto
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }
    }
}
