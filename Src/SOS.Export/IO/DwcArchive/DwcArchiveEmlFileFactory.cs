using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SOS.Export.Helpers;

namespace SOS.Export.IO.DwcArchive
{
    /// <summary>
    /// Creates an eml file based on existing eml that is used as an template.
    /// The created eml will get an updated publish date.     
    /// </summary>
    public static class DwCArchiveEmlFileFactory
    {
        /// <summary>
        /// Creates an eml xml file with an updated publish date.
        /// The xml declaration is omitted.
        /// The new updated file is written to the outstream.
        /// </summary>
        /// <param name="outstream"></param>
        /// <returns></returns>
        public static async Task CreateEmlXmlFileAsync(Stream outstream)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var emlTemplatePath = Path.Combine(assemblyPath, @"Resources\DarwinCore\eml.xml");
            var emlString = CreateEmlXmlString(emlTemplatePath);
            var emlBytes = Encoding.UTF8.GetBytes(emlString);
            await outstream.WriteAsync(emlBytes, 0, emlBytes.Length);
        }

        /// <summary>
        /// Creates an eml xml string with an updated publish date. The xml declaration is omitted. 
        /// </summary>
        /// <param name="emlSourcePath"></param>
        /// <returns></returns>
        public static string CreateEmlXmlString(string emlSourcePath)
        {
            var xDoc = XDocument.Load(emlSourcePath);
            UpdateDynamicElements(xDoc); // Change date to current date
            return xDoc.ToString();
        }

        private static void UpdateDynamicElements(XDocument doc)
        {
            var dataset = doc.Root.Element("dataset");
            SetPubDateToCurrentDate(dataset);
        }

        /// <summary>
        ///  Set pubDate to today's date
        /// </summary>
        /// <param name="dataset"></param>
        private static void SetPubDateToCurrentDate(XElement dataset)
        {
            var pubDate = dataset.Element("pubDate");
            pubDate.SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }
}