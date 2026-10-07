using SOS.Api.Contracts.Observations.Responses;
using SOS.Api.Contracts.Lookups.Responses;

namespace SOS.Lib.Extensions.Dto;

/// <summary>Maps nested response data while retaining null collections in the published JSON.</summary>
public static class ObservationResponseMappingExtensions
{
    public static Event? ToEventDto(this Models.Processed.Observation.Event? source)
    {
        if (source is null) return null;
        return new Event
        {
            EventId = source.EventId,
            StartDate = source.StartDate,
            StartDayOfYear = source.StartDayOfYear,
            StartHistogramWeek = source.StartHistogramWeek,
            StartYear = source.StartYear,
            StartMonth = source.StartMonth,
            StartDay = source.StartDay,
            EndDate = source.EndDate,
            EndDayOfYear = source.EndDayOfYear,
            EndHistogramWeek = source.EndHistogramWeek,
            EndYear = source.EndYear,
            EndMonth = source.EndMonth,
            EndDay = source.EndDay,
            PlainStartDate = source.PlainStartDate,
            PlainEndDate = source.PlainEndDate,
            PlainStartTime = source.PlainStartTime,
            PlainEndTime = source.PlainEndTime,
            DiscoveryMethod = source.DiscoveryMethod is null ? null : new VocabularyValue
            {
                Id = source.DiscoveryMethod.Id,
                Value = source.DiscoveryMethod.Value
            },
            EventRemarks = source.EventRemarks,
            FieldNotes = source.FieldNotes,
            FieldNumber = source.FieldNumber,
            Habitat = source.Habitat,
            Media = source.Media?.Select(ToMultimediaDto).ToList(),
            MeasurementOrFacts = source.MeasurementOrFacts?.Select(value => ToExtendedMeasurementOrFactDto(value)!).ToList(),
            ParentEventId = source.ParentEventId,
            SamplingEffort = source.SamplingEffort,
            SamplingProtocol = source.SamplingProtocol,
            SampleSizeUnit = source.SampleSizeUnit,
            SampleSizeValue = source.SampleSizeValue,
            VerbatimEventDate = source.VerbatimEventDate,
            Weather = ToWeatherDto(source.Weather)
        };
    }

    public static Multimedia ToMultimediaDto(this Models.Processed.Observation.Multimedia source)
    {
        return new Multimedia
        {
            Comments = source.Comments?.Select(value => ToMultimediaCommentDto(value)!).ToList(),
            Type = source.Type,
            Format = source.Format,
            Identifier = source.Identifier,
            References = source.References,
            Title = source.Title,
            Description = source.Description,
            Created = source.Created,
            Creator = source.Creator,
            Contributor = source.Contributor,
            Publisher = source.Publisher,
            Audience = source.Audience,
            Source = source.Source,
            License = source.License,
            RightsHolder = source.RightsHolder,
            DatasetID = source.DatasetID
        };
    }

    private static MultimediaComment? ToMultimediaCommentDto(Models.Processed.Observation.MultimediaComment? source) =>
        source is null ? null : new MultimediaComment
        {
            Comment = source.Comment,
            CommentBy = source.CommentBy,
            Created = source.Created
        };

    private static ExtendedMeasurementOrFact? ToExtendedMeasurementOrFactDto(Models.Processed.Observation.ExtendedMeasurementOrFact? source) =>
        source is null ? null : new ExtendedMeasurementOrFact
        {
            MeasurementID = source.MeasurementID,
            OccurrenceID = source.OccurrenceID,
            MeasurementType = source.MeasurementType,
            MeasurementTypeID = source.MeasurementTypeID,
            MeasurementValue = source.MeasurementValue,
            MeasurementValueID = source.MeasurementValueID,
            MeasurementAccuracy = source.MeasurementAccuracy,
            MeasurementUnit = source.MeasurementUnit,
            MeasurementUnitID = source.MeasurementUnitID,
            MeasurementDeterminedDate = source.MeasurementDeterminedDate,
            MeasurementDeterminedBy = source.MeasurementDeterminedBy,
            MeasurementMethod = source.MeasurementMethod,
            MeasurementRemarks = source.MeasurementRemarks
        };

    private static Weather? ToWeatherDto(Models.Processed.Observation.Weather? source) =>
        source is null ? null : new Weather
        {
            SnowCover = (SnowCover?)source.SnowCover,
            Sunshine = ToMeasuringDto(source.Sunshine),
            AirTemperature = ToMeasuringDto(source.AirTemperature),
            WindDirection = (CompassDirection?)source.WindDirection,
            WindDirectionDegrees = ToMeasuringDto(source.WindDirectionDegrees),
            WindSpeed = ToMeasuringDto(source.WindSpeed),
            // These two enums have different numeric values in the domain and API models.
            WindStrength = source.WindStrength.HasValue ? Enum.Parse<WindStrength>(source.WindStrength.Value.ToString()) : null,
            Precipitation = source.Precipitation.HasValue ? Enum.Parse<Precipitation>(source.Precipitation.Value.ToString()) : null,
            Visibility = (Visibility?)source.Visibility,
            Cloudiness = (Cloudiness?)source.Cloudiness
        };

    private static Measuring? ToMeasuringDto(Models.Processed.Observation.Measuring? source) =>
        source is null ? null : new Measuring
        {
            Value = source.Value,
            Unit = (Unit?)source.Unit
        };
}
