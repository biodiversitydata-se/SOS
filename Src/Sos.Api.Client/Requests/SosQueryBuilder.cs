using System.Globalization;

namespace SOS.Api.Client.Requests;

internal sealed class SosQueryBuilder
{
    private readonly List<string> _parts = [];

    public SosQueryBuilder Add(string name, string? value)
    {
        if (value is not null)
        {
            _parts.Add($"{Escape(name)}={Escape(value)}");
        }

        return this;
    }

    public SosQueryBuilder Add(string name, int? value) =>
        AddFormattable(name, value);

    public SosQueryBuilder Add(string name, long? value) =>
        AddFormattable(name, value);

    public SosQueryBuilder Add(string name, double? value) =>
        AddFormattable(name, value);

    public SosQueryBuilder Add(string name, bool? value) =>
        Add(name, value.HasValue ? value.Value.ToString().ToLowerInvariant() : null);

    public SosQueryBuilder AddEnum<TEnum>(string name, TEnum? value)
        where TEnum : struct, Enum =>
        Add(name, value?.ToString());

    public SosQueryBuilder AddEnum<TEnum>(string name, TEnum value)
        where TEnum : struct, Enum =>
        Add(name, value.ToString());

    public SosQueryBuilder AddValues<T>(string name, IEnumerable<T>? values)
    {
        if (values is null)
        {
            return this;
        }

        foreach (var value in values)
        {
            Add(name, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        return this;
    }

    public string AppendTo(string relativePath) =>
        _parts.Count == 0 ? relativePath : $"{relativePath}?{this}";

    public override string ToString() => string.Join("&", _parts);

    private SosQueryBuilder AddFormattable<T>(string name, T? value)
        where T : struct, IFormattable =>
        Add(name, value?.ToString(null, CultureInfo.InvariantCulture));

    private static string Escape(string value) => Uri.EscapeDataString(value);
}
