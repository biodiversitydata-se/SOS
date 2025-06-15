using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SOS.Shared.Api.Dtos.Status
{
    [BsonIgnoreExtraElements]
    public class HarvestInfoDto
    {
        public string Id { get; set; }

        public int Count { get; set; }

        public DateTime? DataLastModified { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
