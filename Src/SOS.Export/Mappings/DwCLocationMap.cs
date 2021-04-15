using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCLocationMap : ClassMap<DwCLocation>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCLocationMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreID");
            Map(m => m.LocationID).Index(1).Name("locationID");
            Map(m => m.HigherGeographyID).Index(2).Name("higherGeographyID");
            Map(m => m.HigherGeography).Index(3).Name("higherGeography");
            Map(m => m.Continent).Index(4).Name("continent");
            Map(m => m.WaterBody).Index(5).Name("waterBody");
            Map(m => m.IslandGroup).Index(6).Name("islandGroup");
            Map(m => m.Island).Index(7).Name("island");
            Map(m => m.Country).Index(8).Name("country");
            Map(m => m.CountryCode).Index(9).Name("countryCode");
            Map(m => m.StateProvince).Index(10).Name("stateProvince");
            Map(m => m.County).Index(11).Name("county");
            Map(m => m.Municipality).Index(12).Name("municipality");
            Map(m => m.Locality).Index(13).Name("locality");
            Map(m => m.VerbatimLocality).Index(14).Name("verbatimLocality");
            Map(m => m.MinimumElevationInMeters).Index(15).Name("minimumElevationInMeters");
            Map(m => m.MaximumElevationInMeters).Index(16).Name("maximumElevationInMeters");
            Map(m => m.VerbatimElevation).Index(17).Name("verbatimElevation");
            Map(m => m.MinimumDepthInMeters).Index(18).Name("minimumDepthInMeters");
            Map(m => m.MaximumDepthInMeters).Index(19).Name("maximumDepthInMeters");
            Map(m => m.VerbatimDepth).Index(20).Name("verbatimDepth");
            Map(m => m.MinimumDistanceAboveSurfaceInMeters).Index(21).Name("minimumDistanceAboveSurfaceInMeters");
            Map(m => m.MaximumDistanceAboveSurfaceInMeters).Index(22).Name("maximumDistanceAboveSurfaceInMeters");
            Map(m => m.LocationAccordingTo).Index(23).Name("locationAccordingTo");
            Map(m => m.LocationRemarks).Index(24).Name("locationRemarks");
            Map(m => m.DecimalLatitude).Index(25).Name("decimalLatitude");
            Map(m => m.DecimalLongitude).Index(26).Name("decimalLongitude");
            Map(m => m.GeodeticDatum).Index(27).Name("geodeticDatum");
            Map(m => m.CoordinateUncertaintyInMeters).Index(28).Name("coordinateUncertaintyInMeters");
            Map(m => m.CoordinatePrecision).Index(29).Name("coordinatePrecision");
            Map(m => m.PointRadiusSpatialFit).Index(30).Name("pointRadiusSpatialFit");
            Map(m => m.VerbatimCoordinates).Index(31).Name("verbatimCoordinates");
            Map(m => m.VerbatimLatitude).Index(32).Name("verbatimLatitude");
            Map(m => m.VerbatimLongitude).Index(33).Name("verbatimLongitude");
            Map(m => m.VerbatimCoordinateSystem).Index(34).Name("verbatimCoordinateSystem");
            Map(m => m.VerbatimSRS).Index(35).Name("verbatimSRS");
            Map(m => m.FootprintWKT).Index(36).Name("footprintWKT");
            Map(m => m.FootprintSRS).Index(37).Name("footprintSRS");
            Map(m => m.FootprintSpatialFit).Index(38).Name("footprintSpatialFit");
            Map(m => m.GeoreferencedBy).Index(39).Name("georeferencedBy");
            Map(m => m.GeoreferencedDate).Index(40).Name("georeferencedDate");
            Map(m => m.GeoreferenceProtocol).Index(41).Name("georeferenceProtocol");
            Map(m => m.GeoreferenceSources).Index(42).Name("georeferenceSources");
            Map(m => m.GeoreferenceVerificationStatus).Index(43).Name("georeferenceVerificationStatus");
            Map(m => m.GeoreferenceRemarks).Index(44).Name("georeferenceRemarks");
        }
    }
}