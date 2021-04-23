using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Nest;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Factories
{
    /// <summary>
    ///     This class creates EML files.
    /// </summary>
    public static class DwCArchiveEmlFileFactory
    {
        private static void SetContact(XElement dataset, DataProvider dataProvider)
        {
            var contact = dataset.Element("contact");//metadataProvider
            contact.XPathSelectElement("individualName/givenName").SetValue(dataProvider.ContactPerson?.FirstName ?? "N/A");
            contact.XPathSelectElement("individualName/surName").SetValue(dataProvider.ContactPerson?.LastName ?? "N/A");
            contact.XPathSelectElement("organizationName").SetValue(dataProvider.Organizations?.Translate("en-GB") ?? "N/A");
            contact.XPathSelectElement("electronicMailAddress").SetValue(dataProvider.ContactPerson?.Email ?? "N/A");
        }

        private static void SetGeographicCoverage(XElement dataset, GeoBounds geoBounds)
        {
            if (geoBounds == null)
            {
                return;
            }

            var coverage = dataset.Element("coverage");
            var geographicCoverage = coverage.Element("geographicCoverage");
            geographicCoverage.XPathSelectElement("geographicDescription").SetValue("All data is collected within Sweden.");
            geographicCoverage.XPathSelectElement("boundingCoordinates/westBoundingCoordinate").SetValue(geoBounds.TopLeft.Lon ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/eastBoundingCoordinate").SetValue(geoBounds.BottomRight.Lon ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/northBoundingCoordinate").SetValue(geoBounds.TopLeft.Lat ?? 0.0);
            geographicCoverage.XPathSelectElement("boundingCoordinates/southBoundingCoordinate").SetValue(geoBounds.BottomRight.Lat ?? 0.0);
        }
       
        private static void SetTemporalCoverage(XElement dataset, DateTime? firstSpotted, DateTime? lastSpotted)
        {
            if (!(firstSpotted.HasValue || lastSpotted.HasValue))
            {
                return;
            }

            var temporalCoverage = dataset.Element("temporalCoverage");
            var rangeOfDates = temporalCoverage.Element("rangeOfDates");
           
            if (firstSpotted.HasValue)
            {
                rangeOfDates.XPathSelectElement("beginDate/calendarDate").SetValue(firstSpotted);
            }

            if (lastSpotted.HasValue)
            {
                rangeOfDates.XPathSelectElement("endDate/calendarDate").SetValue(lastSpotted);
            }
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
        public static async Task CreateEmlXmlFileAsync(Stream outStream, DataProvider dataProvider)
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
            SetContact(dataset, dataProvider);
            

            dataset.XPathSelectElement("distribution/online/url").SetValue(dataProvider.Url ?? "N/A");

            var emlString = xDoc.ToString();
            var emlBytes = Encoding.UTF8.GetBytes(emlString);
            await outStream.WriteAsync(emlBytes, 0, emlBytes.Length);
        }

        /// <summary>
        /// Update meta data that changes when processing is done
        /// </summary>
        /// <param name="eml"></param>
        /// <param name="firstSpotted"></param>
        /// <param name="lastSpotted"></param>
        /// <param name="geographicCoverage"></param>
        /// <returns></returns>
        public static void UpdateDynamicMetaData(XDocument eml, DateTime? firstSpotted, DateTime? lastSpotted, GeoBounds geographicCoverage)
        {
            var dataset = eml.Root.Element("dataset");
            SetTemporalCoverage(dataset, firstSpotted, lastSpotted);
            SetGeographicCoverage(dataset, geographicCoverage);
        }
    }
}