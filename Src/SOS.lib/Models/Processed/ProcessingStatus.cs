using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Processed
{
    public class ProcessingStatus 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        protected ProcessingStatus(DataSet type)
        {
            Type = type;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderIdentifier"></param>
        /// <param name="type"></param>
        protected ProcessingStatus(string dataProviderIdentifier, DataSet type)
        {
            DataProviderIdentifier = dataProviderIdentifier;
            Type = type;
        }

        /// <summary>
        /// Number of items
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Data provider identifier
        /// </summary>
        public string DataProviderIdentifier { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public DataSet Type { get; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Running status
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public RunStatus Status { get; set; }


        public static ProcessingStatus Success(
            string dataProviderIdentifier,
            DataSet type,
            DateTime start,
            DateTime end,
            int count)
        {
            return new ProcessingStatus(dataProviderIdentifier, type)
            {
                Status = RunStatus.Success,
                Start = start,
                End = end,
                Count = count
            };
        }

        public static ProcessingStatus Failed(
            string dataProviderIdentifier,
            DataSet type,
            DateTime start,
            DateTime end)
        {
            return new ProcessingStatus(dataProviderIdentifier, type)
            {
                Status = RunStatus.Failed,
                Start = start,
                End = end
            };
        }

        public static ProcessingStatus Cancelled(
            string dataProviderIdentifier,
            DataSet type,
            DateTime start,
            DateTime end)
        {
            return new ProcessingStatus(dataProviderIdentifier, type)
            {
                Status = RunStatus.Canceled,
                Start = start,
                End = end
            };
        }
    }
}
