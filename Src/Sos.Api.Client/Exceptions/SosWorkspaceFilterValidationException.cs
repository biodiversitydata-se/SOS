namespace SOS.Api.Client.Exceptions;

/// <summary>
/// Exception thrown when workspace settings cannot be converted into a valid SOS filter.
/// </summary>
public sealed class SosWorkspaceFilterValidationException : Exception
{
    /// <summary>Creates a workspace-filter validation exception.</summary>
    public SosWorkspaceFilterValidationException(
        string parameterName,
        string message,
        IReadOnlyCollection<string>? allowedValues = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ParameterName = parameterName;
        AllowedValues = allowedValues;
    }

    /// <summary>The workspace parameter that failed validation.</summary>
    public string ParameterName { get; }
    /// <summary>Allowed values when validation failed because a value was unsupported.</summary>
    public IReadOnlyCollection<string>? AllowedValues { get; }
}
