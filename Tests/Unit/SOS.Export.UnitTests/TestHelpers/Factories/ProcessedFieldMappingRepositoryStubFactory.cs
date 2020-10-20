using Moq;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.TestHelpers.Helpers;

namespace SOS.Export.UnitTests.TestHelpers.Factories
{
    public static class ProcessedFieldMappingRepositoryStubFactory
    {
        public static Mock<IFieldMappingRepository> Create(string filename = @"Resources\FieldMappings.msgpck")
        {
            var fieldMappings = MessagePackHelper.CreateListFromMessagePackFile<FieldMapping>(filename);
            var processedFieldMappingRepositoryStub = new Mock<IFieldMappingRepository>();
            processedFieldMappingRepositoryStub
                .Setup(pfmr =>pfmr.GetAllAsync())
                .ReturnsAsync(fieldMappings);

            return processedFieldMappingRepositoryStub;
        }
    }
}