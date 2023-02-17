using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using System;

namespace SOS.Lib.Models.Export
{
    public class ExportJobInfo : IEntity<string>
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessStartDate { get; set; }
        public DateTime? ProcessEndDate { get; set; }
        public int? NumberOfObservations { get; set; }
        public string Description { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ExportFormat Format { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ExportJobStatus Status { get; set; }
        public string ErrorMsg { get; set; }
        [BsonRepresentation(BsonType.String)]
        public OutputFieldSet? OutputFieldSet { get; set; }
        public string PickUpUrl { get; set; }
        public double LifetimeDays { get; set; }
    }
}
