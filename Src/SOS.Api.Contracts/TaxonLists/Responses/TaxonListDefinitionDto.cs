using System.Collections.Generic;
using System.Linq;

namespace SOS.Api.Contracts.TaxonLists.Responses;

/// <summary>
/// Taxon list definition.
/// </summary>
public class TaxonListDefinitionDto
{
    /// <summary>
    /// Is the list allowed in signal search?
    /// </summary>
    public bool CanBeUsedInSignalSearch { get; set; }

    /// <summary>
    /// The Id of the taxon list.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The parent Id of the taxon list.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    ///     The name of the taxon list.
    /// </summary>
    public string Name { get; set; }

}
