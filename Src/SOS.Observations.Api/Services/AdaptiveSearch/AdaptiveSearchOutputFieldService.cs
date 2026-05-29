namespace SOS.Observations.Api.Services.AdaptiveSearch;

internal static class AdaptiveSearchOutputFields
{
    public static ICollection<string> EnsureCoordinates(ICollection<string> outputFields)
    {
        if (outputFields == null || !outputFields.Any())
        {
            return outputFields;
        }

        if (outputFields.Contains("location", StringComparer.OrdinalIgnoreCase) ||
            outputFields.Contains("location.decimalLongitude", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.decimalLatitude", StringComparer.OrdinalIgnoreCase))
        {
            return outputFields;
        }

        var newList = new List<string>(outputFields);
        AddIfMissing(newList, "location.decimalLongitude");
        AddIfMissing(newList, "location.decimalLatitude");
        return newList;
    }

    public static ICollection<string> EnsurePointPolygonFields(ICollection<string> outputFields)
    {
        if (outputFields == null || !outputFields.Any())
        {
            return outputFields;
        }

        if (outputFields.Contains("location", StringComparer.OrdinalIgnoreCase) ||
            outputFields.Contains("location.decimalLongitude", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.decimalLatitude", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.coordinateUncertaintyInMeters", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.locationId", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.type", StringComparer.OrdinalIgnoreCase))
        {
            return outputFields;
        }

        var newList = new List<string>(EnsureCoordinates(outputFields));
        AddIfMissing(newList, "location.coordinateUncertaintyInMeters");
        AddIfMissing(newList, "location.locationId");
        AddIfMissing(newList, "location.type");
        return newList;
    }

    public static ICollection<string> EnsureLocationFields(ICollection<string> outputFields)
    {
        if (outputFields == null || !outputFields.Any())
        {
            return outputFields;
        }

        if (outputFields.Contains("location", StringComparer.OrdinalIgnoreCase) ||
            outputFields.Contains("location.locationId", StringComparer.OrdinalIgnoreCase) &&
            outputFields.Contains("location.type", StringComparer.OrdinalIgnoreCase))
        {
            return outputFields;
        }

        var newList = new List<string>(outputFields);
        AddIfMissing(newList, "location.locationId");
        AddIfMissing(newList, "location.type");
        return newList;
    }

    public static ICollection<string> EnsureTaxonId(ICollection<string> outputFields)
    {
        if (outputFields == null || !outputFields.Any())
        {
            return outputFields;
        }

        if (outputFields.Contains("taxon", StringComparer.OrdinalIgnoreCase) ||
            outputFields.Contains("taxon.id", StringComparer.OrdinalIgnoreCase))
        {
            return outputFields;
        }

        var newList = new List<string>(outputFields);
        AddIfMissing(newList, "taxon.id");
        return newList;
    }

    private static void AddIfMissing(ICollection<string> outputFields, string field)
    {
        if (!outputFields.Contains(field, StringComparer.OrdinalIgnoreCase))
        {
            outputFields.Add(field);
        }
    }
}
