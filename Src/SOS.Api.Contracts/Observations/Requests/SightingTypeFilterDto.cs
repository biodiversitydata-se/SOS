using System.Text.Json.Serialization;

namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>Controls which ordinary, merged, child and replacement observations from Artportalen are included.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SightingTypeFilterDto>))]
public enum SightingTypeFilterDto
{

    /// <summary>Exclude merged assessment observations.</summary>
    DoNotShowMerged = 0,

    /// <summary>Only include merged assessment observations.</summary>
    ShowOnlyMerged = 1,

    /// <summary>Include both merged assessments and their underlying observations.</summary>
    ShowBoth = 2,

    /// <summary>Include merged assessments but exclude their underlying observations.</summary>
    DoNotShowSightingsInMerged = 3,

    /// <summary>Exclude merged assessments and include replacement child observations.</summary>
    DoNotShowMergedIncludeReplacementChilds = 4,

    /// <summary>Include ordinary observations, child observations and replacements instead of aggregate parent observations.</summary>
    ShowChildrenAndReplacements = 5,

}


