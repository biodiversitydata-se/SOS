using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Observations.Api.Dtos
{
    public class DataProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SwedishName { get; set; }
        public string Organization { get; set; }
        public string SwedishOrganization { get; set; }
        public string Description { get; set; }
        public string SwedishDescription { get; set; }
        public string Url { get; set; }
        public int NumberOfPublicObservations { get; set; }
        public int NumberOfProtectedObservations { get; set; }
        public DateTime? LatestHarvestDate { get; set; }

        public static DataProviderDto Create(DataProvider dataProvider)
        {
            return new DataProviderDto
            {
                Id = dataProvider.Id,
                Name = dataProvider.Name,
                SwedishName = dataProvider.SwedishName,
                Organization = dataProvider.Organization,
                SwedishOrganization = dataProvider.SwedishOrganization,
                Description = dataProvider.Description,
                SwedishDescription = dataProvider.SwedishDescription,
                Url = dataProvider.Url,
                NumberOfPublicObservations = dataProvider.PublicObservations,
                NumberOfProtectedObservations = dataProvider.ProtectedObservations,
                LatestHarvestDate = dataProvider.LatestHarvestDate
            };
        }
    }
}
