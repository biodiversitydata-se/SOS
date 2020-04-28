using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SOS.Lib.Models.Verbatim. VirtualHerbarium;

namespace SOS.Import.Extensions
{
    public static class VirtualHerbariumObservationExtensions
    {
        /// <summary>
        /// Cast virtual Herbarium file data to verbatims
        /// </summary>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static IEnumerable<VirtualHerbariumObservationVerbatim> ToVerbatims(this XDocument xDocument)
        {
            if (xDocument == null)
            {
                return null;
            }
            
            XNamespace xmlns = "urn:schemas-microsoft-com:office:spreadsheet";

            var table = xDocument.Elements($"{xmlns}Workbook")?.Elements()?.Elements();

            if (table.FirstOrDefault()?.HasElements ?? false)
            {
                var header = table.Elements().FirstOrDefault();
                var propertyMapping = new Dictionary<int, string>();
                var index = 0;

                foreach (var cell in header.Elements())
                {
                    propertyMapping.Add(index, cell.Value.Replace("_", "").ToLower());
                    index++;
                }

            }
            return null;
        }
    }
}
