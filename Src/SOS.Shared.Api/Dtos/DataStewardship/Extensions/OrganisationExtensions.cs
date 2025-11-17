using SOS.Lib.Models.Processed.DataStewardship.Common;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions;

public static class OrganisationExtensions
{
    extension(Organisation organisation)
    {
        public DsOrganisationDto ToDto()
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
