using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateDefaultDataProvidersTool : TestBase
    {
        private List<DataProvider> GetDataProviders()
        {
            var dataProviders = new List<DataProvider>
            {
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.Artportalen,
                    Name = "Species Observation System (Artportalen)",
                    SwedishName = "Artportalen",
                    Organization = "SLU Swedish Species Information Centre (SLU Artdatabanken)",
                    SwedishOrganization = "SLU Artdatabanken",
                    Description =
                        "This is an open access database for sightings of plants, fungi and animals in Sweden. The system handles reports of geo-referenced species observations of all major organism groups from all environments, including terrestrial, freshwater and marine habitats. Data derives from citizen science, research, governmental monitoring and conservation administration. The database is administered by SLU Swedish Species Information Centre (SLU Artdatabanken).",
                    SwedishDescription =
                        "Artportalen administreras av SLU Artdatabanken och innehåller fynddata för alla slags organismer. Artportalen är ett webbaserat system för fynd av Sveriges växter, djur och svampar. Här samlas uppgifter om artfynd rapporterade från både allmänhet och föreningar såväl som länsstyrelser, kommuner, konsultfirmor och andra myndigheter och organisationer. De flesta data är publika, med undantag för information om särskilt känsliga arter som är skyddsklassade.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "stephen.coulson@slu.se",
                        FirstName = "Stephen",
                        LastName = "Coulson"
                    },
                    ContactEmail = "stephen.coulson@slu.se",
                    Url = "https://www.artportalen.se/",
                    Type = DataProviderType.ArtportalenObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    SupportIncrementalHarvest = true,
                    HarvestFailPreventProcessing = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ClamGateway,
                    Name = "Clam Gateway",
                    SwedishName = "Musselportalen",
                    Organization = "SLU Swedish Species Information Centre (SLU Artdatabanken)",
                    SwedishOrganization = "SLU Artdatabanken",
                    Description =
                        "The Clam Gateway is an independent site for collecting data about large freshwater clams in Sweden. Threatened and rare species are of special interest but information on more common clams is also important.",
                    SwedishDescription =
                        "Musselportalen är ett öppet rapporteringssystem för stormusslor som förekommer i svenska sjöar och vatten­drag. Syftet är att öka kunskapen om stormusslor och innehållet i portalen kan användas för att följa tillståndet för de hotade och sällsynta musselarter­na i landet.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "eddie.vonwachenfeldt@slu.se",
                        FirstName = "Eddie",
                        LastName = "Von Wachenfeldt"
                    },
                    ContactEmail = "eddie.vonwachenfeldt@slu.se",
                    Url = "http://musselportalen.se/",
                    Type = DataProviderType.ClamPortalObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.KUL,
                    Name = "The database for coastal fish (KUL)",
                    SwedishName = "Kustfiskdatabasen (KUL)",
                    Organization = "Department of Aquatic Resources, Institute of Coastal Research, SLU",
                    SwedishOrganization = "Kustlaboratoriet, Institutionen för akvatiska resurser, SLU",
                    Description = "KUL provides quality assured catch data of coastal fish.",
                    SwedishDescription =
                        "Kustlaboratoriets databas KUL innehåller kvalitetssäkrade fångstdata av fisk. I basen lagras också stickprovsinformation som könsfördelning och uppgifter om enskilda fiskar som längd, vikt, ålder och likande. Databasen kompletterar Kustlaboratoriets interna provfiskedatabas FiRRe som lagrar fångstuppgifter som insamlats sedan 1960-talet. Ett kontinuerligt arbete pågår med att föra över äldre data till KUL. Administreras av Kustlaboratoriet, Institutionen för akvatiska resurser, SLU.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "peter.ljungberg@slu.se",
                        FirstName = "Peter",
                        LastName = "Ljungberg"
                    },
                    ContactEmail = "peter.ljungberg@slu.se",
                    Url = "https://www.slu.se/kul/",
                    Type = DataProviderType.KULObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.MVM,
                    Name = "MVM",
                    SwedishName = "MVM",
                    Organization = "Environmental data MVM, SLU",
                    SwedishOrganization = "Miljödata MVM, SLU",
                    Description = "Contains fresh water and benthic species observation data. Administered by SLU.",
                    SwedishDescription =
                        "MVM står för Mark, Vatten och Miljödata. De delar av databasen som utgörs av artobservationer är tillgängliga via Svenska LifeWatch. Här finns data om till exempel växtplankton, bottenfauna, makrofyter och kiselalger.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "Lars.Sonesten@slu.se",
                        FirstName = "Lars",
                        LastName = "Sonesten"
                    },
                    ContactEmail = "Lars.Sonesten@slu.se",
                    Url = "http://miljodata.slu.se/mvm/",
                    Type = DataProviderType.MvmObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.NORS,
                    Name = "The National Register of Survey test-fishing (NORS)",
                    SwedishName = "Sjöprovfiskedatabasen NORS",
                    Organization = "Department of Aquatic Resources, SLU",
                    SwedishOrganization = "Institutionen för akvatiska resurser, SLU",
                    Description = "The database called NORS contains data from netfishing in Swedish lakes.",
                    SwedishDescription =
                        "Innehåller data från nätprovfiske i svenska sjöar. Administrerades tidigare av Fiskeriverket, numera av Institutionen för akvatiska resurser vid SLU.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "anders.kinnerback@slu.se",
                        FirstName = "Anders",
                        LastName = "Kinnerbäck"
                    },
                    ContactEmail = "anders.kinnerback@slu.se",
                    Url =
                        "http://www.slu.se/en/faculties/nl/about-the-faculty/departments/department-of-aquatic-resources/databases/national-register-of-survey-test-fishing-nors/",
                    Type = DataProviderType.NorsObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SERS,
                    Name = "The Database for electrofishing in streams (SERS)",
                    SwedishName = "Elfiskeregistret SERS",
                    Organization = "Department of Aquatic Resources, SLU",
                    SwedishOrganization = "Institutionen för akvatiska resurser, SLU",
                    Description =
                        "The database called SERS contains data from experimental fishing with electricity in Swedish rivers and streams.",
                    SwedishDescription =
                        "Innehåller data från provfiske med elaggregat i svenska vattendrag. Administrerades tidigare av Fiskeriverket, numera av Institutionen för akvatiska resurser vid SLU.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "berit.sers@slu.se",
                        FirstName = "Berit",
                        LastName = "Sers"
                    },
                    ContactEmail = "berit.sers@slu.se",
                    Url =
                        "http://www.slu.se/en/faculties/nl/about-the-faculty/departments/department-of-aquatic-resources/databases/database-for-testfishing-in-streams/",
                    Type = DataProviderType.SersObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.VirtualHerbarium,
                    Name = "Swedish Virtual Herbarium",
                    SwedishName = "Virtuella Herbariet",
                    Organization = "Umeå University",
                    SwedishOrganization = "Umeå universitet",
                    Description =
                        "The six largest herbaria in Sweden are gathered in the Virtual Herbarium. Herbarium (GB) - Gothenburg, Botanical Museum (LD) - Lund, Biological Museum (OHN) - Oskarshamn, Swedish Museum of Natural History (S) - Stockholm, Herbarium (UME) - Umeå, Museum of Evolution (UPS) - Uppsala.",
                    SwedishDescription =
                        "De sex största herbarierna i Sverige är samlade i Virtuella Herbariet. Herbarium (GB) - Göteborg, Botaniska Museet (LD) - Lund, Biologiska museet (OHN) - Oskarshamn, Naturhistoriska riksmuseet (S) - Stockholm, Herbarium (UME) - Umeå, Evolutionsmuseet (UPS) - Uppsala.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "mossnisse@hotmail.com",
                        FirstName = "Nils",
                        LastName = "Ericson"
                    },
                    ContactEmail = "mossnisse@hotmail.com",
                    Url = "http://herbarium.emg.umu.se",
                    Type = DataProviderType.VirtualHerbariumObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SHARK,
                    Name = "Shark SMHI",
                    SwedishName = "Shark SMHI",
                    Organization = "SMHI",
                    SwedishOrganization = "SMHI",
                    Description =
                        "(\"Svenskt HavsARKiv\" / \"Swedish Ocean Archive\") contains marine environmental monitoring data from the seas surrounding Sweden. Data from SHARK includes marine benthic fauna, marine benthic vegetation, plankton, and seal observations.",
                    SwedishDescription =
                        "Innehåller marina miljöövervakningsdata från de hav som omger Sverige. Innehåller bland annat data från provtagning av mjukbottenfauna (zoobenthos), bottenvegetation (phytobenthos), djur-, växt- och bakterieplankton, samt förekomst av vikaresäl, gråsäl och knubbsäl. SHARK administreras av SMHI.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "patrik.stromberg@smhi.se",
                        FirstName = "Patrik",
                        LastName = "Strömberg"
                    },
                    ContactEmail = "patrik.stromberg@smhi.se",
                    Url = "http://sharkweb.smhi.se/",
                    Type = DataProviderType.SharkObservations,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.BirdRingingCentre,
                    Name = "Bird ringing centre in Sweden via GBIF",
                    SwedishName = "Ringmärkningscentralen via GBIF",
                    Organization = "Swedish Museum of Natural History",
                    SwedishOrganization = "Naturhistoriska riksmuseet",
                    Description =
                        "This database contains information about bird ringing in Sweden. The database is maintained by The Swedish Natural History Museum. Swedish LifeWatch provides all georeferenced records from this data provider which relates to taxa in the Swedish Taxonomic Database (Dyntaxa) and that represents observations made in Sweden. The records are obtained via the web service at GBIF.org.",
                    SwedishDescription =
                        "Databasen innehåller uppgifter om fåglar ringmärkta i Sverige från 1911 och framåt (dock ej uppgifter om återfynd). Uppskattningsvis en tredjedel av Ringmärkningscentralens data är hittills digitaliserade.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    ContactEmail = "",
                    Url =
                        "http://www.nrm.se/forskningochsamlingar/miljoforskningochovervakning/ringmarkningscentralen.214.html",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.EntomologicalCollection,
                    Name = "Entomological Collections (NHRS) from GBIF",
                    SwedishName = "Entomologiska samlingarna (NHRS) via GBIF",
                    Organization = "Swedish Museum of Natural History",
                    SwedishOrganization = "Naturhistoriska riksmuseet",
                    Description =
                        "The dataset represents the digital holdings of insects, arachnids (mites and spiders) and myriapods (millipedes and centipedes) at the Swedish Museum of Natural History. These collections include over three million specimens, are international in scope, and have broad systematic and geographic coverage.",
                    SwedishDescription =
                        "Denna datamängd representerar digitala innehav av insekter, kvalster (kvalster och spindlar) och myriapods (tusenfotingar) vid Naturhistoriska riksmuseet. Dessa samlingar omfattar över tre miljoner exemplar, har internationella omfattning, och har en bred systematisk och geografisk täckning.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    ContactEmail = "-",
                    Url = "http://www.gbif.org/dataset/9940af5a-3271-4e6a-ad71-ced986b9a9a5",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nhrs-nrm",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.MalaiseTrap,
                    Name = "Swedish Malaise Trap Project (SMTP) from GBIF",
                    SwedishName = "Svenska Malaisefälle projektet (SMTP) via GBIF",
                    Organization = "Foundation Station Linné",
                    SwedishOrganization = "Stiftelsen Station Linné",
                    Description =
                        "The Swedish Malaise Trap Project (SMTP) aims to provide species determinations for the 80 million insect specimens obtained from Malaise traps sampling continuously over a three-year period (2003-2006). More than 500 undescribed species have been discovered in these samples, and new records have been verified for species not known previously from Sweden. Specimens in this collection are dry-mounted or stored in 96% ethanol. The collection is estimated to represent 50-60% of the Swedish insect fauna and is particularly rich in species of Diptera and Hymenoptera.",
                    SwedishDescription =
                        "Den svenska Malaisefälle projektet (SMTP) syftar till att artbestämma 80 miljoner insektsexemplar som erhållits från malaisefälleprovtagning kontinuerligt under en treårsperiod (2003-2006). Mer än 500 obeskrivna arter har upptäckts i dessa prover, och nytt rekord har nåtts för arter som inte tidigare varit kända i Sverige. Prover i denna samling är nålade eller lagras i 96% etanol. Insamlingen beräknas utgöra 50-60% av den svenska insektsfaunan och är särskilt rik på arter av Diptera och Hymenoptera.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    ContactEmail = "",
                    Url = "http://www.gbif.org/dataset/38c1351d-9cfe-42c0-97da-02d2c8be141c",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=smtp-nrm",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.Porpoises,
                    Name = "Porpoises (NRM) via GBIF",
                    SwedishName = "Tumlare (NRM) via GBIF",
                    Organization = "Swedish Museum of Natural History",
                    SwedishOrganization = "Naturhistoriska riksmuseet",
                    Description =
                        "This data set contains observations of dead or alive harbor porpoises made by the public mostly around the Swedish coast. Each observation of harbor porpoises is verified at the Swedish Museum of Natural History before it is approved and published on the web. Swedish LifeWatch provides all georeferenced records from this data provider which relates to taxa in the Swedish Taxonomic Database (Dyntaxa) and that represents observations made in Sweden. The records are obtained via the web service at GBIF.org.",
                    SwedishDescription =
                        "Databasen omfattar observationer av levande och döda tumlare som rapporterats till Naturhistoriska museet.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    ContactEmail = "",
                    Url = "http://www.gbif.org/dataset/6aa7c400-0c66-11dd-84d2-b8a03c50a862",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nrm-porpoises",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ButterflyMonitoring,
                    Name = "Swedish Butterfly Monitoring Scheme (SeBMS)",
                    SwedishName = "Svensk Dagfjärilsövervakning",
                    Organization = "Lund University",
                    SwedishOrganization = "Lunds universitet",
                    Description =
                        "The SeBMS is a standarised monitoring scheme for monitoring in Sweden. This dataset includes records from traditional fixed transect sites, often called ‘Pollard Walks’, and point site counts which cover a set area for a standardised visit time. The SeBMS is run by Lund University for the Swedish Environmental Protection Agency, in partnership with the Entomological Society of Sweden, the Swedish Environmental Protection Agency, Lund University, the Swedish University of Agricultural Sciences, the Swedish Transport Administration and the Swedish County Administration Boards.",
                    SwedishDescription = "-",
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    ContactEmail = "",
                    Url = "https://www.dagfjarilar.lu.se/",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=lu_sebms",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SharkZooplankton,
                    Name = "SHARK - National zooplankton monitoring in Sweden since 1979",
                    Organization = "Swedish Meterological and Hydrological Institute (SMHI)",
                    SwedishOrganization = "SMHI, Sveriges meteorologiska och hydrologiska institut",
                    Description =
                        "Zooplankton have been monitored in Sweden since 1979. The national monitoring program was initiated by the Swedish Environmental Protection Agency and is now financed by the Swedish Agency for Marine and Water Management. Monitoring is performed by Stockholm University, Umeå University and Swedish Meteorological and Hydrological Institute and Gothenburg University. Data are stored in the Swedish Ocean Archive database (SHARK), by the Swedish Meteorological and Hydrological Institute. Data are collected and analyzed according to the HELCOM COMBINE Manual - Part C Annex C7 Mesozooplankton (https://helcom.fi/media/publications/Guidelines-for-monitoring-of-mesozooplankton.pdf) or similar methods. \"Mesozooplankton constitute an important part of zooplankton in the pelagic food webs, since these are the organisms representing the link between primary producers (phytoplankton) and higher trophic levels (zooplanktivorous fish and invertebrates)...The sampling of mesozooplankton serves...to describe the species composition and the spatial distribution of mesozooplankton abundance and biomass.\" (HELCOM COMBINE Manual https://helcom.fi/media/publications/Guidelines-for-monitoring-of-mesozooplankton.pdf). Information about the program and the methods are also available in Swedish at https://www.havochvatten.se/hav/vagledning--lagar/vagledningar/ovriga-vagledningar/undersokningstyper-for-miljoovervakning/undersokningstyper/djurplankton-trend--och-omradesovervakning.html",
                    SwedishDescription = "-",
                    ContactPerson = new ContactPerson
                    {
                        Email = "shark@smhi.se",
                        FirstName = "Lisa",
                        LastName = "Sundqvist"
                    },
                    ContactEmail = "shark@smhi.se",
                    Url = "http://sharkdata.se/",
                    DownloadUrl = "",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.FishData,
                    Name = "Fishdata2 (FD2)",
                    SwedishName = "Fiskdata2 (FD2)",
                    Organization = "Department of Aquatic Resources (SLU Aqua)",
                    SwedishOrganization = "Havsfiskelaboratoriet, Institutionen för akvatiska resurser, SLU",
                    Description = "The database Fishdata2 at the institute of Marine Research, Department of Aquatic Resources at SLU, stores quality assured data from environmental investigations and fishery investigations of marine fish and shellfish. It is the national provider of data for the European Data collection Framework (DCF), feeding data to eg. ICES international assessment work of fish and shellfish.",
                    SwedishDescription = "Databasen Fiskdata 2 innehåller kvalitetssäkrad fångstdata och individdata om fisk och skaldjur insamlade under miljöövervakningsundersökningar eller fiskeriundersökningar. Fiskdata2 är den nationella plattformen för hantering av data som samlas in under EUs datainsamlingsförordning (DCF) och vidarebefodrar data till bl.a. Internationella havsforskningsrådets (ICES) arbete med beståndsuppskattning av fisk och skaldjur. Databasen administreras av Havsfiskelaboratoriet, Institutionen för akvatiska resurser, SLU.",
                    ContactPerson = new ContactPerson
                    {
                        Email = "Malin.Werner@slu.se",
                        FirstName = "Malin",
                        LastName = "Werner"
                    },
                    ContactEmail = "Malin.Werner@slu.se",
                    Url = "https://www.slu.se/forskning/framgangsrik-forskning/forskningsinfrastruktur/databaser-och-biobanker/Databasen-for-fiske-i-havet/",
                    Type = DataProviderType.FishDataObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ForestInventory,
                    Name = "Swedish National Forest Inventory: Presence-absence Vegetation data",
                    SwedishName = "Riksskogstaxeringen",
                    Organization = "Swedish National Forest Inventory, SLU",
                    SwedishOrganization = "Riksskogstaxeringen, SLU",
                    Description = "The Swedish National Forest Inventory (NFI) is a sample based field inventory performed by the Swedish University of Agricultural Sciences (SLU). The annual inventory is undertaken on sample plots and this forms the basis for a large number of estimates. Data is collected over the whole of Sweden and statistics is published for all land use classes except urban land and sea and fresh water. The data collection is focused on forest land and particularly productive forest landSince 2003 areas within formally protected land are included and since 2016 non –coniferiuos alpine forest was included. The sample plots are circular and are, for practical reasons, grouped into clusters (tracts). The tracts are square or rectangular in shape and are of different dimensions in different parts of the country. The density between tracts and the number of sample plots per tract also varies for different parts of the country, with a higher sample intensity in southern Sweden. Two thirds of the sample consists of permanent tracts and one third are temporary. The permanent tracts are revisited every 5 th year and the temporary tracts are only visited once. The real coordinates of the permanent sample plots are protected. The coordinate in this dataset is randomely obfuscated with 200-1000 m.",
                    SwedishDescription = "-",
                    ContactPerson = new ContactPerson
                    {
                        Email = "riksskogstaxeringen@slu.se",
                        FirstName = "Jonas",
                        LastName = "Dahlgren"
                    },
                    ContactEmail = "riksskogstaxeringen@slu.se",
                    Url = "https://www.slu.se/riksskogstaxeringen",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    DataQualityIsApproved = true,
                    HarvestSchedule = "* * * * *"
                }

            };

            return dataProviders;
        }

        [Fact]
        public void CreateDefaultDataProvidersJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\DefaultDataProviders.json";
            var dataProviderByIdentifier = GetDataProviders()
                .ToDictionary(dataProvider => dataProvider.Identifier, dataProvider => dataProvider);

            // Select order and set Id.
            var dataProviders = new List<DataProvider>
            {
                dataProviderByIdentifier[DataProviderIdentifiers.Artportalen],
                dataProviderByIdentifier[DataProviderIdentifiers.ClamGateway],
                dataProviderByIdentifier[DataProviderIdentifiers.KUL],
                dataProviderByIdentifier[DataProviderIdentifiers.MVM],
                dataProviderByIdentifier[DataProviderIdentifiers.NORS],
                dataProviderByIdentifier[DataProviderIdentifiers.SERS],
                dataProviderByIdentifier[DataProviderIdentifiers.VirtualHerbarium],
                dataProviderByIdentifier[DataProviderIdentifiers.SHARK],
                dataProviderByIdentifier[DataProviderIdentifiers.BirdRingingCentre],
                dataProviderByIdentifier[DataProviderIdentifiers.EntomologicalCollection],
                dataProviderByIdentifier[DataProviderIdentifiers.MalaiseTrap],
                dataProviderByIdentifier[DataProviderIdentifiers.Porpoises],
                dataProviderByIdentifier[DataProviderIdentifiers.ButterflyMonitoring],
                dataProviderByIdentifier[DataProviderIdentifiers.SharkZooplankton],
                dataProviderByIdentifier[DataProviderIdentifiers.FishData],
                dataProviderByIdentifier[DataProviderIdentifiers.ForestInventory]
            };

            for (var i = 0; i < dataProviders.Count; i++)
            {
                dataProviders[i].Id = i + 1;
            }

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var strJson = JsonConvert.SerializeObject(dataProviders, Formatting.Indented);
            File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }
    }
}