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

namespace SOS.Export.IO.DwcArchive
{
    /// <summary>
    ///     This class creates EML files.
    /// </summary>
    public static class DwCArchiveEmlFileFactory
    {
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
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var emlTemplatePath = Path.Combine(assemblyPath, @"Resources\DarwinCore\eml.xml");
            var xDoc = XDocument.Load(emlTemplatePath);
            var dataset = xDoc.Root.Element("dataset");
            SetPubDateToCurrentDate(dataset);
            SetTitle(dataset, dataProvider.Names.Translate("en-GB"));
            SetCreator(dataset, dataProvider.ContactPerson, dataProvider.Organizations.Translate("en-GB"));
            SetUrl(dataset, dataProvider.Url);
            SetAbstract(dataset, dataProvider.Descriptions.Translate("en-GB"));
            var emlString = xDoc.ToString();
            var emlBytes = Encoding.UTF8.GetBytes(emlString);
            await outStream.WriteAsync(emlBytes, 0, emlBytes.Length);
        }

        private static void SetUrl(XElement dataset, string url)
        {
            dataset.XPathSelectElement("distribution/online/url").SetValue(url);
        }

        private static void SetAbstract(XElement dataset, string description)
        {
            dataset.XPathSelectElement("abstract/para").SetValue(description);
        }

        private static void SetCreator(XElement dataset, ContactPerson contactPerson, string organization)
        {
            var creator = dataset.Element("creator");
            creator.XPathSelectElement("individualName/givenName").SetValue(contactPerson.FirstName);
            creator.XPathSelectElement("individualName/surName").SetValue(contactPerson.LastName);
            creator.XPathSelectElement("organizationName").SetValue(organization);
            creator.XPathSelectElement("electronicMailAddress").SetValue(contactPerson.Email);
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

        private static void SetTitle(XElement dataset, string title)
        {
            dataset.XPathSelectElement("title").SetValue(title);
        }
    }
}