using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
namespace SOS.Administration.Gui.Models
{

    [BsonIgnoreExtraElements]
    public class ProcessInfoDto
    {
        [BsonIgnoreExtraElements]
        public class Provider
        {
            public int? DataProviderId { get; set; }
            public string DataProviderIdentifier { get; set; }
            public DateTime? ProcessEnd { get; set; }
            public DateTime? ProcessStart { get; set; }
            public string ProcessStatus { get; set; }
            public int? PublicProcessCount { get; set; }
            public int? ProtectedProcessCount { get; set; }
            public DateTime? HarvestEnd { get; set; }
            public DateTime? HarvestStart { get; set; }
            public string HarvestNotes { get; set; }
            public string HarvestStatus { get; set; }
            public int? HarvestCount { get; set; }
            public DateTime? LatestIncrementalEnd { get; set; }
            public DateTime? LatestIncrementalStart { get; set; }
            public int? LatestIncrementalStatus { get; set; }
            public int? LatestIncrementalPublicCount { get; set; }
            public int? LatestIncrementalProtectedCount { get; set; }
            public string DataProviderType { get; set; }
        }
        public string Id { get; set; }

        public int PublicCount { get; set; }
        public int ProtectedCount { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public string Status { get; set; }
        public IEnumerable<Provider> ProvidersInfo { get; set; }
    }
}
