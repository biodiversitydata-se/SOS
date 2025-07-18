﻿using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Text.Json;

namespace SOS.Lib.Extensions
{
    public static class ObservationExtensions
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory(),
            }
        };

        public static bool ShallBeProtected(this Observation observation)
        {
            return observation.Occurrence.SensitivityCategory > 2 || observation.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage;
        }

        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        public static IEnumerable<Observation> ToObservations(this IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<IEnumerable<Observation>>(JsonSerializer.Serialize(dynamicObjects), jsonSerializerOptions);
        }

        /// <summary>
        /// Cast dynamic to observations array.
        /// </summary>
        /// <param name="dynamicObjects"></param>
        /// <returns></returns>
        public static Observation[] ToObservationsArray(this IEnumerable<dynamic> dynamicObjects)
        {
            if (dynamicObjects == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<Observation[]>(JsonSerializer.Serialize(dynamicObjects), jsonSerializerOptions);
        }
    }
}