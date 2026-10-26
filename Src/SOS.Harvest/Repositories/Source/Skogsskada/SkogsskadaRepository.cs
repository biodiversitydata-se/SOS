using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Skogsskada;
using SOS.Harvest.Repositories.Source.Skogsskada.Interfaces;
using SOS.Harvest.Services.Interfaces;

namespace SOS.Harvest.Repositories.Source.Skogsskada;

public class SkogsskadaRepository : ISkogsskadaRepository
{
    /// <summary>
    ///     Logger
    /// </summary>
    private readonly ILogger<SkogsskadaRepository> _logger;

    /// <summary>
    ///     Skogsskada data service
    /// </summary>
    private readonly ISkogsskadaDataService _skogsskadaDataService;


    private async Task<IEnumerable<E>> QueryAsync<E>(string query, dynamic? parameters = null)
    {
        return await _skogsskadaDataService.QueryAsync<E>(query, parameters);
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="skogsskadaDataService"></param>
    /// <param name="logger"></param>
    public SkogsskadaRepository(ISkogsskadaDataService skogsskadaDataService, ILogger<SkogsskadaRepository> logger)
    {
        _skogsskadaDataService = skogsskadaDataService ??
                                          throw new ArgumentNullException(nameof(skogsskadaDataService));

        _logger = logger ??
                  throw new ArgumentNullException(nameof(logger));
    }


    /// <inheritdoc />
    public async Task<IEnumerable<SkogsskadaEntity>> GetChunkAsync(string? afterOccurrenceId, int batchSize)
    {
        try
        {
            var query = @"
                SELECT
                    ""occurrenceID"",
                    ""basisOfRecord"",
                    ""eventDate"",
                    ""scientificName"",
                    ""identificationVerificationStatus"",
                    ""decimalLatitude"",
                    ""decimalLongitude"",
                    ""geodeticDatum"",
                    ""footprintWKT"",
                    ""footprintSRS"",
                    ""ownerInstitutionCode"",
                    ""institutionCode"",
                    ""occurrenceRemarks"",
                    ""verbatimCoordinates"",
                    ""verbatimSRS"",
                    ""country"",
                    ""countryCode"",
                    ""recordedBy"",
                    ""rightsHolder"",
                    ""accessRights"",
                    ""samplingProtocol""
                FROM sosview
                WHERE @After IS NULL OR ""occurrenceID"" > @After
                ORDER BY ""occurrenceID""
                LIMIT @Batch";

            return await QueryAsync<SkogsskadaEntity>(query, new { After = afterOccurrenceId, Batch = batchSize });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting observations");

            throw;
        }
    }
}