using SOS.Lib.Models.Processed.DataStewardship.Common;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class OrganisationExtensions
    {
        public static DsOrganisationDto ToDto(this Organisation organisation)
        {
            if (organisation == null) return null;
            return new DsOrganisationDto
            {
                OrganisationID = organisation.OrganisationID,
                OrganisationCode = organisation.OrganisationCode
            };
        }
    }
}
