﻿using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamTreePortal;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class TreeObservationVerbatimRepository : VerbatimBaseRepository<TreeObservationVerbatim, ObjectId>, Interfaces.ITreeObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public TreeObservationVerbatimRepository(IVerbatimClient client,
            ILogger<TreeObservationVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}