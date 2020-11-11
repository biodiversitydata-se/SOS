using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    public class ExternalSystemMappingValueDto
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
    }
}
