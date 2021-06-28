using System;
using System.Collections.Generic;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Log
{
    /// <summary>
    /// Protected observation log
    /// </summary>
    public class ProtectedLog : IEntity<ObjectId>
    {
        /// <summary>
        /// Identifier of application making the request
        /// </summary>
        public string ApplicationIdentifier { get; set; }

        /// <summary>
        /// Item unique id
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// IP request was made from
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Date request was made
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// List of observations returned
        /// </summary>
        public IEnumerable<string> OccurenceIds { get; set; }

        /// <summary>
        /// Person making the request
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Person making the request
        /// </summary>
        public string UserId { get; set; }
    }
}
