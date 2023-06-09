using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class WeatherVariableExtensions
    {
        private static DsCloudiness ToCloudiness(this Lib.Models.Processed.DataStewardship.Enums.Cloudiness source)
        {
            return (DsCloudiness)source;
        }

        private static DsPrecipitation ToPrecipitation(this Lib.Models.Processed.DataStewardship.Enums.Precipitation source)
        {
            return (DsPrecipitation)source;
        }

        private static DsSnowCover ToSnowCover(this Lib.Models.Processed.DataStewardship.Enums.SnowCover source)
        {
            return (DsSnowCover)source;
        }

        private static DsUnit ToUnit(this Lib.Models.Processed.DataStewardship.Enums.Unit source)
        {
            return (DsUnit)source;
        }

        private static DsVisibility ToVisibility(this Lib.Models.Processed.DataStewardship.Enums.Visibility source)
        {
            return (DsVisibility)source;
        }

        private static DsWeatherMeasuringDto ToWeatherMeasuring(this WeatherMeasuring source)
        {
            if (source == null) return null;

            return new DsWeatherMeasuringDto
            {
                Unit = source.Unit?.ToUnit(),
                WeatherMeasure = source.WeatherMeasure
            };
        }

        private static DsWindDirectionCompass ToWindDirectionCompass(this Lib.Models.Processed.DataStewardship.Enums.WindDirectionCompass source)
        {
            return (DsWindDirectionCompass)source;
        }

        private static DsWindStrength ToWindStrength(this Lib.Models.Processed.DataStewardship.Enums.WindStrength source)
        {
            return (DsWindStrength)source;
        }

        public static DsWeatherVariableDto ToDto(this WeatherVariable source)
        {
            if (source == null) return null;

            return new DsWeatherVariableDto
            {

                SnowCover = source.SnowCover?.ToSnowCover(),
                Sunshine = source.Sunshine?.ToWeatherMeasuring(),
                AirTemperature = source.AirTemperature?.ToWeatherMeasuring(),
                WindDirectionCompass = source.WindDirectionCompass?.ToWindDirectionCompass(),
                WindDirectionDegrees = source.WindDirectionDegrees?.ToWeatherMeasuring(),
                WindSpeed = source.WindDirectionDegrees?.ToWeatherMeasuring(),
                WindStrength = source.WindStrength?.ToWindStrength(),
                Precipitation = source.Precipitation?.ToPrecipitation(),
                Visibility = source.Visibility?.ToVisibility(),
                Cloudiness = source.Cloudiness?.ToCloudiness()
            };
        }
    }
}
