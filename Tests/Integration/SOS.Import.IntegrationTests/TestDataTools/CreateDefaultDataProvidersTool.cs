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
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Species Observation System (Artportalen)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Artportalen"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value ="This is an open access database for sightings of plants, fungi and animals in Sweden. The system handles reports of geo-referenced species observations of all major organism groups from all environments, including terrestrial, freshwater and marine habitats. Data derives from citizen science, research, governmental monitoring and conservation administration. The database is administered by SLU Swedish Species Information Centre (SLU Artdatabanken)."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Artportalen administreras av SLU Artdatabanken och innehåller fynddata för alla slags organismer. Artportalen är ett webbaserat system för fynd av Sveriges växter, djur och svampar. Här samlas uppgifter om artfynd rapporterade från både allmänhet och föreningar såväl som länsstyrelser, kommuner, konsultfirmor och andra myndigheter och organisationer. De flesta data är publika, med undantag för information om särskilt känsliga arter som är skyddsklassade."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "SLU Swedish Species Information Centre (SLU Artdatabanken)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SLU Artdatabanken"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "stephen.coulson@slu.se",
                        FirstName = "Stephen",
                        LastName = "Coulson"
                    },
                    Url = "https://www.artportalen.se/",
                    Type = DataProviderType.ArtportalenObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    SupportIncrementalHarvest = true,
                    HarvestFailPreventProcessing = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ClamGateway,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Clam Gateway"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Musselportalen"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The Clam Gateway is an independent site for collecting data about large freshwater clams in Sweden. Threatened and rare species are of special interest but information on more common clams is also important."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Musselportalen är ett öppet rapporteringssystem för stormusslor som förekommer i svenska sjöar och vatten­drag. Syftet är att öka kunskapen om stormusslor och innehållet i portalen kan användas för att följa tillståndet för de hotade och sällsynta musselarter­na i landet."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "SLU Swedish Species Information Centre (SLU Artdatabanken)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SLU Artdatabanken"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "eddie.vonwachenfeldt@slu.se",
                        FirstName = "Eddie",
                        LastName = "Von Wachenfeldt"
                    },
                    Url = "http://musselportalen.se/",
                    Type = DataProviderType.ClamPortalObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.KUL,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The database for coastal fish (KUL)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Kustfiskdatabasen (KUL)"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "KUL provides quality assured catch data of coastal fish."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Kustlaboratoriets databas KUL innehåller kvalitetssäkrade fångstdata av fisk. I basen lagras också stickprovsinformation som könsfördelning och uppgifter om enskilda fiskar som längd, vikt, ålder och likande. Databasen kompletterar Kustlaboratoriets interna provfiskedatabas FiRRe som lagrar fångstuppgifter som insamlats sedan 1960-talet. Ett kontinuerligt arbete pågår med att föra över äldre data till KUL. Administreras av Kustlaboratoriet, Institutionen för akvatiska resurser, SLU."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Department of Aquatic Resources, Institute of Coastal Research, SLU"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Kustlaboratoriet, Institutionen för akvatiska resurser, SLU"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "peter.ljungberg@slu.se",
                        FirstName = "Peter",
                        LastName = "Ljungberg"
                    },
                    Url = "https://www.slu.se/kul/",
                    Type = DataProviderType.KULObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.MVM,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "MVM"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "MVM"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Contains fresh water and benthic species observation data. Administered by SLU."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "MVM står för Mark, Vatten och Miljödata. De delar av databasen som utgörs av artobservationer är tillgängliga via Svenska LifeWatch. Här finns data om till exempel växtplankton, bottenfauna, makrofyter och kiselalger."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Environmental data MVM, SLU"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Miljödata MVM, SLU"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "Lars.Sonesten@slu.se",
                        FirstName = "Lars",
                        LastName = "Sonesten"
                    },
                    Url = "http://miljodata.slu.se/mvm/",
                    Type = DataProviderType.MvmObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.NORS,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The National Register of Survey test-fishing (NORS)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Sjöprovfiskedatabasen NORS"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The database called NORS contains data from netfishing in Swedish lakes."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Innehåller data från nätprovfiske i svenska sjöar. Administrerades tidigare av Fiskeriverket, numera av Institutionen för akvatiska resurser vid SLU."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Department of Aquatic Resources, SLU"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Institutionen för akvatiska resurser, SLU"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "anders.kinnerback@slu.se",
                        FirstName = "Anders",
                        LastName = "Kinnerbäck"
                    },
                    Url =
                        "http://www.slu.se/en/faculties/nl/about-the-faculty/departments/department-of-aquatic-resources/databases/national-register-of-survey-test-fishing-nors/",
                    Type = DataProviderType.NorsObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SERS,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The Database for electrofishing in streams (SERS)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Elfiskeregistret SERS"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The database called SERS contains data from experimental fishing with electricity in Swedish rivers and streams."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Innehåller data från provfiske med elaggregat i svenska vattendrag. Administrerades tidigare av Fiskeriverket, numera av Institutionen för akvatiska resurser vid SLU."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Department of Aquatic Resources, SLU"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Institutionen för akvatiska resurser, SLU"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "berit.sers@slu.se",
                        FirstName = "Berit",
                        LastName = "Sers"
                    },
                    Url =
                        "http://www.slu.se/en/faculties/nl/about-the-faculty/departments/department-of-aquatic-resources/databases/database-for-testfishing-in-streams/",
                    Type = DataProviderType.SersObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.VirtualHerbarium,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Virtual Herbarium"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Virtuella Herbariet"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The six largest herbaria in Sweden are gathered in the Virtual Herbarium. Herbarium (GB) - Gothenburg, Botanical Museum (LD) - Lund, Biological Museum (OHN) - Oskarshamn, Swedish Museum of Natural History (S) - Stockholm, Herbarium (UME) - Umeå, Museum of Evolution (UPS) - Uppsala."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "De sex största herbarierna i Sverige är samlade i Virtuella Herbariet. Herbarium (GB) - Göteborg, Botaniska Museet (LD) - Lund, Biologiska museet (OHN) - Oskarshamn, Naturhistoriska riksmuseet (S) - Stockholm, Herbarium (UME) - Umeå, Evolutionsmuseet (UPS) - Uppsala."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Umeå University"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Umeå universitet"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "mossnisse@hotmail.com",
                        FirstName = "Nils",
                        LastName = "Ericson"
                    },
                    Url = "http://herbarium.emg.umu.se",
                    Type = DataProviderType.VirtualHerbariumObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SHARK,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Shark SMHI"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Shark SMHI"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "(\"Svenskt HavsARKiv\" / \"Swedish Ocean Archive\") contains marine environmental monitoring data from the seas surrounding Sweden. Data from SHARK includes marine benthic fauna, marine benthic vegetation, plankton, and seal observations."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Innehåller marina miljöövervakningsdata från de hav som omger Sverige. Innehåller bland annat data från provtagning av mjukbottenfauna (zoobenthos), bottenvegetation (phytobenthos), djur-, växt- och bakterieplankton, samt förekomst av vikaresäl, gråsäl och knubbsäl. SHARK administreras av SMHI."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "SMHI"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SMHI"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "patrik.stromberg@smhi.se",
                        FirstName = "Patrik",
                        LastName = "Strömberg"
                    },
                    Url = "http://sharkweb.smhi.se/",
                    Type = DataProviderType.SharkObservations,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.BirdRingingCentre,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Bird ringing centre in Sweden via GBIF"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Ringmärkningscentralen via GBIF"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "This database contains information about bird ringing in Sweden. The database is maintained by The Swedish Natural History Museum. Swedish LifeWatch provides all georeferenced records from this data provider which relates to taxa in the Swedish Taxonomic Database (Dyntaxa) and that represents observations made in Sweden. The records are obtained via the web service at GBIF.org."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Databasen innehåller uppgifter om fåglar ringmärkta i Sverige från 1911 och framåt (dock ej uppgifter om återfynd). Uppskattningsvis en tredjedel av Ringmärkningscentralens data är hittills digitaliserade."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Museum of Natural History"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Naturhistoriska riksmuseet"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    Url =
                        "http://www.nrm.se/forskningochsamlingar/miljoforskningochovervakning/ringmarkningscentralen.214.html",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nrm-ringedbirds",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.EntomologicalCollection,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Entomological Collections (NHRS) from GBIF"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Entomologiska samlingarna (NHRS) via GBIF"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The dataset represents the digital holdings of insects, arachnids (mites and spiders) and myriapods (millipedes and centipedes) at the Swedish Museum of Natural History. These collections include over three million specimens, are international in scope, and have broad systematic and geographic coverage."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Denna datamängd representerar digitala innehav av insekter, kvalster (kvalster och spindlar) och myriapods (tusenfotingar) vid Naturhistoriska riksmuseet. Dessa samlingar omfattar över tre miljoner exemplar, har internationella omfattning, och har en bred systematisk och geografisk täckning."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Museum of Natural History"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Naturhistoriska riksmuseet"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    Url = "http://www.gbif.org/dataset/9940af5a-3271-4e6a-ad71-ced986b9a9a5",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nhrs-nrm",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.MalaiseTrap,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Malaise Trap Project (SMTP) from GBIF"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Svenska Malaisefälle projektet (SMTP) via GBIF"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The Swedish Malaise Trap Project (SMTP) aims to provide species determinations for the 80 million insect specimens obtained from Malaise traps sampling continuously over a three-year period (2003-2006). More than 500 undescribed species have been discovered in these samples, and new records have been verified for species not known previously from Sweden. Specimens in this collection are dry-mounted or stored in 96% ethanol. The collection is estimated to represent 50-60% of the Swedish insect fauna and is particularly rich in species of Diptera and Hymenoptera."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Den svenska Malaisefälle projektet (SMTP) syftar till att artbestämma 80 miljoner insektsexemplar som erhållits från malaisefälleprovtagning kontinuerligt under en treårsperiod (2003-2006). Mer än 500 obeskrivna arter har upptäckts i dessa prover, och nytt rekord har nåtts för arter som inte tidigare varit kända i Sverige. Prover i denna samling är nålade eller lagras i 96% etanol. Insamlingen beräknas utgöra 50-60% av den svenska insektsfaunan och är särskilt rik på arter av Diptera och Hymenoptera."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Foundation Station Linné"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Stiftelsen Station Linné"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    Url = "http://www.gbif.org/dataset/38c1351d-9cfe-42c0-97da-02d2c8be141c",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=smtp-nrm",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.Porpoises,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Porpoises (NRM) via GBIF"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Tumlare (NRM) via GBIF"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "This data set contains observations of dead or alive harbor porpoises made by the public mostly around the Swedish coast. Each observation of harbor porpoises is verified at the Swedish Museum of Natural History before it is approved and published on the web. Swedish LifeWatch provides all georeferenced records from this data provider which relates to taxa in the Swedish Taxonomic Database (Dyntaxa) and that represents observations made in Sweden. The records are obtained via the web service at GBIF.org."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Databasen omfattar observationer av levande och döda tumlare som rapporterats till Naturhistoriska museet."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Museum of Natural History"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Naturhistoriska riksmuseet"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    Url = "http://www.gbif.org/dataset/6aa7c400-0c66-11dd-84d2-b8a03c50a862",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=nrm-porpoises",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ButterflyMonitoring,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Butterfly Monitoring Scheme (SeBMS)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Svensk Dagfjärilsövervakning"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Lund University"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Lunds universitet"
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The SeBMS is a standarised monitoring scheme for monitoring in Sweden. This dataset includes records from traditional fixed transect sites, often called ‘Pollard Walks’, and point site counts which cover a set area for a standardised visit time. The SeBMS is run by Lund University for the Swedish Environmental Protection Agency, in partnership with the Entomological Society of Sweden, the Swedish Environmental Protection Agency, Lund University, the Swedish University of Agricultural Sciences, the Swedish Transport Administration and the Swedish County Administration Boards."
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "",
                        FirstName = "",
                        LastName = ""
                    },
                    Url = "https://www.dagfjarilar.lu.se/",
                    DownloadUrl = "http://www.gbif.se/ipt/archive.do?r=lu_sebms",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.SharkZooplankton,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "SHARK - National zooplankton monitoring in Sweden since 1979"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Zooplankton have been monitored in Sweden since 1979. The national monitoring program was initiated by the Swedish Environmental Protection Agency and is now financed by the Swedish Agency for Marine and Water Management. Monitoring is performed by Stockholm University, Umeå University and Swedish Meteorological and Hydrological Institute and Gothenburg University. Data are stored in the Swedish Ocean Archive database (SHARK), by the Swedish Meteorological and Hydrological Institute. Data are collected and analyzed according to the HELCOM COMBINE Manual - Part C Annex C7 Mesozooplankton (https://helcom.fi/media/publications/Guidelines-for-monitoring-of-mesozooplankton.pdf) or similar methods. \"Mesozooplankton constitute an important part of zooplankton in the pelagic food webs, since these are the organisms representing the link between primary producers (phytoplankton) and higher trophic levels (zooplanktivorous fish and invertebrates)...The sampling of mesozooplankton serves...to describe the species composition and the spatial distribution of mesozooplankton abundance and biomass.\" (HELCOM COMBINE Manual https://helcom.fi/media/publications/Guidelines-for-monitoring-of-mesozooplankton.pdf). Information about the program and the methods are also available in Swedish at https://www.havochvatten.se/hav/vagledning--lagar/vagledningar/ovriga-vagledningar/undersokningstyper-for-miljoovervakning/undersokningstyper/djurplankton-trend--och-omradesovervakning.html"
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish Meterological and Hydrological Institute (SMHI)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SMHI, Sveriges meteorologiska och hydrologiska institut"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "shark@smhi.se",
                        FirstName = "Lisa",
                        LastName = "Sundqvist"
                    },
                    Url = "http://sharkdata.se/",
                    DownloadUrl = "",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.FishData,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Fishdata2 (FD2)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Fiskdata2 (FD2)"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The database Fishdata2 at the institute of Marine Research, Department of Aquatic Resources at SLU, stores quality assured data from environmental investigations and fishery investigations of marine fish and shellfish. It is the national provider of data for the European Data collection Framework (DCF), feeding data to eg. ICES international assessment work of fish and shellfish."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Databasen Fiskdata 2 innehåller kvalitetssäkrad fångstdata och individdata om fisk och skaldjur insamlade under miljöövervakningsundersökningar eller fiskeriundersökningar. Fiskdata2 är den nationella plattformen för hantering av data som samlas in under EUs datainsamlingsförordning (DCF) och vidarebefodrar data till bl.a. Internationella havsforskningsrådets (ICES) arbete med beståndsuppskattning av fisk och skaldjur. Databasen administreras av Havsfiskelaboratoriet, Institutionen för akvatiska resurser, SLU."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Department of Aquatic Resources (SLU Aqua)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Havsfiskelaboratoriet, Institutionen för akvatiska resurser, SLU"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "Malin.Werner@slu.se",
                        FirstName = "Malin",
                        LastName = "Werner"
                    },
                    Url = "https://www.slu.se/forskning/framgangsrik-forskning/forskningsinfrastruktur/databaser-och-biobanker/Databasen-for-fiske-i-havet/",
                    Type = DataProviderType.FishDataObservations,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ForestInventory,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish National Forest Inventory: Presence-absence Vegetation data"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SLU Riksskogstaxeringen: Förekomst- och icke förekomst. Vegetationsdata"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "The Swedish National Forest Inventory (NFI) is a sample based field inventory performed by the Swedish University of Agricultural Sciences (SLU). The annual inventory is undertaken on sample plots and this forms the basis for official national statistics. Data is collected over the whole of Sweden and statistics is published for all land use classes except urban land and sea and fresh water. The data collection is focused on forest land. Since 2003 areas within formally protected land are included and since 2016 non –coniferiuos alpine forest is included. The sample plots are circular and are clustered so called tracts. The tracts are square or rectangular in shape and are of different dimensions in different parts of the country. The density between tracts and the number of sample plots per tract also varies for different parts of the country, with a higher sample intensity in southern Sweden compared to the north. Two thirds of the sample consists of permanent tracts and one third are temporary. The permanent tracts are revisited every 5 th year and the temporary tracts are only visited once. The real coordinates of the permanent sample plots are protected. The coordinate in this dataset is randomely obfuscated with 200-1000 m."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Riksskogstaxeringen (RT) är en stickprovsinventering som utförs av Sveriges lantbruksuniversitet (SLU). Den årliga inventeringen utförs på provytor som utgör underlag för officiell statistik. Data samlas in över hela Sverige och statistic produceras för alla ägoslag undantaget bebygd mark, sjöar och hav. Datainsamlingen fokuseras på  skogsmark. Sedan 2003 inventeras även provytor inom formellt skyddade områden, och från 2016 även provytor I fjällen, dock ej på kalfjäll.  Provytorna är cirkulära och klustrade I så kallade trakter. Trakterna är till formen rektangulära med olika sidlängd I olika delar av Sverige. Även antalet provytor per trakt varierar över landet. Generellt ökar stickprovstätheten från norr till söder Två tredjedelar av trakterna är permanenta (återinventeras var 5:e år) och en tredjedel är tillfälliga (inventeras bara en gang) De exakta positionerna för de permanenta provytorna är sekretesskyddade varför positionerna på observationerna i detta dataset är slumpvis förskjutna med 200-1000 m"
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Swedish National Forest Inventory, SLU"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SLU Riksskogstaxeringen"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "riksskogstaxeringen@slu.se",
                        FirstName = "Jonas",
                        LastName = "Dahlgren"
                    },
                    Url = "https://www.slu.se/riksskogstaxeringen",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *"
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.ObservationDatabase,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Observation database of Redlisted species"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Observationsdatabasen"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "A non-public database used mainly for conservation planning. Access to data requires permission. Administered by SLU Swedish Species Information Centre."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Observationsdatabasen administreras av SLU Artdatabanken och innehåller både äldre och nyare fynd av främst rödlistade arter. Fynduppgifterna är inte allmänt tillgängliga på grund av att de ofta lämnats till ArtDatabanken under premisserna att de inte får offentliggöras eller spridas vidare annat än till naturvårdsrelaterade ändamål. Arbete pågår med att överföra material från Observationsdatabasen till Artportalen."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "SLU Swedish Species Information Centre (SLU Artdatabanken)"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "SLU Artdatabanken"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "Jan.Edelsjo@slu.se",
                        FirstName = "Jan",
                        LastName = "Edelsjö"
                    },
                    Url = "http://www.artdatabanken.se/verksamhet-och-uppdrag/arter-kunskapsinsamling/fynd-av-arter/soeka-och-goera-uttag-av-fynddata/?st=uttag+av+fynddata",
                    Type = DataProviderType.ObservationDatabase,
                    IsActive = true,
                    IncludeInScheduledHarvest = true,
                    IncludeInSearchByDefault = true,
                    HarvestSchedule = "* * * * *",
                    SupportProtectedHarvest = true
                },
                new DataProvider
                {
                    Identifier = DataProviderIdentifiers.Biologg,
                    Names = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Biologg"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Biologg"
                        }
                    },
                    Descriptions = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Biologg is a mobile game that logs observations of species in the wild. In the game you can collect points, level up in different species groups and do challenges. The data collected will later be used for research and nature conservation."
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Biologg är ett mobilspel som går ut på att logga observationer av arter ute i naturen. I spelet kan man samla poäng, levla up i olika artgrupper och göra utmaningar. Den data som samlas in kommer senare att kunna användas för forskning och naturvård."
                        }
                    },
                    Organizations = new []
                    {
                        new VocabularyValueTranslation
                        {
                            CultureCode = "en-GB",
                            Value = "Overstellar Solutions AB"
                        },
                        new VocabularyValueTranslation
                        {
                            CultureCode = "sv-SE",
                            Value = "Overstellar Solutions AB"
                        }
                    },
                    ContactPerson = new ContactPerson
                    {
                        Email = "info@biologg.se",
                        FirstName = "Daniel",
                        LastName = "Eriksson"
                    },
                    Url = "https://www.biologg.se/",
                    Type = DataProviderType.DwcA,
                    IsActive = false,
                    IncludeInScheduledHarvest = false,
                    IncludeInSearchByDefault = false,
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
                dataProviderByIdentifier[DataProviderIdentifiers.ForestInventory],
                dataProviderByIdentifier[DataProviderIdentifiers.ObservationDatabase],
                dataProviderByIdentifier[DataProviderIdentifiers.Biologg]
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