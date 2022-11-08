using SOS.DataStewardship.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace SOS.DataStewardship.Api.Models.SampleData
{
    public static class DataStewardshipArtportalenSampleData
    {
        public static Dataset DatasetBats = new Dataset {            
            Identifier = "ArtportalenDataHost - Dataset Bats", // Ändra till Id-nummer. Enligt DPS:en ska det vara ett id-nummer från informationsägarens metadatakatalog. Om det är LST som är informationsägare så bör de ha datamängden registrerad i sin metadatakatalog, med ett id där.
            Metadatalanguage = "Swedish",
            Language = "Swedish",
            AccessRights = AccessRights.Publik,
            Purpose = Purpose.NationellMiljöövervakning,
            Assigner = new Organisation
            {
                OrganisationID = "2021001975",
                OrganisationCode = "Naturvårdsverket"
            },
            // Creator = Utförare av datainsamling.
            Creator = new List<Organisation> 
            { 
                new Organisation
                {
                    OrganisationID = "OrganisationId-unknown",
                    OrganisationCode = "SLU Artdatabanken"
                }
            },
            // Finns inte alltid i AP, behöver skapas/hämtasandra DV informationskällor?
            OwnerinstitutionCode = new Organisation
            {
                OrganisationID = "OrganisationId-unknown",
                OrganisationCode = "Länsstyrelsen Jönköping"
            },
            Publisher = new Organisation
            {
                OrganisationID = "OrganisationId-unknown",
                OrganisationCode = "SLU Artdatabanken"
            },            
            DataStewardship = "Datavärdskap Naturdata: Arter", // Artdatabanken har ett datavärdskap, inte ett separat per datamängd.
            StartDate = new DateTime(2011, 1, 1),
            EndDate = null,            
            Description = "Inventeringar av fladdermöss som görs inom det gemensamma delprogrammet för fladdermöss, dvs inom regional miljöövervakning, biogeografisk uppföljning och områdesvis uppföljning (uppföljning av skyddade områden).\r\n\r\nDet finns totalt tre projekt på Artportalen för det gemensamma delprogrammet och i detta projekt rapporteras data från den biogeografiska uppföljningen. Syftet med övervakningen är att följa upp hur antal och utbredning av olika arter förändras över tid. Övervakningen ger viktig information till bland annat EU-rapporteringar, rödlistningsarbetet och kan även användas i uppföljning av miljömålen och som underlag i ärendehandläggning. Den biogeografiska uppföljningen omfattar för närvarande några av de mest artrika fladdermuslokalerna i de olika biogeografiska regionerna i Sverige. Dessa inventeras vartannat år. Ett fåartsområde för fransfladdermus i norra Sverige samt några övervintringslokaler ingår också i övervakningen.",
            Title = "Fladdermöss",
            Spatial = "Sverige",
            ProjectID = "Artportalen ProjectId:3606",
            ProjectCode = "Fladdermöss - gemensamt delprogram (biogeografisk uppföljning)",
            Methodology = new List<Methodology>
            {
                new Methodology
                {
                    MethodologyDescription = "Methodology description?", // finns sällan i projektbeskrivning i AP, behöver hämtas från andra DV informationskällor
                    MethodologyLink = "https://www.naturvardsverket.se/upload/stod-i-miljoarbetet/vagledning/miljoovervakning/handledning/metoder/undersokningstyper/landskap/fladdermus-artkartering-2017-06-05.pdf",
                    MethodologyName = "Undersökningstyp fladdermöss - artkartering",
                    SpeciesList = "Species list?" // artlistan behöver skapas, alternativt "all species occurring in Sweden"
                }
            },
            Events = new List<string>
            {
                "urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00"
            }
        };

        public static EventModel EventBats1 = new EventModel
        {
            EventID = "urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00", // $"{Location.LocationId}#{Event.VerbatimEventDate}"
            Dataset = new EventDataset
            {
                Identifier = "ArtportalenDataHost - Dataset Bats",
                Title = "Fladdermöss"
            },
            EventStartDate = new DateTime(2012, 3, 6, 8, 0, 0),
            EventEndDate = new DateTime(2012, 3, 6, 13, 0, 0),
            LocationProtected = false,
            //SurveyLocation = "Karlsborgs fästning, Vg", // Location.Locality
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
            SamplingProtocol = "Undersökningstyp fladdermöss - artkartering",
            NoObservations = NoObservations.Falskt,
            Occurrences = new List<string>
            {
                "urn:lsid:artportalen.se:sighting:74542663",
                "urn:lsid:artportalen.se:sighting:74542662",
                "urn:lsid:artportalen.se:sighting:74542657",
                //"urn:lsid:artportalen.se:sighting:74542667",
                //"urn:lsid:artportalen.se:sighting:74542666",
                //"urn:lsid:artportalen.se:sighting:74542660",
                //"urn:lsid:artportalen.se:sighting:74542659",
                //"urn:lsid:artportalen.se:sighting:74542658",
                //"urn:lsid:artportalen.se:sighting:74542664",
                //"urn:lsid:artportalen.se:sighting:74542665",
                //"urn:lsid:artportalen.se:sighting:74542661"
            },
            RecorderCode = new List<string>
            {
                "Ingemar Ahlén, Jens Rydell" // Observatörer
            },
            RecorderOrganisation = new List<Organisation>
            {
                new Organisation
                {
                    OrganisationID = "urn:lsid:artportalen.se:organisation:232", 
                    OrganisationCode = "Länsstyrelsen Jönköping" // detta heter "inventeringsorganisation" i DPS och avser organisationen som inventeraren representerar dvs Lst Jönk
                }
            },
            Weather = null, // i AP kan man registrera Fältbesök vilket skulle motsvarar events och där man registrera väder, men jag vet inte om det används av DV projekten
            AssociatedMedia = null, // i AP kan man registrera Fältbesök vilket skulle motsvarar events, men jag vet inte om man kan lägga upp media för Fältsbesök, och om Fältbsök används av DV projekt
            EventRemarks = null, // i Fältbesök kan man göra anteckningar men jag vet inte om de används av DV projekt
            EventType = null, // Saknas i Artportalen
            ParentEventID = null, // Saknas i Artportalen
        };

        public static OccurrenceModel EventBats1Occurrence1 = new OccurrenceModel()
        {
            OccurrenceID = "urn:lsid:artportalen.se:sighting:74542663",
            Event = "urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00",
            ObservationTime = new DateTime(2012, 3, 6, 8, 0, 0), // Ej obligatorisk, men kan sättas om StartDate och EndDate har samma värde. EventDate ger tiden för själva besöket (t.ex. från 8:10 på morgonen till 11:36 på förmiddagen. Occ under den tiden gjordes en obs av spillkråka kl 10:54. Det är den tiden (10:54) som är observationstidpunkt.
            BasisOfRecord = BasisOfRecord.MänskligObservation,
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, // Ovaliderad  // för värdelistan väntar vi på vad arbetsgruppen för verifieringsmodulen skulle ha som slutgiltig förslag på kategorier - vi bör fråga de om att få den listan         
            ObservationCertainty = 200m,
            ObservationPoint = new PointGeoShape(new GeoCoordinate(58.52845, 14.52667)),
            ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { 58.52845, 14.52667 }
            },            
            OccurrenceRemarks = "Östra utfallsgången",
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable
            {
                Activity = Activity.PåÖvervintringsplats,
                LifeStage = null,
                Sex = null
            },
            Quantity = 2m,
            QuantityVariable = QuantityVariable.AntalIndivider,
            Unit = null, // Enhet för Quantity
            Taxon = new TaxonModel
            {
                ScientificName = "Barbastella barbastellus",
                TaxonID = "100015",
                TaxonRank = "species",
                VerbatimName = "barbastell",
                VerbatimTaxonID = "100015",
                VernacularName = "barbastell"
            },
            AssociatedMedia = null
        };

        public static OccurrenceModel EventBats1Occurrence2 = new OccurrenceModel()
        {
            OccurrenceID = "urn:lsid:artportalen.se:sighting:74542662",
            Event = "urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00",            
            ObservationTime = new DateTime(2012, 3, 6, 8, 0, 0), 
            BasisOfRecord = BasisOfRecord.MänskligObservation,            
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, 
            ObservationCertainty = 200m,
            ObservationPoint = new PointGeoShape(new GeoCoordinate(58.52845, 14.52667)),
            ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { 58.52845, 14.52667 }
            },            
            OccurrenceRemarks = "Östra utfallsgången",
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable
            {
                Activity = Activity.PåÖvervintringsplats,
                LifeStage = null,
                Sex = null
            },
            Quantity = 1m,
            QuantityVariable = QuantityVariable.AntalIndivider,            
            Unit = null, 
            Taxon = new TaxonModel
            {
                ScientificName = "Eptesicus nilssonii",
                TaxonID = "205998",
                TaxonRank = "species",
                VerbatimName = "nordfladdermus",
                VerbatimTaxonID = "205998",
                VernacularName = "nordfladdermus"
            },
            AssociatedMedia = null
        };

        public static OccurrenceModel EventBats1Occurrence3 = new OccurrenceModel()
        {
            OccurrenceID = "urn:lsid:artportalen.se:sighting:74542657",
            Event = "urn:lsid:artportalen.se:site:3775204#2012-03-06T08:00:00+01:00/2012-03-06T13:00:00+01:00",
            ObservationTime = new DateTime(2012, 3, 6, 8, 0, 0), // StartDate och EndDate verkar saknas i OccurrenceModel?
            BasisOfRecord = BasisOfRecord.MänskligObservation,
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, // Godkänd baserat på observatörens uppgifter
            ObservationCertainty = 200m,
            ObservationPoint = new PointGeoShape(new GeoCoordinate(58.52845, 14.52667)),
            ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { 58.52845, 14.52667 }
            },
            OccurrenceRemarks = "Västra blindgången",
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable
            {
                Activity = Activity.PåÖvervintringsplats,
                LifeStage = null,
                Sex = null
            },
            Quantity = 5m,
            QuantityVariable = QuantityVariable.AntalIndivider,
            Unit = null, // Vad är unit kopplat till för variabel?
            Taxon = new TaxonModel
            {
                ScientificName = "Eptesicus nilssonii",
                TaxonID = "205998",
                TaxonRank = "species",
                VerbatimName = "nordfladdermus",
                VerbatimTaxonID = "205998",
                VernacularName = "nordfladdermus"
            },
            AssociatedMedia = null
        };

        public static EventModel EventBats2 = new EventModel
        {
            EventID = "urn:lsid:artportalen.se:site:5084342#2016-07-14T22:00:00+02:00/2016-07-15T05:00:00+02:00", // $"{Location.LocationId}#{Event.VerbatimEventDate}"
            Dataset = new EventDataset
            {
                Identifier = "ArtportalenDataHost - Project Bats",
                Title = "Fladdermöss - gemensamt delprogram (biogeografisk uppföljning)"
            },
            EventStartDate = new DateTime(2016, 7, 14),
            EventEndDate = new DateTime(2016, 7, 15),
            LocationProtected = false,
            //SurveyLocation = "Allarps bjär, Sk", // Location.Locality
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
            SamplingProtocol = "Undersökningstyp fladdermöss - artkartering",
            NoObservations = NoObservations.Falskt,
            Occurrences = new List<string>
            {
                "urn:lsid:artportalen.se:sighting:85188138",
                "urn:lsid:artportalen.se:sighting:85188141",
                //"urn:lsid:artportalen.se:sighting:85188142",
                //"urn:lsid:artportalen.se:sighting:85188140",
                //"urn:lsid:artportalen.se:sighting:85188139",
            },
            RecorderCode = new List<string>
            {
                "Rune Gerell" // Är detta korrekt?
            },
            RecorderOrganisation = new List<Organisation>
            {
                new Organisation
                {
                    OrganisationID = "urn:lsid:artportalen.se:organisation:232", // Är detta korrekt?
                    OrganisationCode = "Länsstyrelsen Jönköping" // Är detta korrekt?
                }
            },
            Weather = null, // Saknas i Artportalen?
            AssociatedMedia = null, // Saknas i Artportalen?
            EventRemarks = null, // Saknas i Artportalen?
            EventType = null, // Saknas i Artportalen?
            ParentEventID = null, // Saknas i Artportalen?
        };

        public static OccurrenceModel EventBats2Occurrence1 = new OccurrenceModel()
        {
            OccurrenceID = "urn:lsid:artportalen.se:sighting:85188138",
            Event = "urn:lsid:artportalen.se:site:5084342#2016-07-14T22:00:00+02:00/2016-07-15T05:00:00+02:00",
            ObservationTime = new DateTime(2016, 7, 14), // StartDate och EndDate verkar saknas i OccurrenceModel?
            BasisOfRecord = BasisOfRecord.MänskligObservation,
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, // Godkänd baserat på observatörens uppgifter
            ObservationCertainty = 5m,
            ObservationPoint = new PointGeoShape(new GeoCoordinate(58.52845, 14.52667)),
            ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { 58.52845, 14.52667 }
            },
            OccurrenceRemarks = null,
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable
            {
                Activity = null,
                LifeStage = null,
                Sex = null
            },
            Quantity = 4m,
            QuantityVariable = QuantityVariable.AntalIndivider,
            Unit = null, // Vad är unit kopplat till för variabel?
            Taxon = new TaxonModel
            {
                ScientificName = "Myotis",
                TaxonID = "1001620",
                TaxonRank = "genus",
                VerbatimName = null,
                VerbatimTaxonID = "1001620",
                VernacularName = null
            },
            AssociatedMedia = null
        };

        public static OccurrenceModel EventBats2Occurrence2 = new OccurrenceModel()
        {
            OccurrenceID = "urn:lsid:artportalen.se:sighting:85188141",
            Event = "urn:lsid:artportalen.se:site:5084342#2016-07-14T22:00:00+02:00/2016-07-15T05:00:00+02:00",
            ObservationTime = new DateTime(2016, 7, 14), // StartDate och EndDate verkar saknas i OccurrenceModel?
            BasisOfRecord = BasisOfRecord.MänskligObservation,
            IdentificationVerificationStatus = IdentificationVerificationStatus.VärdelistaSaknas, // Godkänd baserat på observatörens uppgifter
            ObservationCertainty = 5m,
            ObservationPointTest = new GeometryObject
            {
                Type = "point",
                Coordinates = new double[] { 58.52845, 14.52667 }
            },            
            OccurrenceRemarks = null,
            OccurrenceStatus = OccurrenceStatus.Observerad,
            Organism = new OrganismVariable
            {
                Activity = null,
                LifeStage = null,
                Sex = null
            },
            Quantity = 1m,
            QuantityVariable = QuantityVariable.AntalIndivider,
            Unit = null, // Vad är unit kopplat till för variabel?
            Taxon = new TaxonModel
            {
                ScientificName = "Myotis",
                TaxonID = "1001620",
                TaxonRank = "genus",
                VerbatimName = null,
                VerbatimTaxonID = "1001620",
                VernacularName = null
            },
            AssociatedMedia = null
        };

    }
}
