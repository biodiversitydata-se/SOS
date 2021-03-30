using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Processed.ProcessInfo
{
    /// <summary>
    ///     Information about verbatim
    /// </summary>
    public class ProviderInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        public ProviderInfo(DataProvider dataProvider)
        {
            DataProviderId = dataProvider.Id;
            DataProviderIdentifier = dataProvider.Identifier;
        }

        public int? DataProviderId { get; set; }

        public string DataProviderIdentifier { get; set; }

        /// <summary>
        ///     Number of items harvested
        /// </summary>
        public int? HarvestCount { get; set; }

        /// <summary>
        ///     Harvest end date and time
        /// </summary>
        public DateTime? HarvestEnd { get; set; }

        /// <summary>
        ///     Harvest note
        /// </summary>
        public string HarvestNotes { get; set; }

        /// <summary>
        ///     Harvest start date and time
        /// </summary>
        public DateTime? HarvestStart { get; set; }

        /// <summary>
        ///     Status of harvest
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public RunStatus? HarvestStatus { get; set; }

        /// <summary>
        /// Last incremental process count 
        /// </summary>
        public int? LatestIncrementalPublicCount { get; set; }

        /// <summary>
        /// Last incremental process count 
        /// </summary>
        public int? LatestIncrementalProtectedCount { get; set; }

        /// <summary>
        /// Last incremental process end 
        /// </summary>
        public DateTime? LatestIncrementalEnd { get; set; }

        /// <summary>
        /// Last incremental process status 
        /// </summary>
        public RunStatus? LatestIncrementalStatus { get; set; }

        /// <summary>
        /// Last incremental process start 
        /// </summary>
        public DateTime? LatestIncrementalStart { get; set; }

        /// <summary>
        ///     Number of public observations processed
        /// </summary>
        public int? PublicProcessCount { get; set; }

        /// <summary>
        ///     Number of protected observations processed
        /// </summary>
        public int? ProtectedProcessCount { get; set; }

        /// <summary>
        ///     Process end date and time
        /// </summary>
        public DateTime? ProcessEnd { get; set; }

        /// <summary>
        ///     Process start date and time
        /// </summary>
        public DateTime ProcessStart { get; set; }

        /// <summary>
        ///     Status of processing
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public RunStatus? ProcessStatus { get; set; }

        /// <summary>
        ///     Id of data provider
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public DataProviderType DataProviderType { get; private set; }

    }
}