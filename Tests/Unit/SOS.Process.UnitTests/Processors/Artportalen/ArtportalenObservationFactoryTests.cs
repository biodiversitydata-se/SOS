using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Harvest.Processors.Artportalen;
using System.Collections.Generic;
using SOS.Lib.Configuration.Process;
using SOS.Harvest.Managers;
using Xunit;
using System;

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
            var processConfiguration = new ProcessConfiguration();
            var factory = new ArtportalenObservationFactory(
                dataProvider,
                new Dictionary<int, Taxon>(),
                new Dictionary<VocabularyId, IDictionary<object, int>>(),
                new Dictionary<int, ArtportalenObservationFactory.DatasetMapping>(),
                false,
                "https://artportalen-st.artdata.slu.se",
                new ProcessTimeManager(processConfiguration),
                processConfiguration);
            ArtportalenObservationVerbatim verbatimObservation = new ArtportalenObservationVerbatim();
            verbatimObservation.Activity = new MetadataWithCategory<int>(1, 1);
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

        [Theory]
        [MemberData(nameof(EventDatesTestData))]
        public void GetStartEndDate_AllInputs_Should_Return_Correct_Result(DateTime? startDate,
                    TimeSpan? startTime,
                    DateTime? endDate,
                    TimeSpan? endTime,
                    DateTime? expectedStartDate,
                    DateTime? expectedEndDate)
        {
            // Act
            ArtportalenObservationFactory.GetStartEndDate(
                startDate, 
                startTime, 
                endDate, 
                endTime, 
                out DateTime? startDateResult, 
                out DateTime? endDateResult);

            // Assert
            startDateResult.Should().Be(expectedStartDate);
            endDateResult.Should().Be(expectedEndDate);
        }

        public static IEnumerable<object[]> EventDatesTestData =>
            new List<object[]>
            {
                new object[] // Only startDate without time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // startDate
                    null, // startTime
                    null, // endDate
                    null, // endTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // expectedStartDate
                    null // expectedEndDate
                },
                new object[] // startDate and endDate without time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // startDate
                    null, // startTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // endDate
                    null, // endTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // expectedStartDate
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).Add(new TimeSpan(23, 59, 59)) // expectedEndDate
                },
                new object[] // startDate and endDate with time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // startDate
                    new TimeSpan(10, 15, 0), // startTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local), // endDate
                    new TimeSpan(12, 0, 0), // endTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).Add(new TimeSpan(10, 15, 0)), // expectedStartDate
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).Add(new TimeSpan(12, 0, 0)) // expectedEndDate
                },           
            };
    }
}
