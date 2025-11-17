using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Text.Json;

namespace SOS.Lib.Extensions;

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

    extension(Observation observation)
    {
        public bool ShallBeProtected()
        {
            return observation.Occurrence.SensitivityCategory > 2 || observation.AccessRights?.Id == (int)AccessRightsId.NotForPublicUsage;
        }
    }

    extension(IEnumerable<dynamic> dynamicObjects)
    {
        /// <summary>
        /// Cast dynamic to observation
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Observation> ToObservations()
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
        /// <returns></returns>
        public Observation[] ToObservationsArray()
        {
            if (dynamicObjects == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<Observation[]>(JsonSerializer.Serialize(dynamicObjects), jsonSerializerOptions);
        }
    }
}