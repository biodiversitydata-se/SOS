using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
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
