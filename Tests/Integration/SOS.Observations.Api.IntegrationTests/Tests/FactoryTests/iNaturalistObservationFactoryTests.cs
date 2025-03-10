using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.FactoryTests;

[Collection(TestCollection.Name)]
public class iNaturalistObservationFactoryTests : TestBase
{
    public iNaturalistObservationFactoryTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    private string _verbatimObservationJson = """
        {
          "ObservationId": 3256,
          "Annotations": [],
          "Identifications": [
            {
              "Id": 27931382,
              "Created_at": "2018-05-26T11:21:11+00:00",
              "Current": true,
              "Taxon": {
                "Ancestor_ids": [
                  48460,
                  1,
                  2,
                  355675,
                  3,
                  67561,
                  4342,
                  447155,
                  4343,
                  4381
                ],
                "Ancestry": "48460/1/2/355675/3/67561/4342/447155/4343/4381",
                "Id": 515475,
                "Iconic_taxon_id": 3,
                "Iconic_taxon_name": "Aves",
                "Is_active": true,
                "Name": "Larus fuscus fuscus",
                "Preferred_common_name": "Baltic Gull",
                "Rank": "subspecies",
                "Rank_level": 5.0
              },
              "User": {
                "Id": 275,
                "Suspended": false,
                "Login": "paco"
              }
            }
          ],
          "Cached_votes_total": 0,
          "Captive": false,
          "Comments": [
            {
              "Id": 656,
              "Created_at": "2009-08-28T15:49:26+00:00",
              "Created_at_details": {
                "Date": "2009-08-28T00:00:00+02:00",
                "Day": 28,
                "Hour": 15,
                "Month": 8,
                "Week": 35,
                "Year": 2009
              },
              "User": {
                "Id": 1,
                "Suspended": false,
                "Icon": "https://static.inaturalist.org/attachments/users/icons/1/thumb.jpg?1475527316",
                "Icon_url": "https://static.inaturalist.org/attachments/users/icons/1/medium.jpg?1475527316",
                "Login": "kueda",
                "Name": "Ken-ichi Ueda"
              }
            },
            {
              "Id": 1800491,
              "Created_at": "2018-05-26T11:13:36+00:00",
              "Created_at_details": {
                "Date": "2018-05-26T00:00:00+02:00",
                "Day": 26,
                "Hour": 11,
                "Month": 5,
                "Week": 21,
                "Year": 2018
              },
              "User": {
                "Id": 275,
                "Suspended": false,
                "Login": "paco"
              }
            }
          ],
          "Comments_count": 2,
          "Created_at": "2009-08-28T09:15:29+02:00",
          "Created_at_details": {
            "Date": "2009-08-28T00:00:00+02:00",
            "Day": 28,
            "Hour": 9,
            "Month": 8,
            "Week": 35,
            "Year": 2009
          },
          "Created_time_zone": "Europe/Stockholm",
          "Description": "Hungry and lonely Baltic lesser black backed gull circling over central Stockholm",
          "Faves_count": 0,
          "Geojson": {
            "Type": "Point",
            "Coordinates": [
              18.0605792999,
              59.316532135
            ]
          },
          "Taxon_geoprivacy": "open",
          "Identifications_count": 0,
          "Identifications_most_agree": false,
          "Identifications_most_disagree": false,
          "Identifications_some_agree": false,
          "Location": "59.316532135,18.0605792999",
          "Mappable": true,
          "Non_owner_ids": [],
          "Num_identification_agreements": 0,
          "Num_identification_disagreements": 0,
          "Obscured": false,
          "Observed_on": "2009-08-28T00:00:00+02:00",
          "Observed_on_details": {
            "Date": "2009-08-28T00:00:00+02:00",
            "Day": 28,
            "Hour": 0,
            "Month": 8,
            "Week": 35,
            "Year": 2009
          },
          "Observed_on_string": "2009-08-28",
          "Observed_time_zone": "Europe/Stockholm",
          "Ofvs": [],
          "Photos": [],
          "Place_guess": "Stockholm",
          "Place_ids": [
            7599,
            9357,
            44985,
            59614,
            67952,
            80627,
            81490,
            96372,
            97391,
            108692,
            194475
          ],
          "Project_ids": [],
          "Project_ids_with_curator_id": [],
          "Project_ids_without_curator_id": [],
          "Quality_grade": "casual",
          "Reviewed_by": [
            275
          ],
          "Site_id": 1,
          "Sounds": [],
          "Species_guess": "Baltic Gull",
          "Tags": [],
          "Taxon": {
            "Ancestor_ids": [
              48460,
              1,
              2,
              355675,
              3,
              67561,
              4342,
              447155,
              4343,
              4381,
              515475
            ],
            "Ancestry": "48460/1/2/355675/3/67561/4342/447155/4343/4381",
            "Endemic": false,
            "Introduced": false,
            "Native": true,
            "Threatened": false,
            "Id": 515475,
            "Iconic_taxon_id": 3,
            "Iconic_taxon_name": "Aves",
            "Is_active": true,
            "Name": "Larus fuscus fuscus",
            "Preferred_common_name": "Baltic Gull",
            "Rank": "subspecies",
            "Rank_level": 5.0
          },
          "Time_zone_offset": "+01:00",
          "Updated_at": "2018-05-26T13:21:11+02:00",
          "Uri": "http://www.inaturalist.org/observations/3256",
          "User": {
            "Id": 275,
            "Suspended": false,
            "Login": "paco"
          }
        }
        """;

    [Fact]
    [Trait("Category", "AutomaticIntegrationTest")]
    public void Test_create_processed_iNaturalist_observation()
    {
        // Arrange        
        var observation = Newtonsoft.Json.JsonConvert.DeserializeObject<iNaturalistVerbatimObservation>(_verbatimObservationJson);
        var factory = ProcessFixture.GetiNaturalistFactory(initAreaHelper: true);

        // Act
        var processedObservation = factory.CreateProcessedObservation(observation, false);

        // Assert        
        processedObservation.Event.PlainStartDate.Should().Be("2009-08-28");
        processedObservation.Location.Municipality.Name.Should().Be("Stockholm");
        processedObservation.Location.CoordinateUncertaintyInMeters.Should().Be(5000);
        processedObservation.Location.DecimalLatitude.Should().Be(59.31653D);
        processedObservation.Location.DecimalLongitude.Should().Be(18.06058D);
        processedObservation.Occurrence.CatalogNumber.Should().Be("3256");
        processedObservation.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        processedObservation.Occurrence.OccurrenceId.Should().Be("https://www.inaturalist.org/observations/3256");
        processedObservation.Occurrence.OccurrenceRemarks.Should().Be("Hungry and lonely Baltic lesser black backed gull circling over central Stockholm");
        processedObservation.Occurrence.RecordedBy.Should().Be("paco");
        processedObservation.Taxon.Id.Should().Be(100071);
        processedObservation.Taxon.Attributes.RedlistCategory.Should().Be("VU");
        processedObservation.Taxon.Attributes.OrganismGroup.Should().Be("Fåglar");
        processedObservation.Taxon.VernacularName.Should().Be("östersjötrut");
        processedObservation.Taxon.ScientificName.Should().Be("Larus fuscus fuscus");
        processedObservation.DatasetName.Should().Be("iNaturalist");
        processedObservation.AccessRights.Id.Should().Be((int)AccessRightsId.FreeUsage);
        processedObservation.BasisOfRecord.Id.Should().Be((int)BasisOfRecordId.HumanObservation);
    }
}