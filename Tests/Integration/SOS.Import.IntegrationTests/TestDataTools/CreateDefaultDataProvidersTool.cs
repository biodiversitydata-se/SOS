using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.Artportalen;
using SOS.Import.Repositories.Destination.FieldMappings;
using SOS.Import.Repositories.Destination.Taxon;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.TestHelpers.IO;
using SOS.TestHelpers.JsonConverters;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateDefaultDataProvidersTool : TestBase
    {
        [Fact]
        public void CreateDefaultDataProvidersJsonFile()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            List<DataProvider> dataProviders = new List<DataProvider>
            {
                new DataProvider
                {
                    Id = 1,
                    Name = "Artportalen",
                    Identifier = "Artportalen",
                    Organization = "SLU Swedish Species Information Centre (SLU Artdatabanken)",
                    Description = "This is an open access database for sightings of plants, fungi and animals in Sweden. The system handles reports of geo-referenced species observations of all major organism groups from all environments, including terrestrial, freshwater and marine habitats. Data derives from citizen science, research, governmental monitoring and conservation administration. The database is administered by SLU Swedish Species Information Centre (SLU Artdatabanken).",
                    ContactEmail = "stephen.coulson@slu.se",
                    ContactPerson = "Stephen Coulson",
                    Url = "https://www.artportalen.se/",
                    DataType = DataSet.ArtportalenObservations
                },
                new DataProvider
                {
                    Id = 2,
                    Name = "Clam Gateway",
                    Identifier = "ClamGateway",
                    Organization = "SLU Swedish Species Information Centre (SLU Artdatabanken)",
                    Description = "The Clam Gateway is an independent site for collecting data about large freshwater clams in Sweden. Threatened and rare species are of special interest but information on more common clams is also important.",
                    ContactEmail = "eddie.vonwachenfeldt@slu.se",
                    ContactPerson = "Eddie Von Wachenfeldt",
                    Url = "http://musselportalen.se/",
                    DataType = DataSet.ClamPortalObservations
                },
                new DataProvider
                {
                    Id = 3,
                    Name = "The database for coastal fish (KUL)",
                    Identifier = "KUL",
                    Organization = "Department of Aquatic Resources, Institute of Coastal Research, SLU",
                    Description = "	KUL provides quality assured catch data of coastal fish.",
                    ContactEmail = "peter.ljungberg@slu.se",
                    ContactPerson = "Peter Ljungberg",
                    Url = "https://www.slu.se/kul/",
                    DataType = DataSet.KULObservations
                },
                new DataProvider
                {
                    Id = 4,
                    Name = "Swedish Malaise Trap Project (SMTP) from GBIF",
                    Identifier = "MalaiseTrap",
                    Organization = "Foundation Station Linné",
                    Description = "The Swedish Malaise Trap Project (SMTP) aims to provide species determinations for the 80 million insect specimens obtained from Malaise traps sampling continuously over a three-year period (2003-2006). More than 500 undescribed species have been discovered in these samples, and new records have been verified for species not known previously from Sweden. Specimens in this collection are dry-mounted or stored in 96% ethanol. The collection is estimated to represent 50-60% of the Swedish insect fauna and is particularly rich in species of Diptera and Hymenoptera.",
                    ContactEmail = "",
                    ContactPerson = "",
                    Url = "http://www.gbif.org/dataset/38c1351d-9cfe-42c0-97da-02d2c8be141c",
                    DataType = DataSet.Dwc
                },
                new DataProvider
                {
                    Id = 5,
                    Name = "Entomological Collections (NHRS) from GBIF",
                    Identifier = "EntomologicalCollection",
                    Organization = "Swedish Museum of Natural History",
                    Description = "The dataset represents the digital holdings of insects, arachnids (mites and spiders) and myriapods (millipedes and centipedes) at the Swedish Museum of Natural History. These collections include over three million specimens, are international in scope, and have broad systematic and geographic coverage.",
                    ContactEmail = "",
                    ContactPerson = "",
                    Url = "http://www.gbif.org/dataset/9940af5a-3271-4e6a-ad71-ced986b9a9a5",
                    DataType = DataSet.Dwc
                },
                new DataProvider
                {
                    Id = 6,
                    Name = "Bird ringing centre in Sweden via GBIF",
                    Identifier = "BirdRinging",
                    Organization = "Swedish Museum of Natural History",
                    Description = "This database contains information about bird ringing in Sweden. The database is maintained by The Swedish Natural History Museum. Swedish LifeWatch provides all georeferenced records from this data provider which relates to taxa in the Swedish Taxonomic Database (Dyntaxa) and that represents observations made in Sweden. The records are obtained via the web service at GBIF.org.",
                    ContactEmail = "",
                    ContactPerson = "",
                    Url = "http://www.nrm.se/forskningochsamlingar/miljoforskningochovervakning/ringmarkningscentralen.214.html",
                    DataType = DataSet.Dwc
                },
                new DataProvider
                {
                    Id = 7,
                    Name = "Swedish Butterfly Monitoring Scheme (SeBMS)",
                    Identifier = "ButterflyMonitoring",
                    Organization = "Lund University",
                    Description = "The SeBMS is a standarised monitoring scheme for monitoring in Sweden. This dataset includes records from traditional fixed transect sites, often called ‘Pollard Walks’, and point site counts which cover a set area for a standardised visit time. The SeBMS is run by Lund University for the Swedish Environmental Protection Agency, in partnership with the Entomological Society of Sweden, the Swedish Environmental Protection Agency, Lund University, the Swedish University of Agricultural Sciences, the Swedish Transport Administration and the Swedish County Administration Boards.",
                    ContactEmail = "",
                    ContactPerson = "",
                    Url = "http://www.gbif.se/ipt/resource?r=lu_sebms",
                    DataType = DataSet.Dwc
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            const string filePath = @"c:\temp\DefaultDataProviders.json";
            var strJson = JsonConvert.SerializeObject(dataProviders, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, strJson, Encoding.UTF8);
        }
    }
}