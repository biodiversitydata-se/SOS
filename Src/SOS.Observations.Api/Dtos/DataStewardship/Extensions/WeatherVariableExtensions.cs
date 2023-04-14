using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Observations.Api.Dtos.DataStewardship.Enums;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class WeatherVariableExtensions
    {
        private static Cloudiness ToCloudiness(this Lib.Models.Processed.DataStewardship.Enums.Cloudiness source)
        {
            return (Cloudiness)source;
        }

        private static Precipitation ToPrecipitation(this Lib.Models.Processed.DataStewardship.Enums.Precipitation source)
        {
            return (Precipitation)source;
        }

        private static SnowCover ToSnowCover(this Lib.Models.Processed.DataStewardship.Enums.SnowCover source)
        {
            return (SnowCover)source;
        }

        private static Unit ToUnit(this Lib.Models.Processed.DataStewardship.Enums.Unit source)
        {
            return (Unit)source;
        }

        private static Visibility ToVisibility(this Lib.Models.Processed.DataStewardship.Enums.Visibility source)
        {
            return (Visibility)source;
        }

        private static WeatherMeasuringDto ToWeatherMeasuring(this WeatherMeasuring source)
        {
            if (source == null) return null;

            return new WeatherMeasuringDto
            {
                Unit = source.Unit?.ToUnit(),
                WeatherMeasure = source.WeatherMeasure
            };
        }

        private static WindDirectionCompass ToWindDirectionCompass(this Lib.Models.Processed.DataStewardship.Enums.WindDirectionCompass source)
        {
            return (WindDirectionCompass)source;
        }

        private static WindStrength ToWindStrength(this Lib.Models.Processed.DataStewardship.Enums.WindStrength source)
        {
            return (WindStrength)source;
        }

        public static WeatherVariableDto ToDto(this WeatherVariable source)
        {
            if (source == null) return null;

            return new WeatherVariableDto
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
