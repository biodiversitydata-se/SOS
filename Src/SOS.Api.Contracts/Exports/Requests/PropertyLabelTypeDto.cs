namespace SOS.Api.Contracts.Exports.Requests;

/// <summary>
/// Determines how property labels are written in an export.
/// </summary>
public enum PropertyLabelTypeDto
{
    /// <summary>Use the property name.</summary>
    PropertyName = 0,
    /// <summary>Use the complete property path.</summary>
    PropertyPath = 1,
    /// <summary>Use the Swedish property label.</summary>
    Swedish = 2,
    /// <summary>Use the English property label.</summary>
    English = 3
}
