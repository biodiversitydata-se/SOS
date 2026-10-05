using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Harvest.Managers;
using SOS.Harvest.Processors.iNaturalist;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using Xunit;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using SharedArea = SOS.Lib.Models.Shared.Area;

namespace SOS.Process.UnitTests.Processors.iNaturalist;

public class iNaturalistObservationFactoryTests
{
    private const string ObscuredVerbatimObservationJson = """
        {
          "_id": 813598,
          "ObservationId": "305227623",
          "Annotations": [
            {
              "Concatenated_attr_val": "22|25",
              "Controlled_attribute_id": 22,
              "Controlled_value_id": 25,
              "UserId": 3354081,
              "User": {
                "_id": 3354081,
                "Suspended": false,
                "Icon": "https://static.inaturalist.org/attachments/users/icons/3354081/thumb.jpg?1597775128",
                "Icon_url": "https://static.inaturalist.org/attachments/users/icons/3354081/medium.jpg?1597775128",
                "Login": "mikaelnyman",
                "Name": "Mikael Nyman"
              }
            }
          ],
          "Identifications": [
            {
              "_id": 679449610,
              "Created_at": {
                "DateTime": "2025-08-10T13:19:57.000Z",
                "Ticks": "638904359970000000",
                "Offset": 120
              },
              "Current": true,
              "Taxon": {
                "_id": 42048,
                "Iconic_taxon_id": 40151,
                "Iconic_taxon_name": "Mammalia",
                "Is_active": true,
                "Name": "Canis lupus",
                "Preferred_common_name": "Gray Wolf",
                "Rank": "species",
                "Rank_level": 10,
                "Ancestor_ids": [
                  48460,
                  1,
                  2,
                  355675,
                  40151,
                  848317,
                  848320,
                  848324,
                  41573,
                  42043,
                  42044
                ],
                "Ancestry": "48460/1/2/355675/40151/848317/848320/848324/41573/42043/42044"
              },
              "User": {
                "_id": 965267,
                "Suspended": false,
                "Icon": "https://static.inaturalist.org/attachments/users/icons/965267/74d0e6a67e9e6bf4b6f57633d23c1ab1-thumb.jpeg?1769524965",
                "Icon_url": "https://static.inaturalist.org/attachments/users/icons/965267/74d0e6a67e9e6bf4b6f57633d23c1ab1-medium.jpeg?1769524965",
                "Login": "sachauit",
                "Name": "Sacha Uit"
              }
            },
            {
              "_id": 679463558,
              "Created_at": {
                "DateTime": "2025-08-10T13:51:39.000Z",
                "Ticks": "638904378990000000",
                "Offset": 120
              },
              "Current": true,
              "Taxon": {
                "_id": 42048,
                "Iconic_taxon_id": 40151,
                "Iconic_taxon_name": "Mammalia",
                "Is_active": true,
                "Name": "Canis lupus",
                "Preferred_common_name": "Gray Wolf",
                "Rank": "species",
                "Rank_level": 10,
                "Ancestor_ids": [
                  48460,
                  1,
                  2,
                  355675,
                  40151,
                  848317,
                  848320,
                  848324,
                  41573,
                  42043,
                  42044
                ],
                "Ancestry": "48460/1/2/355675/40151/848317/848320/848324/41573/42043/42044"
              },
              "User": {
                "_id": 7936416,
                "Suspended": false,
                "Icon": "https://static.inaturalist.org/attachments/users/icons/7936416/1e884f5485e27eb945b0e440ce726681-thumb.jpeg?1732729835",
                "Icon_url": "https://static.inaturalist.org/attachments/users/icons/7936416/1e884f5485e27eb945b0e440ce726681-medium.jpeg?1732729835",
                "Login": "ivan87227",
                "Name": "Ivan Bronder"
              }
            }
          ],
          "PositionalAccuracy": 962,
          "PublicPositionalAccuracy": 24879,
          "Cached_votes_total": 0,
          "Captive": false,
          "Comments": [],
          "Comments_count": 0,
          "Created_at": {
            "DateTime": "2025-08-10T13:19:57.000Z",
            "Ticks": "638904359970000000",
            "Offset": 120
          },
          "Created_at_details": {
            "Date": {
              "DateTime": "2025-08-09T22:00:00.000Z",
              "Ticks": "638903808000000000",
              "Offset": 120
            },
            "Day": 10,
            "Hour": 15,
            "Month": 8,
            "Week": 32,
            "Year": 2025
          },
          "Created_time_zone": "Europe/Stockholm",
          "Description": "",
          "Faves_count": 0,
          "Geojson": {
            "Type": "Point",
            "Coordinates": [
              16.1507526952,
              59.9308152356
            ]
          },
          "Geoprivacy": "obscured",
          "Identifications_count": 1,
          "Identifications_most_agree": true,
          "Identifications_most_disagree": false,
          "Identifications_some_agree": true,
          "Location": "59.9308152356,16.1507526952",
          "Mappable": true,
          "Non_owner_ids": [
            {
              "_id": 679463558,
              "Created_at": {
                "DateTime": "2025-08-10T13:51:39.000Z",
                "Ticks": "638904378990000000",
                "Offset": 120
              },
              "Created_at_details": {
                "Date": {
                  "DateTime": "2025-08-09T22:00:00.000Z",
                  "Ticks": "638903808000000000",
                  "Offset": 120
                },
                "Day": 10,
                "Hour": 15,
                "Month": 8,
                "Week": 32,
                "Year": 2025
              },
              "User": {
                "_id": 7936416,
                "Suspended": false,
                "Icon": "https://static.inaturalist.org/attachments/users/icons/7936416/1e884f5485e27eb945b0e440ce726681-thumb.jpeg?1732729835",
                "Icon_url": "https://static.inaturalist.org/attachments/users/icons/7936416/1e884f5485e27eb945b0e440ce726681-medium.jpeg?1732729835",
                "Login": "ivan87227",
                "Name": "Ivan Bronder"
              }
            }
          ],
          "Num_identification_agreements": 1,
          "Num_identification_disagreements": 0,
          "Obscured": true,
          "Observed_on": {
            "DateTime": "2025-08-09T22:00:00.000Z",
            "Ticks": "638903808000000000",
            "Offset": 120
          },
          "Observed_on_details": {
            "Date": {
              "DateTime": "2025-08-09T22:00:00.000Z",
              "Ticks": "638903808000000000",
              "Offset": 120
            },
            "Day": 10,
            "Hour": 15,
            "Month": 8,
            "Week": 32,
            "Year": 2025
          },
          "Observed_on_string": "2025-08-10 15:19:38+02:00",
          "Observed_time_zone": "Europe/Stockholm",
          "Ofvs": [],
          "Photos": [
            {
              "_id": 550316791,
              "Attribution": "(c) Sacha Uit, all rights reserved",
              "Url": "https://static.inaturalist.org/photos/550316791/square.jpg"
            }
          ],
          "Place_guess": "Västmanland, SE",
          "Place_ids": [
            7599,
            12029,
            45043,
            59614,
            67952,
            80627,
            81490,
            96372,
            97391,
            108692,
            169180,
            206465
          ],
          "Project_ids": [],
          "Project_ids_with_curator_id": [],
          "Project_ids_without_curator_id": [],
          "Quality_grade": "research",
          "Reviewed_by": [
            965267,
            7936416
          ],
          "Site_id": 21,
          "Sounds": [],
          "Species_guess": "Wolf",
          "Tags": [],
          "Taxon": {
            "_id": 42048,
            "Iconic_taxon_id": 40151,
            "Iconic_taxon_name": "Mammalia",
            "Is_active": true,
            "Name": "Canis lupus",
            "Preferred_common_name": "Gray Wolf",
            "Rank": "species",
            "Rank_level": 10,
            "Ancestor_ids": [
              48460,
              1,
              2,
              355675,
              40151,
              848317,
              848320,
              848324,
              41573,
              42043,
              42044,
              42048
            ],
            "Ancestry": "48460/1/2/355675/40151/848317/848320/848324/41573/42043/42044",
            "Endemic": false,
            "Introduced": false,
            "Native": true,
            "Threatened": false
          },
          "Time_observed_at": {
            "DateTime": "2025-08-10T13:19:38.000Z",
            "Ticks": "638904359780000000",
            "Offset": 120
          },
          "Time_zone_offset": "+01:00",
          "Updated_at": {
            "DateTime": "2025-08-11T09:33:38.000Z",
            "Ticks": "638905088180000000",
            "Offset": 120
          },
          "Uri": "https://www.inaturalist.org/observations/305227623",
          "User": {
            "_id": 965267,
            "Suspended": false,
            "Icon": "https://static.inaturalist.org/attachments/users/icons/965267/74d0e6a67e9e6bf4b6f57633d23c1ab1-thumb.jpeg?1769524965",
            "Icon_url": "https://static.inaturalist.org/attachments/users/icons/965267/74d0e6a67e9e6bf4b6f57633d23c1ab1-medium.jpeg?1769524965",
            "Login": "sachauit",
            "Name": "Sacha Uit"
          }
        }
        """;

    [Fact]
    public void CreateProcessedObservation_Should_Process_Obscured_iNaturalist_Observation()
    {
        // Arrange
        var observation = DeserializeObservation(ObscuredVerbatimObservationJson);
        var factory = CreateFactory();

        // Act
        var processedObservation = factory.CreateProcessedObservation(observation, false);

        // Assert
        processedObservation.Should().NotBeNull();
        processedObservation.Event.PlainStartDate.Should().Be("2025-08-10");
        processedObservation.DataGeneralizations.Should().Be("The coordinates have been obfuscated to protect sensitive information");
        processedObservation.DatasetName.Should().Be("iNaturalist");
        processedObservation.Location.Locality.Should().Be("Västmanland, SE");
        processedObservation.Location.CoordinateUncertaintyInMeters.Should().Be(24879);
        processedObservation.Location.DecimalLatitude.Should().Be(59.93082D);
        processedObservation.Location.DecimalLongitude.Should().Be(16.15075D);
        processedObservation.Occurrence.CatalogNumber.Should().Be("305227623");
        processedObservation.Occurrence.OccurrenceId.Should().Be("https://www.inaturalist.org/observations/305227623");
        processedObservation.Occurrence.RecordedBy.Should().Be("Sacha Uit");
        processedObservation.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        processedObservation.Identification.IdentifiedBy.Should().Be("Sacha Uit, Ivan Bronder");
        processedObservation.Identification.Verified.Should().BeTrue();
        processedObservation.Identification.VerificationStatus.Id.Should().Be((int)ValidationStatusId.ApprovedBasedOnCommunityConsensus);
        processedObservation.Taxon.Id.Should().Be(123456);
        processedObservation.Taxon.ScientificName.Should().Be("Canis lupus");
        var media = processedObservation.Occurrence.Media.Should().ContainSingle().Subject;
        media.Identifier.Should().Be("https://static.inaturalist.org/photos/550316791/square.jpg");
        media.License.Should().Be("(c) Sacha Uit, all rights reserved");
        media.Creator.Should().Be("Sacha Uit");
        processedObservation.AccessRights.Id.Should().Be((int)AccessRightsId.FreeUsage);
        processedObservation.BasisOfRecord.Id.Should().Be((int)BasisOfRecordId.HumanObservation);
    }

    [Fact]
    public void CreateProcessedObservation_Should_Match_IconicTaxon_As_Complete_Ancestor_Name()
    {
        // Arrange
        var observation = DeserializeObservation(ObscuredVerbatimObservationJson);
        var taxa = new Dictionary<int, Taxon>
        {
            [111111] = new()
            {
                Id = 111111,
                ScientificName = "Canis lupus",
                HigherClassification = null
            },
            [222222] = new()
            {
                Id = 222222,
                ScientificName = "Canis lupus",
                HigherClassification = "Biota | Mammaliaformes"
            },
            [333333] = new()
            {
                Id = 333333,
                ScientificName = "Canis lupus",
                HigherClassification = "Biota | Animalia | Chordata | Mammalia | Carnivora"
            }
        };
        var factory = CreateFactory(taxa);

        // Act
        var processedObservation = factory.CreateProcessedObservation(observation, false);

        // Assert
        processedObservation.Taxon.Id.Should().Be(333333);
    }

    [Fact]
    public void CreateProcessedObservation_Should_Match_Rank()
    {
        // Arrange
        var observation = DeserializeObservation(ObscuredVerbatimObservationJson);
        var taxa = new Dictionary<int, Taxon>
        {
            [111111] = new()
            {
                Id = 111111,
                ScientificName = "Canis lupus",
                TaxonRank = "subspecies"
            },
            [222222] = new()
            {
                Id = 222222,
                ScientificName = "Canis lupus",
                TaxonRank = "kingdom"
            },
            [333333] = new()
            {
                Id = 333333,
                ScientificName = "Canis lupus",
                TaxonRank = "species"
            }
        };
        var factory = CreateFactory(taxa);

        // Act
        var processedObservation = factory.CreateProcessedObservation(observation, false);

        // Assert
        processedObservation.Taxon.Id.Should().Be(333333);
    }


    private static iNaturalistObservationFactory CreateFactory(IDictionary<int, Taxon>? taxa = null)
    {
        var processConfiguration = new ProcessConfiguration();
        var dataProvider = new DataProvider { Id = 19, Identifier = "iNaturalist" };
        var vocabularyById = new Dictionary<VocabularyId, IDictionary<object, int>>
        {
            [VocabularyId.AccessRights] = new Dictionary<object, int>
            {
                ["free usage"] = (int)AccessRightsId.FreeUsage
            }
        };
        taxa ??= new Dictionary<int, Taxon>
        {
            [123456] = new()
            {
                Id = 123456,
                ScientificName = "Canis lupus",
                VernacularName = "varg",
                Attributes = new TaxonAttributes
                {
                    OrganismGroup = "Däggdjur"
                },
                HigherClassification = "Biota | Mammalia"
            },
            [654321] = new()
            {
                Id = 654321,
                ScientificName = "Canis lupus",
                VernacularName = "varg",
                Attributes = new TaxonAttributes
                {
                    OrganismGroup = "Kärlväxter"
                },
                HigherClassification = "Biota | Kärlväxter"
            }
        };

        return new iNaturalistObservationFactory(
            dataProvider,
            taxa,
            vocabularyById,
            new NoOpAreaHelper(),
            new ProcessTimeManager(processConfiguration),
            processConfiguration,
            NullLogger<iNaturalistObservationFactory>.Instance);
    }

    private static iNaturalistVerbatimObservation DeserializeObservation(string json)
    {
        var token = JToken.Parse(json);
        NormalizeDateTokens(token);
        return JsonConvert.DeserializeObject<iNaturalistVerbatimObservation>(token.ToString())!;
    }

    private static void NormalizeDateTokens(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties().ToList())
            {
                if (property.Value is JObject dateObject &&
                    dateObject.TryGetValue("DateTime", StringComparison.Ordinal, out var dateTimeToken) &&
                    dateObject.TryGetValue("Ticks", StringComparison.Ordinal, out _) &&
                    dateObject.TryGetValue("Offset", StringComparison.Ordinal, out _))
                {
                    property.Value = dateTimeToken.Value<string>();
                    continue;
                }

                NormalizeDateTokens(property.Value);
            }

            return;
        }

        if (token is JArray array)
        {
            foreach (var item in array)
            {
                NormalizeDateTokens(item);
            }
        }
    }

    private sealed class NoOpAreaHelper : IAreaHelper
    {
        public bool IsInitialized => true;

        public void AddAreaDataToProcessedLocation(Location processedLocation)
        {
        }

        public void AddAreaDataToProcessedLocations(IEnumerable<Location> processedLocations)
        {
        }

        public void AddAreaDataToSite(Site site)
        {
        }

        public void ClearCache()
        {
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SharedArea>> GetAreasAsync(AreaType type)
        {
            return Task.FromResult<IEnumerable<SharedArea>>(Enumerable.Empty<SharedArea>());
        }

        public Task<Geometry> GetGeometryAsync(AreaType type, string featureId)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IFeature> GetPointFeatures(Point point)
        {
            return Enumerable.Empty<IFeature>();
        }
    }
}
