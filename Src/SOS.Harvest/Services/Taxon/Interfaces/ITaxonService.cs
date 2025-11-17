using SOS.Lib.Models.DarwinCore;

namespace SOS.Harvest.Services.Taxon.Interfaces;

/// <summary>
///     Interface for taxon service
/// </summary>
public interface ITaxonService
{
    /// <summary>
    ///     Get all taxa
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync();

    IEnumerable<DarwinCoreTaxon> GetTaxaFromDwcaFile(string filePath);

    IEnumerable<DarwinCoreTaxon> GetTaxaFromDwcaFileStream(Stream zipFileContentStream);
}