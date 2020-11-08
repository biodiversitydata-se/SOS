using Moq;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.TestHelpers.Helpers;

namespace SOS.Process.UnitTests.TestHelpers.Factories
{
    public static class VocabularyRepositoryStubFactory
    {
        public static Mock<IVocabularyRepository> Create(string filename = @"Resources\Vocabularies.msgpck")
        {
            var vocabularies = MessagePackHelper.CreateListFromMessagePackFile<Vocabulary>(filename);
            var vocabularyRepositoryStub = new Mock<IVocabularyRepository>();
            vocabularyRepositoryStub
                .Setup(pfmr => pfmr.GetAllAsync())
                .ReturnsAsync(vocabularies);

            return vocabularyRepositoryStub;
        }
    }
}