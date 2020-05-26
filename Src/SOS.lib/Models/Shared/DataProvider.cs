using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Lib.Models.Shared
{
    public class DataProvider : IEntity<int>, IIdIdentifierTuple
    {
        public int Id { get; set; }
        /// <summary>
        /// A unique identifer that is easier to read than just an Id number.
        /// </summary>
        public string Identifier { get; set; }
        [BsonRepresentation(BsonType.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataProviderType Type { get; set; }
        public bool IsActive { get; set; }
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
        public bool IncludeInScheduledHarvest { get; set; }
        public bool DataQualityIsApproved { get; set; }
        public HarvestInfo HarvestInfo { get; set; }
        public ProviderInfo ProcessInfoInstance0 { get; set; }
        public ProviderInfo ProcessInfoInstance1 { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public int PublicObservations { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public int ProtectedObservations { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public DateTime? LatestHarvestDate { get; set; }
        [JsonIgnore]
        [BsonIgnore]
        public ICollection<DateTime> HarvestHistory { get; set; } // todo - change data type?
        [JsonIgnore]
        [BsonIgnore]
        public string HarvestSchedule { get; set; } // todo - change data type

        public bool EqualsIdOrIdentifier(string idOrIdentifier)
        {
            if (int.TryParse(idOrIdentifier, out int id))
            {
                return Id == id;
            }

            return Identifier.Equals(idOrIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"{Name} [Id={Id}, Identfier={Identifier}]";
        }

        protected bool Equals(DataProvider other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataProvider) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}