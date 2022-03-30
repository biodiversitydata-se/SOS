using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;

namespace SOS.Observations.Api.IntegrationTests.CompleteIntegrationTests
{
    public static class ArtportalenObservationVerbatimFactory
    {
        public static ArtportalenObservationVerbatim Create()
        {
            var observation = new ArtportalenObservationVerbatim
            {
                Id = 5,
                DatasourceId = 1,
                DiscoveryMethod = new Metadata(0),
                DeterminationMethod = new Metadata(0),
                EditDate = DateTime.Now, // ISODate("2022-03-22T08:19:11.563+01:00")
                StartDate = DateTime.Now, // ISODate("2014-07-07T00:00:00.000+02:00")
                EndDate = DateTime.Now, // ISODate("2014-07-07T00:00:00.000+02:00")
                Gender = new Metadata(0),
                HasImages = false,
                HasTriggeredValidationRules = true,
                HasAnyTriggeredValidationRuleWithWarning = false,
                NoteOfInterest = false,
                HasUserComments = false,
                NotPresent = false,
                NotRecovered = false,
                ProtectedBySystem = false,
                ReportedDate = DateTime.Now, //ISODate("2022-03-22T08:18:48.140+01:00")
                RightsHolder = "Tom Volgers",
                Site = new Site
                {
                    Id = 6502743,
                    XCoord = 1683599,
                    YCoord = 8142284,
                    Accuracy = 25,
                    County = new GeographicalArea
                    {
                        FeatureId = "18",
                        Name = "Örebro"
                    },
                    CountyPartIdByCoordinate = "18",
                    BirdValidationAreaIds = new string[] { "100", "13" },
                    Municipality = new GeographicalArea
                    {
                        FeatureId = "1882",
                        Name = "Asersund"
                    },
                    Name = "Isåsen, Nrk",
                    Parish = new GeographicalArea
                    {
                        FeatureId = "2247",
                        Name = "Lerbäck"
                    },
                    PresentationNameParishRegion = "Isåsen, Lerbäck[[Shared_ParishName_Abbrevation]], Nrk",
                    Point = new GeoJsonGeometry
                    {
                        Type = "Point",
                        Coordinates = new System.Collections.ArrayList { 15.124, 58.8233 }
                    },
                    PointWithBuffer = new GeoJsonGeometry
                    {
                        Type = "Polygon",
                        Coordinates = new System.Collections.ArrayList { new double[][] {
                            new double[] { 15.124460958527196, 58.82325882868679 },
                            new double[] {15.124460958527196, 58.82325882868679},
                            new double[] {15.124452622831287, 58.823302641640886},
                            new double[] {15.124427936079675, 58.82334477088773},
                            new double[] {15.124387846970382, 58.82338359742399},
                            new double[] {15.124333896105398, 58.82341762916765},
                            new double[] {15.124268156786204, 58.823445558297884},
                            new double[] {15.124193155337974, 58.82346631151389},
                            new double[] {15.124111774024298, 58.8234790912812},
                            new double[] {15.124027140283424, 58.823483406480534},
                            new double[] {15.12394250654255, 58.8234790912812},
                            new double[] {15.123861125228874, 58.82346631151389},
                            new double[] {15.123786123780643, 58.823445558297884},
                            new double[] {15.12372038446145, 58.82341762916765},
                            new double[] {15.123666433596465, 58.82338359742399},
                            new double[] {15.123626344487173, 58.82334477088773},
                            new double[] {15.123601657735561, 58.823302641640886},
                            new double[] {15.123593322039651, 58.82325882868679},
                            new double[] {15.123601657735561, 58.82321501573269},
                            new double[] {15.123626344487173, 58.82317288648585},
                            new double[] {15.123666433596465, 58.82313405994959},
                            new double[] {15.12372038446145, 58.82310002820593},
                            new double[] {15.123786123780643, 58.823072099075695},
                            new double[] {15.123861125228874, 58.82305134585969},
                            new double[] {15.12394250654255, 58.82303856609238},
                            new double[] {15.124027140283424, 58.823034250893045},
                            new double[] {15.124111774024298, 58.82303856609238},
                            new double[] {15.124193155337974, 58.82305134585969},
                            new double[] {15.124268156786204, 58.823072099075695},
                            new double[] {15.124333896105398, 58.82310002820593},
                            new double[] {15.124387846970382, 58.82313405994959},
                            new double[] {15.124427936079675, 58.82317288648585},
                            new double[] {15.124452622831287, 58.82321501573269},
                            new double[] {15.124460958527196, 58.82325882868679}}
                        }
                    },
                    Province = new GeographicalArea
                    {
                        FeatureId = "10",
                        Name = "Närke"
                    },
                    ProvincePartIdByCoordinate = "10",
                    VerbatimCoordinateSystem = Lib.Enums.CoordinateSys.WebMercator
                },
                SpeciesGroupId = 1,
                Stage = new Metadata(0),
                TaxonId = 231326,
                Unit = new Metadata(0),
                Unspontaneous = false,
                UnsureDetermination = false,
                ValidationStatus = new Metadata(10),
                VerifiedBy = "Gerhard Boré",
                VerifiedByInternal = new List<UserInternal> { new UserInternal {
                    Id = 34,
                    UserAlias = "GB"
                }},
                Observers = "Tom Volgers",
                ObserversInternal = new List<UserInternal> { new UserInternal {
                    Id = 12,
                    UserAlias = "TV",
                    ViewAccess = true
                }},
                ReportedBy = "Tom Volgers",
                ReportedByUserId = 120,
                ReportedByUserServiceUserId = 12,
                ReportedByUserAlias = "TV",
                DeterminedBy = "Gerhard Boré",
                SightingId = 76,
                SightingTypeSearchGroupId = 1,
                SpeciesFactsIds = new List<int> { 4, 1938 },
                FirstImageId = 0
            };

            return observation;
        }
    }
}
