﻿using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;

namespace SOS.Lib.Database
{
    /// <inheritdoc />
    public class VerbatimClient : MongoDbClient, IVerbatimClient
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="dataBaseName"></param>
        /// <param name="readBatchSize"></param>
        /// <param name="writeBatchSize"></param>
        public VerbatimClient(MongoClientSettings settings, string dataBaseName, int readBatchSize, int writeBatchSize) :
            base(settings, dataBaseName, readBatchSize, writeBatchSize)
        {

        }
    }
}