using Moq;
using SOS.Lib.Models.Shared;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.TestHelpers.Helpers;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class ProcessedFieldMappingRepositoryStubFactory
    {
        public static Mock<IProcessedFieldMappingRepository> Create(string filename = @"Resources\FieldMappings.msgpck")
        {
            var fieldMappings = MessagePackHelper.CreateListFromMessagePackFile<FieldMapping>(filename);
            var processedFieldMappingRepositoryStub = new Mock<IProcessedFieldMappingRepository>();
            processedFieldMappingRepositoryStub
                .Setup(pfmr => pfmr.GetAllAsync())
                .ReturnsAsync(fieldMappings);

            return processedFieldMappingRepositoryStub;
        }
    }
}