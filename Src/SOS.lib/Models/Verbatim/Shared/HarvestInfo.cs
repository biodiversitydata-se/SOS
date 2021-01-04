using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;


namespace SOS.Lib.Models.Verbatim.Shared
{
    public class HarvestInfo : IEntity<string>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="start"></param>
        public HarvestInfo(DateTime start)
        {
            Start = start;
        }

        /// <summary>
        ///     Number of items
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     Last time data was modified
        /// </summary>
        public DateTime? DataLastModified { get; set; }

        /// <summary>
        ///     Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     Running status
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public RunStatus Status { get; set; }

        /// <summary>
        ///     Id of data set
        /// </summary>
        public string Id { get; set; }
    }
}