namespace SOS.Process.UnitTests.Helpers
{
    public class AreaHelperTests
    {
        // todo - update Counties.geojson & Provinces.geojson by implementing GetFeaturecollection() in CreateAreaFilesTool class.

        //[Fact]
        //[Trait("Category", "Unit")]
        //public void ProvincePartIdByCoordinateShouldBeSetToLappland_When_ObservationIsInLappmark()
        //{
        //    //-----------------------------------------------------------------------------------------------------------
        //    // Arrange
        //    //-----------------------------------------------------------------------------------------------------------
        //    var areaTypes = new[] {AreaType.County, AreaType.Province};
        //    var areaHelper = new AreaHelper(
        //        AreaVerbatimRepositoryStubFactory.Create(areaTypes) .Object,
        //        ProcessedFieldMappingRepositoryStubFactory.Create().Object);
        //    var observation = new ProcessedSighting(DataProvider.Artportalen)
        //    {
        //        Location = new ProcessedLocation
        //        {
        //            DecimalLatitude = Coordinates.KirunaMunicipality.Latitude,
        //            DecimalLongitude = Coordinates.KirunaMunicipality.Longitude
        //        }
        //    };

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Act
        //    //-----------------------------------------------------------------------------------------------------------
        //    areaHelper.AddAreaDataToProcessedSighting(observation);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Assert
        //    //-----------------------------------------------------------------------------------------------------------
        //    observation.Location.ProvincePartIdByCoordinate.Should().Be((int)SpecialProvincePartId.Lappland);
        //    observation.Location.ProvinceId.Id.Should().Be((int)ProvinceId.AseleLappmark);
        //}

        //[Fact]
        //[Trait("Category", "Unit")]
        //public void CountyPartIdByCoordinateShouldBeSetToOland_When_ObservationIsOnOland()
        //{
        //    //-----------------------------------------------------------------------------------------------------------
        //    // Arrange
        //    //-----------------------------------------------------------------------------------------------------------
        //    var areaTypes = new[] { AreaType.County, AreaType.Province };
        //    var areaHelper = new AreaHelper(
        //        AreaVerbatimRepositoryStubFactory.Create(areaTypes).Object,
        //        ProcessedFieldMappingRepositoryStubFactory.Create().Object);
        //    var observations = new List<ProcessedSighting>();
        //    var observation = new ProcessedSighting(DataProvider.Artportalen)
        //    {
        //        Location = new ProcessedLocation
        //        {
        //            DecimalLatitude = Coordinates.BorgholmMunicipality.Latitude,
        //            DecimalLongitude = Coordinates.BorgholmMunicipality.Longitude
        //        }
        //    };
        //    observations.Add(observation);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Act
        //    //-----------------------------------------------------------------------------------------------------------
        //    areaHelper.AddAreaDataToProcessedSightings(observations);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Assert
        //    //-----------------------------------------------------------------------------------------------------------
        //    observation.Location.CountyPartIdByCoordinate.Should().Be((int)SpecialCountyPartId.Oland);
        //    observation.Location.CountyId.Id.Should().Be((int)CountyId.Kalmar);
        //}

        //[Fact]
        //[Trait("Category", "Unit")]
        //public void CountyPartIdShouldBeSetToKalmarFastland_When_ObservationIsInKalmar()
        //{
        //    //-----------------------------------------------------------------------------------------------------------
        //    // Arrange
        //    //-----------------------------------------------------------------------------------------------------------
        //    var areaTypes = new[] { AreaType.County, AreaType.Province };
        //    var areaHelper = new AreaHelper(
        //        AreaVerbatimRepositoryStubFactory.Create(areaTypes).Object,
        //        ProcessedFieldMappingRepositoryStubFactory.Create().Object);
        //    var observations = new List<ProcessedSighting>();
        //    var observation = new ProcessedSighting(DataProvider.Artportalen)
        //    {
        //        Location = new ProcessedLocation
        //        {
        //            DecimalLatitude = Coordinates.KalmarMunicipality.Latitude,
        //            DecimalLongitude = Coordinates.KalmarMunicipality.Longitude
        //        }
        //    };
        //    observations.Add(observation);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Act
        //    //-----------------------------------------------------------------------------------------------------------
        //    areaHelper.AddAreaDataToProcessedSightings(observations);

        //    //-----------------------------------------------------------------------------------------------------------
        //    // Assert
        //    //-----------------------------------------------------------------------------------------------------------
        //    observation.Location.CountyPartIdByCoordinate.Should().Be((int)SpecialCountyPartId.KalmarFastland);
        //    observation.Location.CountyId.Id.Should().Be((int)CountyId.Kalmar);
        //}
    }
}