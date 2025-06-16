using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SOS.Shared.Api.Dtos.Status
{
    [BsonIgnoreExtraElements]
    public class HangfireJobDto
    {
        public ObjectId Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StateName { get; set; }
        public string InvocationData { get; set; }
    }
}
