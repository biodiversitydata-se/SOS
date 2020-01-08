using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.DarwinCore;

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

        public Task<IEnumerable<DarwinCore<DynamicProperties>>> GetChunkAsync(int skip, int take)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DarwinCoreProject>> GetProjectParameters(int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}
