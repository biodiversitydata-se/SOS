using System;
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
    /// <summary>
    ///     Data provider.
    /// </summary>
    public class DataProvider : IEntity<int>, IIdIdentifierTuple
    {
        /// <summary>
        ///     The harvest data format.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DataProviderType Type { get; set; }

        /// <summary>
        ///     Decides whether the data provider should be included in processing of observations and available for the search
        ///     API.
        /// </summary>
        public bool IsActive { get; set; }

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
        ///     Contact person.
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        ///     Contact person E-mail.
        /// </summary>
        public string ContactEmail { get; set; }

        /// <summary>
        ///     Download URL (for DwC-A files).
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        ///     Decides whether the data provider should be included in scheduled harvest.
        /// </summary>
        public bool IncludeInScheduledHarvest { get; set; }

        /// <summary>
        ///     Decides whether the data quality is approved.
        /// </summary>
        public bool DataQualityIsApproved { get; set; }

        /// <summary>
        ///     Latest harvest info. Currently used for testing purpose.
        /// </summary>
        public HarvestInfo
            HarvestInfo
        {
            get;
            set;
        } // todo - decide if we should remove this property and just keep harvesting info in HarvestInfo MongoDB collection.

        /// <summary>
        ///     Process info for instance 0. Currently used for testing purpose.
        /// </summary>
        public ProviderInfo
            ProcessInfoInstance0
        {
            get;
            set;
        } // todo - decide if we should remove this property and just keep processing info in ProcessInfo MongoDB collection.

        /// <summary>
        ///     Process info for instance 1. Currently used for testing purpose.
        /// </summary>
        public ProviderInfo
            ProcessInfoInstance1
        {
            get;
            set;
        } // todo - decide if we should remove this property and just keep processing info in ProcessInfo MongoDB collection.

        /// <summary>
        ///     Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     A unique identifer that is easier to understand than an Id number.
        /// </summary>
        public string Identifier { get; set; }

        public bool EqualsIdOrIdentifier(string idOrIdentifier)
        {
            if (int.TryParse(idOrIdentifier, out var id))
            {
                return Id == id;
            }

            return Identifier.Equals(idOrIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"[Id={Id}, Identfier={Identifier}] - {Name}";
        }

        protected bool Equals(DataProvider other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataProvider) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}