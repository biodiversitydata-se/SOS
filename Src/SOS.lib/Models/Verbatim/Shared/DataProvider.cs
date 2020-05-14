using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Org.BouncyCastle.Asn1.Crmf;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Shared
{
    public class DataProvider : IEntity<int>
    {
        public int Id { get; set; }
        /// <summary>
        /// A unique identifer that is easier to read than just an Id number.
        /// </summary>
        public string Identifier { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DataSet DataType { get; set; }
        public string Name { get; set; }
        public string SwedishName { get; set; }
        public string Organization { get; set; }
        public string SwedishOrganization { get; set; }
        public string Description { get; set; }
        public string SwedishDescription { get; set; }
        public string Url { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string DownloadUrl { get; set; }
        //public bool IncludeInSearchByDefault { get; set; }
        //public bool ActiveHarvesting { get; set; }

        [JsonIgnore]
        public int PublicObservations { get; set; }
        [JsonIgnore]
        public int ProtectedObservations { get; set; }
        [JsonIgnore]
        public DateTime? LatestHarvestDate { get; set; }
        [JsonIgnore]
        public ICollection<DateTime> HarvestHistory { get; set; } // todo - change data type?
        [JsonIgnore]
        public string HarvestSchedule { get; set; } // todo - change data type
    }
}