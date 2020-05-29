using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib.Models.Shared
{
    public class ExternalSystemMappingField
    {
        /// <summary>
        ///     The key in the external system.
        /// </summary>
        public string Key { get; set; }

        public string Description { get; set; }
        public ICollection<ExternalSystemMappingValue> Values { get; set; }

        public Dictionary<object, int> GetIdByValueDictionary(bool convertValuesToLowercase = false)
        {
            if (Values.First().Value is long || Values.First().Value is byte)
            {
                return Values?.ToDictionary(m => (object) Convert.ToInt32(m.Value), m => m.SosId);
            }

            if (Values.First().Value is string && convertValuesToLowercase)
            {
                return Values?.ToDictionary(m => (object) m.Value.ToString().ToLower(), m => m.SosId);
            }

            return Values?.ToDictionary(m => m.Value, m => m.SosId);
        }

        public Dictionary<int, int> GetIntIdByValueDictionary()
        {
            return Values?.ToDictionary(m => Convert.ToInt32(m.Value), m => m.SosId);
        }
    }
}