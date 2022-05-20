using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using System.Linq;
using LinqStatistics;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using System.ComponentModel;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationProcessing.Artportalen
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class ArtportalenChecklistProcessorTests
    {
        private readonly IntegrationTestFixture _fixture;

        public ArtportalenChecklistProcessorTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]        
        public void Test_process_verbatim_checklists()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimChecklists = Builder<ArtportalenCheckListVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedChecklists()                
                .Build();            

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var checklists = _fixture.ProcessChecklists(verbatimChecklists);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            checklists.Count.Should().Be(100);
        }
    }
}