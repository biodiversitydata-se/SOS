using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Processors.Artportalen;
using System.Collections.Generic;
using SOS.Lib.Configuration.Process;
using SOS.Process.Managers;
using Xunit;

namespace SOS.Process.UnitTests.Processors.Artportalen
{
    public class ArtportalenObservationFactoryTests
    {
        [Fact]
        public void Test_BirdNestActivityId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            //Mock<
            var dataProvider = new DataProvider();
            var factory = new ArtportalenObservationFactory(
                dataProvider, 
                new Dictionary<int, Taxon>(),
                new Dictionary<VocabularyId, IDictionary<object, int>>(),
                false,
                "https://artportalen-st.artdata.slu.se",
                new ProcessTimeManager(new ProcessConfiguration()));
            ArtportalenObservationVerbatim verbatimObservation = new ArtportalenObservationVerbatim();
            verbatimObservation.Activity = new MetadataWithCategory(1, 1);
            Taxon taxon = new Taxon();
            taxon.Attributes = new TaxonAttributes();
            taxon.Attributes.OrganismGroup = "fåglar";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var birdNestActivityId = factory.GetBirdNestActivityId(verbatimObservation, taxon);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            birdNestActivityId.Should().Be(1);
        }

    }
}
