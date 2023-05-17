
namespace SOS.Harvest.Models.RegionalTaxonSensitiveCategories
{
    internal class Rule
    {
        /// <summary>
        /// Rule applies to specific activity/ies
        /// </summary>
        public IEnumerable<int>? ActivityIds { get; set; }

        /// <summary>
        /// Rule applies to specific area/s
        /// </summary>
        public IEnumerable<Area>? Areas { get; set; }

        /// <summary>
        /// Only observations made between start and end date are affected, only month and day used i year equals 0001
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Rule expire date, only month and day used i year equals 0001
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Rule applies to specific stage/s
        /// </summary>
        public IEnumerable<int>? StageIds { get; set; }

        /// <summary>
        /// Only observations made between start and end date are affected, only month and day used i year equals 0001
        /// </summary>
        public DateTime? StartDate { get; set; }
    }
}
