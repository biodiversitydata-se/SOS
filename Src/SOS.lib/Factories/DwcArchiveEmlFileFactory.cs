using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        private static XNamespace dcNs = "http://purl.org/dc/terms/";
        private static XNamespace gbifNs = "http://gbif.org/";
        private static XNamespace sosNs = "https://sos-search.artdata.slu.se/";

        private static XElement GetElement(XElement dataset, XName name)
        {
            var element = dataset.Element(name);

            if (element != null)
            {
                return element;
            }
           
            element = new XElement(name);
            dataset.Add(element);

            return element;
        }

        public static async Task<long> GetEmlSizeWithoutPubDateAsync(Stream stream)
        {
            try
            {
                var xDoc = XDocument.Load(stream);
                var dataset = xDoc.Root.Element("dataset");
                var pubDateElement = GetElement(dataset, "pubDate");
                pubDateElement?.Remove();
                var bytes = await xDoc.ToBytesAsync();
                return bytes.LongLength;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void SetContact(XElement dataset, DataProvider dataProvider)
        {
            var contact = GetElement(dataset, "contact");
            var individualName = GetElement(contact, "individualName");
            GetElement(individualName, "givenName").SetValue(dataProvider.ContactPerson?.FirstName ?? "N/A");
            GetElement(individualName, "surName").SetValue(dataProvider.ContactPerson?.LastName ?? "N/A");
            GetElement(contact, "organizationName").SetValue(dataProvider.Organizations?.Translate("en-GB") ?? "N/A");
            GetElement(contact, "electronicMailAddress").SetValue(dataProvider.ContactPerson?.Email ?? "N/A");
        }

        private static void SetGeographicCoverage(XElement dataset, GeoBounds geoBounds)
        {
            if (geoBounds == null)
            {
                return;
            }

            var coverage = GetElement(dataset, "coverage");
            var geographicCoverage = GetElement(coverage, "geographicCoverage");
            GetElement(geographicCoverage, "geographicDescription").SetValue("All data is collected within Sweden.");
            var boundingCoordinates = GetElement(geographicCoverage, "boundingCoordinates");
            GetElement(boundingCoordinates,"westBoundingCoordinate").SetValue(geoBounds.TopLeft.Lon ?? 0.0);
            GetElement(boundingCoordinates, "eastBoundingCoordinate").SetValue(geoBounds.BottomRight.Lon ?? 0.0);
            GetElement(boundingCoordinates,"northBoundingCoordinate").SetValue(geoBounds.TopLeft.Lat ?? 0.0);
            GetElement(boundingCoordinates,"southBoundingCoordinate").SetValue(geoBounds.BottomRight.Lat ?? 0.0);
        }
       
        private static void SetTemporalCoverage(XElement dataset, DateTime? firstSpotted, DateTime? lastSpotted)
        {
            if (!(firstSpotted.HasValue || lastSpotted.HasValue))
            {
                return;
            }
            
            var temporalCoverage = GetElement(GetElement(dataset, "coverage"), "temporalCoverage");
            var rangeOfDates = GetElement(temporalCoverage, "rangeOfDates");
           
            if (firstSpotted.HasValue)
            {
                var beginDate = GetElement(rangeOfDates, "beginDate");
                GetElement(beginDate, "calendarDate").SetValue(firstSpotted);
            }

            if (lastSpotted.HasValue)
            {
                var endDate = GetElement(rangeOfDates, "endDate");
                GetElement(endDate, "calendarDate").SetValue(lastSpotted);
            }
        }

        
        /// <summary>
        ///     Set pubDate to today's date
        /// </summary>
        /// <param name="dataset"></param>
        private static void SetPubDateToCurrentDate(XElement dataset)
        {
            GetElement(dataset, "pubDate").SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public static void SetPubDateToCurrentDate(XDocument xDoc)
        {
            var dataset = xDoc.Root.Element("dataset");
            GetElement(dataset, "pubDate").SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Creates an EML XML file.
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public static async Task<XDocument> CreateEmlXmlFileAsync(DataProvider dataProvider)
        {
            return await Task.Run(() =>
            {
                if (dataProvider == null)
                {
                    return null;
                }
                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var emlTemplatePath = Path.Combine(assemblyPath, @"Resources/DarwinCore/eml.xml");
                var xDoc = XDocument.Load(emlTemplatePath);
                var dataset = xDoc.Root.Element("dataset");

                SetPubDateToCurrentDate(dataset);
                GetElement(dataset, "identifier").SetValue(new Guid().ToString());
                GetElement(dataset, "alternateIdentifier").SetValue($"urn:lsid:artdata.slu.se:SOS:{dataProvider.Identifier}");

                GetElement(dataset, "title").SetValue(dataProvider.Names.Translate("en-GB") ?? "N/A");
                GetElement(GetElement(dataset, "abstract"), "para").SetValue(dataProvider.Descriptions?.Translate("en-GB") ?? "N/A");
                SetContact(dataset, dataProvider);

                GetElement(dataset, sosNs + "resourceHomepage").SetValue(dataProvider.Url ?? "N/A");

                return xDoc;
            });
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
            SetPubDateToCurrentDate(dataset);
        }
    }
}