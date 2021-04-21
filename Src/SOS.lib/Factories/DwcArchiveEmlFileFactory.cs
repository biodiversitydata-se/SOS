using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace SOS.Lib.Factories
{
    /// <summary>
    ///     This class creates EML files.
    /// </summary>
    public static class DwCArchiveEmlFileFactory
    {
        private static async Task CreateEmlXmlFileByEmlMetadata(Stream outStream, BsonDocument emlMetadata)
        {
            var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson }; // key part
            string strJson = emlMetadata.ToJson(jsonWriterSettings);
            XDocument xDoc = JsonConvert.DeserializeXNode(strJson);
            SetPubDateToCurrentDate(xDoc.Root.Element("dataset"));
            var emlString = xDoc.ToString();
            var emlBytes = Encoding.UTF8.GetBytes(emlString);
            await outStream.WriteAsync(emlBytes, 0, emlBytes.Length);
        }

        private static async Task CreateEmlXmlFileByTemplate(Stream outStream, DataProvider dataProvider)
        {
            if (dataProvider == null)
            {
                return;
            }
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var emlTemplatePath = Path.Combine(assemblyPath, @"Resources\DarwinCore\eml.xml");
            var xDoc = XDocument.Load(emlTemplatePath);
            var dataset = xDoc.Root.Element("dataset");
           
            SetPubDateToCurrentDate(dataset);
            dataset.XPathSelectElement("title").SetValue(dataProvider.Names.Translate("en-GB") ?? "N/A");
            dataset.XPathSelectElement("abstract/para").SetValue(dataProvider.Descriptions?.Translate("en-GB") ?? "N/A");
            dataset.XPathSelectElement("resourceType").SetValue(dataProvider.ResourceType ?? "N/A");
            SetContact(dataset, dataProvider);
            SetGeographicCoverage(dataset, dataProvider);
            dataset.XPathSelectElement("metadataLanguage").SetValue(dataProvider.MetadataLanguage ?? "N/A");
            dataset.XPathSelectElement("language").SetValue(dataProvider.Language ?? "N/A");
            dataset.XPathSelectElement("licenseName").SetValue(dataProvider.LicenseName ?? "N/A");
            dataset.XPathSelectElement("distribution/online/url").SetValue(dataProvider.Url ?? "N/A");
            
            var emlString = xDoc.ToString();
            var emlBytes = Encoding.UTF8.GetBytes(emlString);
            await outStream.WriteAsync(emlBytes, 0, emlBytes.Length);
        }


        private static void SetContact(XElement dataset, DataProvider dataProvider)
        {
            var contact = dataset.Element("contact");//metadataProvider
            contact.XPathSelectElement("individualName/givenName").SetValue(dataProvider.ContactPerson?.FirstName ?? "N/A");
            contact.XPathSelectElement("individualName/surName").SetValue(dataProvider.ContactPerson?.LastName ?? "N/A");
            contact.XPathSelectElement("organizationName").SetValue(dataProvider.Organizations?.Translate("en-GB") ?? "N/A");
            contact.XPathSelectElement("electronicMailAddress").SetValue(dataProvider.ContactPerson?.Email ?? "N/A");
        }

        private static void SetGeographicCoverage(XElement dataset, DataProvider dataProvider)
        {
            var coverage = dataset.Element("coverage");
            var geographicCoverage = coverage.Element("geographicCoverage");
            geographicCoverage.XPathSelectElement("geographicDescription").SetValue("All data is collected within Sweden.");
            geographicCoverage.XPathSelectElement("boundingCoordinates/westBoundingCoordinate").SetValue(dataProvider.BoundingBox?.Left ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/eastBoundingCoordinate").SetValue(dataProvider.BoundingBox?.Right ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/northBoundingCoordinate").SetValue(dataProvider.BoundingBox?.Top ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/southBoundingCoordinate").SetValue(dataProvider.BoundingBox?.Bottom ?? 0.0);
        }

        private static void SetTemporalCoverage(XElement dataset, DataProvider dataProvider)
        {
            if (!(dataProvider.StartDate.HasValue || dataProvider.EndDate.HasValue))
            {
                return;
            }

            var temporalCoverage = dataset.Element("temporalCoverage");
            var rangeOfDates = temporalCoverage.Element("rangeOfDates");
           
            if (dataProvider.StartDate.HasValue)
            {
                rangeOfDates.XPathSelectElement("beginDate/calendarDate").SetValue(dataProvider.StartDate);
            }

            if (dataProvider.EndDate.HasValue)
            {
                rangeOfDates.XPathSelectElement("endDate/calendarDate").SetValue(dataProvider.EndDate);
            }
        }

        private static void SetTaxonomicCoverage(XElement dataset, DataProvider dataProvider)
        {
            var taxonomicCoverage = dataset.Element("taxonomicCoverage");
            taxonomicCoverage.XPathSelectElement("generalTaxonomicCoverage").SetValue(dataProvider.TaxonomicCoverage ?? "N/A");
        }

        
        /// <summary>
        ///     Set pubDate to today's date
        /// </summary>
        /// <param name="dataset"></param>
        private static void SetPubDateToCurrentDate(XElement dataset)
        {
            var pubDate = dataset.Element("pubDate");
            if (pubDate == null)
            { 
                pubDate = new XElement("pubDate");
                dataset.Add(pubDate);
            }
            pubDate.SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        ///     Creates an EML XML file.
        /// </summary>
        public static async Task CreateEmlXmlFileAsync(Stream outstream, DataProvider dataProvider)
        {
            if (dataProvider.EmlMetadata == null)
            {
                await CreateEmlXmlFileByTemplate(outstream, dataProvider);
            }
            else
            {
                await CreateEmlXmlFileByEmlMetadata(outstream, dataProvider.EmlMetadata);
            }
        }
    }
}