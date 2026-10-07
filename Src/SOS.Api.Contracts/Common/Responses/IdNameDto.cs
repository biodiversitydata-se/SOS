namespace SOS.Api.Contracts.Common.Responses;

/// <summary>
/// An identifier and its display name.
/// </summary>
public class IdNameDto<T>
{
    /// <summary>
    ///     Id
    /// </summary>
    public T Id { get; set; }

    /// <summary>
    ///     Name of area
    /// </summary>
    public string Name { get; set; }
}
