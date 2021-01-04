using System;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    ///     Data provider DTO.
    /// </summary>
    public class DataProviderDto
    {
        /// <summary>
        ///     Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     A unique identifer that is easier to understand than an Id number.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        ///     The name of the data provider (in english).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The name of the data provider (in swedish).
        /// </summary>
        public string SwedishName { get; set; }

        /// <summary>
        ///     The organization name (in english).
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        ///     The organization name (in swedish).
        /// </summary>
        public string SwedishOrganization { get; set; }

        /// <summary>
        ///     Description of the data provider (in english).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Description of the data provider (in swedish).
        /// </summary>
        public string SwedishDescription { get; set; }

        /// <summary>
        ///     URL to the data provider source.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Number of public observations.
        /// </summary>
        public int PublicObservations { get; set; }

        /// <summary>
        ///     Number of protected observations.
        /// </summary>
        public int ProtectedObservations { get; set; }

        /// <summary>
        ///     Latest harvest date.
        /// </summary>
        public DateTime? LatestHarvestDate { get; set; }

        /// <summary>
        ///     Latest process date.
        /// </summary>
        public DateTime? LatestProcessDate { get; set; }

        /// <summary>
        ///     Latest incremental harvest and process date. Used for data providers supporting incremental harvest.
        /// </summary>
        public DateTime? LatestIncrementalHarvestDate { get; set; }

        public DateTime? NextHarvestFrom { get; set; }

        /// <summary>
        ///     Creates a new DataProviderDto object.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public static DataProviderDto Create(DataProvider dataProvider)
        {
            return new DataProviderDto
            {
                Id = dataProvider.Id,
                Identifier = dataProvider.Identifier,
                Name = dataProvider.Name,
                SwedishName = dataProvider.SwedishName,
                Organization = dataProvider.Organization,
                SwedishOrganization = dataProvider.SwedishOrganization,
                Description = dataProvider.Description,
                SwedishDescription = dataProvider.SwedishDescription,
                Url = dataProvider.Url,
                NextHarvestFrom = dataProvider.NextHarvestFrom(null)
            };
        }

        /// <summary>
        ///     Creates a new DataProviderDto object.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="publicObservations"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="latestHarvestDate"></param>
        /// <param name="latestProcessDate"></param>
        /// <param name="latestIncrementalHarvestDate"></param>
        /// <returns></returns>
        public static DataProviderDto Create(
            DataProvider dataProvider,
            int publicObservations,
            int protectedObservations,
            DateTime? latestHarvestDate,
            DateTime? latestProcessDate,
            DateTime? latestIncrementalHarvestDate)
        {
            return new DataProviderDto
            {
                Id = dataProvider.Id,
                Identifier = dataProvider.Identifier,
                Name = dataProvider.Name,
                SwedishName = dataProvider.SwedishName,
                Organization = dataProvider.Organization,
                SwedishOrganization = dataProvider.SwedishOrganization,
                Description = dataProvider.Description,
                SwedishDescription = dataProvider.SwedishDescription,
                Url = dataProvider.Url,
                PublicObservations = publicObservations,
                ProtectedObservations = protectedObservations,
                LatestHarvestDate = latestHarvestDate,
                LatestProcessDate = latestProcessDate,
                LatestIncrementalHarvestDate = latestIncrementalHarvestDate,
                NextHarvestFrom = dataProvider.NextHarvestFrom(latestHarvestDate)
            };
        }
    }
}