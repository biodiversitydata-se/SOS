using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// DTO sample for observation V2.
    /// To enable correct documentation all properties should be added.
    /// </summary>
    public class ObservationDtoV2
    {
        /// <summary>
        /// Transforms a dynamic observation of type ProcessedObservation to a V2 observation.
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        public static dynamic TransformToV2(dynamic observation)
        {
            if (observation is IDictionary<string, object> obs)
            {
                obs.Remove("artportalenInternal");
                obs.Remove("basisOfRecord");

                if (obs.TryGetValue("language", out var language))
                {
                    obs.Add("lang", language);
                    obs.Remove("language");
                }
            }

            return observation;
        }
    }
}