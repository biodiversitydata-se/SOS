using System;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// API Usage statistics.
    /// </summary>
    public class ApiUsageStatistics : IEntity<ObjectId>
    {
        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }
        public DateTime Date { get; set; }
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public long RequestCount { get; set; }
        public long FailureCount { get; set; }
        public long AverageDuration { get; set; }
    }
}