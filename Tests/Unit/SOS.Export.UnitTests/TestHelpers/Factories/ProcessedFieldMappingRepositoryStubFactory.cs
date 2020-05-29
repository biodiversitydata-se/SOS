using Moq;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Shared;
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
                .Setup(pfmr => pfmr.GetFieldMappingsAsync())
                .ReturnsAsync(fieldMappings);

            return processedFieldMappingRepositoryStub;
        }
    }
}