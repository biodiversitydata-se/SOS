using SOS.Lib.Models.Interfaces;
using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.Shark;

/// <summary>
///     Verbatim from Shark
/// </summary>
public class SharkJsonFile : IEntity<string>
{
    /// <summary>
    ///     Array of properties in the rows
    /// </summary>
    public IEnumerable<string> Header { get; set; }

    /// <summary>
    ///     Data rows
    /// </summary>
    public IEnumerable<IEnumerable<string>> Rows { get; set; }

    public string Id { get; set; }
}