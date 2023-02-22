using SOS.DataStewardship.Api.Models.Enums;

namespace SOS.DataStewardship.Api.Models.SampleData
{
    public static class DataStewardshipSampleData
    {

        public static Dataset Dataset1 = new Dataset
            {
                Identifier = "identifier",
                Metadatalanguage = "English",
                Language = "Swedish",
                AccessRights = AccessRights.Publik,
                Purpose = Purpose.NationellMiljöövervakning,
                Assigner = new Organisation
                {
                    OrganisationID = "2021001975",
                    OrganisationCode = "Naturvårdsverket"
                },
                Creator = new List<Organisation>
                {
                    new Organisation
                    {
                        OrganisationID = "OrganisationId",
                        OrganisationCode = "OrganisationCode"
                    }
                },                
                OwnerinstitutionCode = new Organisation
                {
                    OrganisationID = "OrganisationId",
                    OrganisationCode = "OrganisationCode"
                },
                Publisher = new Organisation
                {
                    OrganisationID = "OrganisationId",
                    OrganisationCode = "OrganisationCode"
                },
                DataStewardship = "Datavärdskap Naturdata: Fåglar och fjärilar",
                StartDate = new DateTime(2010, 1, 1),
                EndDate = new DateTime(2019, 12, 31),
                Description = "description",
                Title = "Svensk Fågeltaxering: Standardrutterna",
                Spatial = "Sverige",
                Project = new List<Project> { 
                    new Project
                    {
                        ProjectID = "ProjectId",
                        ProjectCode = "ProjectCode",
                        ProjectType = ProjectType.Delprogram
                    }
                },
                Methodology = new List<Methodology>
                {
                    new Methodology()
                    {
                        MethodologyDescription = "MethodologyDescription",
                        MethodologyLink = "http://example.com/aeiou",
                        MethodologyName = "MethodologyName",
                        SpeciesList = "SpeciesList"
                    }
                },
                EventIds = new List<string>
                {
                    "d9baea4e-2436-4481-accb-7c2fe835039e",
                    "60152666-8c2c-4d33-a5c8-da1dda106c5d",
                    "c4eaa558-83cc-4b94-9aff-1aefdc204794",
                    "273998e3-3138-41eb-b740-28ee53f7e344"
                }
            };


        public static EventModel Event1 = new EventModel
        {
            AssociatedMedia = new List<AssociatedMedia>()
            {
                new AssociatedMedia()
                {
                    AssociatedMediaLink = "AssociatedMediaLink",
                    AssociatedMediaName = "AssociatedMediaName",
                    AssociatedMediaType = AssociatedMediaType.Bild,
                    License = "License",
                    RightsHolder = "RightsHolder"
                }
            },
            Dataset = new EventDataset()
            {
                Identifier = "Identifier",
                Title = "Title"
            },
            EventStartDate = new DateTime(2010, 1, 1),
            EventEndDate = new DateTime(2014, 12, 31),
            EventID = "EventId-1",
            EventRemarks = "EventRemarks",
            EventType = "EventType",
            LocationProtected = false,
            NoObservations = false,
            OccurrenceIds = new List<string>()
            {
                "Occurrence-1", "Occurrence-2"
            },
            ParentEventID = "ParentEventID",
            RecorderCode = new List<string>
            {
                "RecorderCode-1"
            },
            RecorderOrganisation = new List<Organisation>()
            {
                new Organisation
                {
                    OrganisationID = "OrganisationId",
                    OrganisationCode = "OrganisationCode"
                }
            },
            SamplingProtocol = "SamplingProtocol",
            //SurveyLocation = "SurveyLocation",
            SurveyLocation = new Location
            {
                Locality = "Karlsborgs fästning, Vg",
                County = County.VästraGötalandsLän,
                Municipality = Municipality.Karlsborg,
                LocationType = Location.LocationTypeEnum.Punkt,
                Emplacement = new PointGeoShape(new GeoCoordinate(58.01221, 14.96721)),
                EmplacementTest = new GeometryObject
                {
                    Type = "point",
                    Coordinates = new double[] { 58.01221, 14.96721 }
                }
            },
            Weather = new WeatherVariable()
            {
                AirTemperature  = new WeatherMeasuring
                {
                    WeatherMeasure = 25.4d,
                    Unit = Unit.GraderCelsius
                }
            }
        };


        public static OccurrenceModel Occurrence1 = new OccurrenceModel()
        {
            AssociatedMedia = new List<AssociatedMedia>()
            {
                new AssociatedMedia()
                {
                    AssociatedMediaLink = "AssociatedMediaLink",
                    AssociatedMediaName = "AssociatedMediaName",
                    AssociatedMediaType = AssociatedMediaType.Bild,
                    License = "License",
                    RightsHolder = "RightsHolder"
                }
            },
            BasisOfRecord = BasisOfRecord.MänskligObservation,
            EventID = "EventId-1",
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, 
            ObservationCertainty = 25d, 
            ObservationPoint = null,
            ObservationTime = new DateTime(2013,10,22),
            OccurrenceID = "OccurrenceId-1",
            OccurrenceRemarks = "OccurrenceRemarks",
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable()
            {
                Activity = Activity.Födosökande,
                LifeStage = LifeStage.ImagoAdult,
                Sex = Sex.Hane
            },
            Quantity = 1d,
            QuantityVariable = QuantityVariable.AntalIndivider,
            Unit = Unit.Styck,
            Taxon = new TaxonModel()
            {
                ScientificName = "ScientificName",
                TaxonID = "4000107",
                TaxonRank = "Species",
                VerbatimName = "VerbatimName",
                VerbatimTaxonID = "VerbatimTaxonID",
                VernacularName = "VernacularName"
            }
        };

    }
}
