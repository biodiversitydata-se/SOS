using System.Globalization;
using System.Xml.Linq;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Harvest.Harvesters.VirtualHerbarium
{
    public class VirtualHerbariumHarvestFactory : HarvestBaseFactory, IHarvestFactory<XDocument, VirtualHerbariumObservationVerbatim>
    {
        private readonly IDictionary<string, double[]> _localities;
        
        /// <summary>
        ///     Create virtual herbarium verbatim from one row of data
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="propertyMapping"></param>
        /// <param name="localities"></param>
        /// <returns></returns>
        private VirtualHerbariumObservationVerbatim CastEntitiesToVerbatims(XElement rowData, IDictionary<int, string> propertyMapping)
        {
            var observation = new VirtualHerbariumObservationVerbatim
            {
                Id = NextId
            };
            var index = 0;

            foreach (var cell in rowData.Elements())
            {
                var value = cell.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    observation.SetProperty(propertyMapping[index], value);
                }
                
                index++;
            }
            
            // If position is missing, try to get it from locality file
            if ((observation.DecimalLatitude.Equals(0) || observation.DecimalLongitude.Equals(0)) &&
                !string.IsNullOrEmpty(observation.Province) &&
                !string.IsNullOrEmpty(observation.District))
            {
                _localities.TryGetValue(GetLocalityKey(observation.Province, observation.District, observation.Locality),
                    out var locality);

                if (locality != null)
                {
                    observation.DecimalLongitude = locality[0];
                    observation.DecimalLatitude = locality[1];
                }
            }

            return observation;
        }

        /// <summary>
        ///     Return a locality key
        /// </summary>
        /// <param name="province"></param>
        /// <param name="district"></param>
        /// <param name="locality"></param>
        /// <returns></returns>
        private string GetLocalityKey(string province, string district, string locality)
        {
            return
                $"{province?.Replace(" ", "").ToLower()}:{district?.Replace(" ", "").ToLower()}:{locality?.Replace(" ", "").ToLower()}";
        }

        /// <summary>
        ///     Get dictionary with locality column mapping
        /// </summary>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        private IDictionary<string, double[]> InitializeLocalities(XDocument xDocument)
        {
            var localities = new Dictionary<string, double[]>();

            if (xDocument?.Document == null)
            {
                return localities;
            }

            XNamespace xmlns = "urn:schemas-microsoft-com:office:spreadsheet";

            // var table = xDocument.Elements(xmlns + "Workbook")?.Elements()?.Elements();
            var workbook = xDocument.Elements(xmlns + "Workbook").FirstOrDefault();
            var worksheet = workbook.Elements(xmlns + "Worksheet").FirstOrDefault();
            var table = worksheet.Elements(xmlns + "Table");

            if (table.FirstOrDefault()?.HasElements ?? false)
            {
                // Header data in first row
                var header = table.Elements().FirstOrDefault();
                var propertyMapping = new Dictionary<string, int>();
                var index = 0;

                foreach (var cell in header.Elements())
                {
                    if (new[] { "country", "province", "district", "locality", "long", "lat" }.Contains(cell.Value,
                        StringComparer.CurrentCultureIgnoreCase))
                    {
                        propertyMapping.Add(cell.Value.ToLower(), index);
                    }

                    index++;
                }

                // Data in all rows where country is sweden
                var rows = table.Elements(xmlns + "Row").Where(r =>
                    r.Elements().ToArray()[propertyMapping["country"]].Value
                        .Equals("Sweden", StringComparison.CurrentCultureIgnoreCase));

                foreach (var row in rows)
                {
                    var cells = row.Elements().ToArray();
                    var key = GetLocalityKey(cells[propertyMapping["province"]].Value,
                        cells[propertyMapping["district"]].Value, cells[propertyMapping["locality"]].Value);
                    var lon = double.Parse(cells[propertyMapping["long"]].Value, CultureInfo.InvariantCulture);
                    var lat = double.Parse(cells[propertyMapping["lat"]].Value, CultureInfo.InvariantCulture);

                    if (!localities.ContainsKey(key))
                    {
                        localities.Add(key, new[] { lon, lat });
                    }
                }
            }

            return localities;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xDocument"></param>
        public VirtualHerbariumHarvestFactory(
            XDocument xDocument): base()
        {
            _localities = InitializeLocalities(xDocument);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VirtualHerbariumObservationVerbatim>> CastEntitiesToVerbatimsAsync(XDocument xDocument)
        {
            return await Task.Run(() =>
            {
                if (xDocument?.Document == null)
                {
                    return Array.Empty<VirtualHerbariumObservationVerbatim>();
                }

                XNamespace xmlns = "urn:schemas-microsoft-com:office:spreadsheet";

                // var table = xDocument.Elements(xmlns + "Workbook")?.Elements()?.Elements();
                var workbook = xDocument.Elements(xmlns + "Workbook").FirstOrDefault();
                var worksheet = workbook.Elements(xmlns + "Worksheet").FirstOrDefault();
                var table = worksheet.Elements(xmlns + "Table");

                if (table.FirstOrDefault()?.HasElements ?? false)
                {
                    // Header data in first row
                    var header = table.Elements().FirstOrDefault();
                    var propertyMapping = new Dictionary<int, string>();
                    var index = 0;

                    foreach (var cell in header.Elements())
                    {
                        propertyMapping.Add(index, cell.Value.Replace("_", "").ToLower());
                        index++;
                    }

                    // Data in all rows except first one
                    var rows = table.Elements(xmlns + "Row").Skip(1);

                    return from r in rows
                           select CastEntitiesToVerbatims(r, propertyMapping);
                }

                return Array.Empty<VirtualHerbariumObservationVerbatim>();
            });
        }
    }
}
