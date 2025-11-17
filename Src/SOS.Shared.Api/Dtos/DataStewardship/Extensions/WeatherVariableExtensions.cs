using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Shared.Api.Dtos.DataStewardship.Enums;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions;

public static class WeatherVariableExtensions
{
    extension(Lib.Models.Processed.DataStewardship.Enums.Cloudiness source)
    {
        private DsCloudiness ToCloudiness()
        {
            return (DsCloudiness)source;
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.Precipitation source)
    {
        private DsPrecipitation ToPrecipitation()
        {
            return (DsPrecipitation)source;
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.SnowCover source)
    {
        private DsSnowCover ToSnowCover()
        {
            return (DsSnowCover)source;
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.Unit source)
    {
        private DsUnit ToUnit()
        {
            return (DsUnit)source;
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.Visibility source)
    {
        private DsVisibility ToVisibility()
        {
            return (DsVisibility)source;
        }
    }

    extension(WeatherMeasuring source)
    {
        private DsWeatherMeasuringDto ToWeatherMeasuring()
        {
            if (source == null) return null;

            return new DsWeatherMeasuringDto
            {
                Unit = source.Unit?.ToUnit(),
                WeatherMeasure = source.WeatherMeasure
            };
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.WindDirectionCompass source)
    {
        private DsWindDirectionCompass ToWindDirectionCompass()
        {
            return (DsWindDirectionCompass)source;
        }
    }

    extension(Lib.Models.Processed.DataStewardship.Enums.WindStrength source)
    {
        private DsWindStrength ToWindStrength()
        {
            return (DsWindStrength)source;
        }
    }

    extension(WeatherVariable source)
    {
        public DsWeatherVariableDto ToDto()
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
