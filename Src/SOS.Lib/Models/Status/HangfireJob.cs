using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace SOS.Lib.Models.Status;

/// <summary>A MongoDB job document with the database's native identifier.</summary>
[BsonIgnoreExtraElements]
public class HangfireJob
{
    public ObjectId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StateName { get; set; }
    public string InvocationData { get; set; }
}
