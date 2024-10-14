using System;

namespace SOS.Lib.Models.TaxonAttributeService
{
    public class RedlistPeriod
    {
        /// <summary>
        ///    Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///    Name of redlist period
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Current value
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Period end date
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}