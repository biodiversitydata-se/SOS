using Moq;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.TestHelpers.Helpers;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedFieldMappingRepositoryStubFactory
    {
        public static Mock<IProcessedFieldMappingRepository> Create(string filename = @"Resources\FieldMappings.msgpck")
        {
            var fieldMappings = MessagePackHelper.CreateListFromMessagePackFile<FieldMapping>(filename);
            var processedFieldMappingRepositoryStub = new Mock<IProcessedFieldMappingRepository>();
            processedFieldMappingRepositoryStub
                .Setup(pfmr =>pfmr.GetAllAsync())
                .ReturnsAsync(fieldMappings);

            return processedFieldMappingRepositoryStub;
        }
    }
}