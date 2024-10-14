﻿using MongoDB.Bson;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    public interface IApiUsageStatisticsRepository : IRepositoryBase<ApiUsageStatistics, ObjectId>
    {
        Task<DateTime?> GetLatestHarvestDate();
        Task CreateIndexAsync();
        Task VerifyCollection();
    }
}