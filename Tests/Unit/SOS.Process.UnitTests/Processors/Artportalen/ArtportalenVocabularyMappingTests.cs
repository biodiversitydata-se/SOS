using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Shared;
using SOS.Process.UnitTests.TestHelpers;
using Xunit;

namespace SOS.Process.UnitTests.Processors.Artportalen
{
    [CollectionDefinition("ArtportalenObservationFactory collection")]
    public class ArtportalenVocabularyMappingTests : IClassFixture<ArtportalenObservationFactoryFixture>
    {
        private readonly ArtportalenObservationFactoryFixture _fixture;
        
        public ArtportalenVocabularyMappingTests(ArtportalenObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Get_ValidationStatus_that_exists_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var activity = new Metadata<int>((int)ValidationStatusId.DialogueWithReporter);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(activity, VocabularyId.VerificationStatus);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Id.Should().Be((int)ValidationStatusId.DialogueWithReporter);
        }

        [Fact]
        public void Get_organization_that_doesnt_exist_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var ownerOrganization = new Metadata<int>(-1)
            {
                Translations = new List<MetadataTranslation>
                {
                    new MetadataTranslation {Culture = Cultures.sv_SE, Value = "Test"}
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(ownerOrganization, VocabularyId.Institution);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Id.Should().Be(VocabularyConstants.NoMappingFoundCustomValueIsUsedId);
            result.Value.Should().Be("Test");
        }

        [Fact]
        public void Get_ReproductiveCondition_that_exists_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var activity = new Metadata<int>((int)ActivityId.BroodPatch);
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(activity, VocabularyId.ReproductiveCondition);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Id.Should().Be((int)ReproductiveConditionId.BroodPatch);
        }

        [Fact]
        public void Get_ReproductiveCondition_that_doesnt_exists_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var activity = new Metadata<int>((int) ActivityId.NestBuilding);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(activity, VocabularyId.ReproductiveCondition, null, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(null);
        }

        [Fact]
        public void Get_Behavior_that_exists_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var activity = new Metadata<int>((int)ActivityId.DisplayOrSong);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(activity, VocabularyId.Behavior);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Id.Should().Be((int)BehaviorId.DisplayOrSong);
        }

        [Fact]
        public void Get_Behavior_that_doesnt_exists_in_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var activity = new Metadata<int>((int)ActivityId.BroodPatch);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.ArtportalenObservationFactory.GetSosIdFromMetadata(activity, VocabularyId.Behavior, null, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(null);
        }
    }
}