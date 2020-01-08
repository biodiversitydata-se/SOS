using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;

namespace SOS.Export.Test.TestHelpers.Stubs
{
    public class ProcessedDarwinCoreRepositoryStub : IProcessedDarwinCoreRepository
    {
        public byte ActiveInstance => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<DarwinCore<DynamicProperties>> GetAsync(ObjectId id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DarwinCore<DynamicProperties>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(AdvancedFilter filter, int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}
