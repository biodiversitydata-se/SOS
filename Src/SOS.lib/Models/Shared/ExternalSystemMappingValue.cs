using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SOS.Lib.Models.Shared
{
    public class ExternalSystemMappingValue
    {
        /// <summary>
        ///     Value in data provider.
        /// </summary>
        [JsonConverter(typeof(ExpandoObjectConverter))]
        public object Value { get; set; }

        /// <summary>
        ///     Id in SOS (Species Observation System).
        /// </summary>
        public int SosId { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(SosId)}: {SosId}";
        }
    }
}