using System.Globalization;
using System.Text;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Harvest.Harvesters.Shark
{
    public class SharkHarvestFactory : HarvestBaseFactory, IHarvestFactory<SharkJsonFile, SharkObservationVerbatim>
    {
        private static readonly Encoding Utf8Encoder = Encoding.GetEncoding(
            "UTF-8",
            new EncoderReplacementFallback(string.Empty),
            new DecoderExceptionFallback()
        );

        /// <inheritdoc />
        public async Task<IEnumerable<SharkObservationVerbatim>> CastEntitiesToVerbatimsAsync(SharkJsonFile fileData)
        {
            return await Task.Run(() =>
            {
                if ((!fileData?.Header?.Any() ?? true) || (!fileData?.Rows?.Any() ?? true))
                {
                    return null!;
                }

                var header = fileData!.Header.ToArray();
                var propertyMappings = new Dictionary<string, int>();
                for (var i = 0; i < header.Length; i++)
                {
                    propertyMappings.Add(header[i].Replace("_", "").ToLower(), i);
                }

                propertyMappings.TryGetValue("sampleid", out var sampleIdIndex);
                propertyMappings.TryGetValue("sharksampleidmd5", out var sharksampleidmd5Index);
                propertyMappings.TryGetValue("dyntaxaid", out var dyntaxaidIndex);
                propertyMappings.TryGetValue("scientificname", out var scientificnameIndex);
                propertyMappings.TryGetValue("reportedscientificname", out var reportedscientificnameIndex);

                // If file don't contains taxon id or sample id there's no reason to go on 
                if (
                    (sampleIdIndex.Equals(0) && sharksampleidmd5Index.Equals(0)) || 
                    (dyntaxaidIndex.Equals(0) && scientificnameIndex.Equals(0) && reportedscientificnameIndex.Equals(0)) 
                )
                {
                    return null!;
                }

                propertyMappings.TryGetValue("parameter", out var parameterIndex);
                propertyMappings.TryGetValue("value", out var valueIndex);
                propertyMappings.TryGetValue("unit", out var unitIndex);

                var verbatims = new Dictionary<string, SharkObservationVerbatim>();

                foreach (IReadOnlyList<string> row in fileData.Rows)
                {
                    var sampleId = string.Empty;
                    // If we have md5 col, try get sample id from it
                    if (!sharksampleidmd5Index.Equals(0))
                    { 
                        sampleId = row[sharksampleidmd5Index];
                    }
                    // No md5 id, try sample id 
                    if (string.IsNullOrEmpty(sampleId) && !sampleIdIndex.Equals(0))
                    {
                        sampleId = row[sampleIdIndex];
                    }

                    var sampleIdSuffix = string.Empty;
                    // If we have taxon id col, try get taxon id as suffix
                    if (!dyntaxaidIndex.Equals(0))
                    {
                        sampleIdSuffix = row[dyntaxaidIndex];
                    }
                    // If no taxon id found, try scientific name 
                    if (string.IsNullOrEmpty(sampleIdSuffix) && !scientificnameIndex.Equals(0))
                    {
                        sampleIdSuffix = row[scientificnameIndex];
                    }
                    // Still no suffix? try reported scientific name
                    if (string.IsNullOrEmpty(sampleIdSuffix) && !reportedscientificnameIndex.Equals(0))
                    {
                        sampleIdSuffix = row[reportedscientificnameIndex];
                    }

                    // If sample id or suffix is missing we can't do anything more
                    if (string.IsNullOrEmpty(sampleId) || string.IsNullOrEmpty(sampleIdSuffix))
                    {
                        continue;
                    }

                    sampleId = $"{sampleId}-{sampleIdSuffix}";

                    if (!verbatims.TryGetValue(sampleId, out var verbatim))
                    {
                        verbatim = new SharkObservationVerbatim
                        {
                            Id = NextId,
                            Parameters = new List<SharkParameter>()
                        };

                        foreach (var propertyMapping in propertyMappings)
                        {
                            if (new[] { parameterIndex, valueIndex, unitIndex }.Contains(propertyMapping.Value))
                            {
                                continue;
                            }

                            var propertyValue = Utf8Encoder.GetString(Utf8Encoder.GetBytes(row[propertyMapping.Value]));

                            if (string.IsNullOrEmpty(propertyValue))
                            {
                                continue;
                            }

                            verbatim.SetProperty(propertyMapping.Key, propertyValue);
                        }

                        verbatims.Add(sampleId, verbatim);
                    }
                    
                    verbatim.Parameters.Add(new SharkParameter
                    {
                        Name = parameterIndex == 0 ? "N/A" : row[parameterIndex],
                        Unit = unitIndex == 0 ? "" : row[unitIndex],
                        Value = valueIndex == 0 ? "" : row[valueIndex]
                    });
                }

                return verbatims!.Values;
            });
        }
    }
}
