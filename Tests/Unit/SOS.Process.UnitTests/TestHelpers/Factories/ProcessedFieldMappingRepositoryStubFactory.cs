using System;
using System.Collections.Generic;
using System.Text;
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
            Mock<IProcessedFieldMappingRepository> processedFieldMappingRepositoryStub = new Mock<IProcessedFieldMappingRepository>();
            processedFieldMappingRepositoryStub
                .Setup(pfmr => pfmr.GetFieldMappingsAsync())
                .ReturnsAsync(fieldMappings);

            return processedFieldMappingRepositoryStub;
        }
    }
}
