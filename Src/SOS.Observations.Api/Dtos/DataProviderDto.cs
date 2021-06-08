using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Information about a data provider.
    /// </summary>
    public class DataProviderDto
    {
        /// <summary>
        ///     Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     A unique text identifer for the data provider.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        ///     The name of the data provider.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Description of the data provider.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The organization name.
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Paths that can be used to group and visualize a data provider as a tree in a GUI.
        /// </summary>
        public IEnumerable<string> Path { get; set; }

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

        /// <summary>
        /// Date time from where next harvest can be run.
        /// </summary>
        public DateTime? NextHarvestFrom { get; set; }

        /// <summary>
        /// Note about harvest
        /// </summary>
        public string HarvestNotes { get; set; }

        /// <summary>
        /// Cron expression used to schedule harvest.
        /// </summary>
        public string HarvestSchedule { get; set; }

        /// <summary>
        /// Decides whether the data provider should be included in search when no data provider filter is set.
        /// </summary>
        public bool IncludeInSearchByDefault { get; set; }

        /// <summary>
        /// Creates a new DataProviderDto object.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static DataProviderDto Create(DataProvider dataProvider, string cultureCode)
        {
            if (dataProvider == null)
            {
                return null;
            }
            return new DataProviderDto
            {
                Id = dataProvider.Id,
                Identifier = dataProvider.Identifier,
                Name = dataProvider.Names?.Translate(cultureCode),
                Description = dataProvider.Descriptions?.Translate(cultureCode),
                Organization = dataProvider.Organizations?.Translate(cultureCode),
                Path = dataProvider.Paths?.Where(p => p.CultureCode?.Equals(cultureCode, StringComparison.CurrentCultureIgnoreCase) ?? false)?.SelectMany(p => p.Path),
                Url = dataProvider.Url,
                NextHarvestFrom = dataProvider.NextHarvestFrom(null),
                HarvestSchedule = dataProvider.HarvestSchedule,
                IncludeInSearchByDefault = dataProvider.IncludeInSearchByDefault
            };
        }

        /// <summary>
        ///  Creates a new DataProviderDto object.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="publicObservations"></param>
        /// <param name="protectedObservations"></param>
        /// <param name="latestHarvestDate"></param>
        /// <param name="harvestNotes"></param>
        /// <param name="latestProcessDate"></param>
        /// <param name="latestIncrementalHarvestDate"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static DataProviderDto Create(
            DataProvider dataProvider,
            int publicObservations,
            int protectedObservations,
            DateTime? latestHarvestDate,
            string harvestNotes,
            DateTime? latestProcessDate,
            DateTime? latestIncrementalHarvestDate,
            string cultureCode)
        {
            var dataProviderDto = Create(dataProvider, cultureCode);

            if (dataProviderDto == null)
            {
                return null;
            }

            dataProviderDto.PublicObservations = publicObservations;
            dataProviderDto.ProtectedObservations = protectedObservations;
            dataProviderDto.LatestHarvestDate = latestHarvestDate;
            dataProviderDto.HarvestNotes = harvestNotes;
            dataProviderDto.LatestProcessDate = latestProcessDate;
            dataProviderDto.LatestIncrementalHarvestDate = latestIncrementalHarvestDate;
            dataProviderDto.NextHarvestFrom = dataProvider.NextHarvestFrom(latestHarvestDate);
            return dataProviderDto;
        }
    }
}