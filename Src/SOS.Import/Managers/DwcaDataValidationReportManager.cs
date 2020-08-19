using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using Microsoft.Extensions.Logging;
using SOS.Import.DarwinCore.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;

namespace SOS.Import.Managers
{
    /// <summary>
    /// DwC-A data validation manager.
    /// </summary>
    public class DwcaDataValidationReportManager : Interfaces.IDwcaDataValidationReportManager
    {
        private readonly IFieldMappingResolverHelper _fieldMappingResolverHelper;
        private readonly IDwcArchiveReader _dwcArchiveReader;
        private readonly IValidationManager _validationManager;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly ILogger<DwcaDataValidationReportManager> _logger;
        private Dictionary<int, ProcessedTaxon> _taxonById;
        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappings;

        public DwcaDataValidationReportManager(IDwcArchiveReader dwcArchiveReader,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IProcessedTaxonRepository processedTaxonRepository,
            ILogger<DwcaDataValidationReportManager> logger)
        {
            _fieldMappingResolverHelper = fieldMappingResolverHelper ?? throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
            _dwcArchiveReader = dwcArchiveReader ?? throw new ArgumentNullException(nameof(dwcArchiveReader));
            _validationManager = validationManager ?? throw new ArgumentNullException(nameof(validationManager));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedTaxonRepository = processedTaxonRepository ?? throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Task.Run(InitializeAsync).Wait();
        }

        private async Task InitializeAsync()
        {
            var taxa = await _processedTaxonRepository.GetAllAsync();
            _taxonById = taxa.ToDictionary(m => m.Id, m => m);

            var allFieldMappings = await _processedFieldMappingRepository.GetAllAsync();
            _fieldMappings = DwcaObservationFactory.GetFieldMappingsDictionary(
                ExternalSystemId.DarwinCore,
                allFieldMappings.ToArray(),
                true);
        }

        public async Task<DwcaDataValidationSummary<DwcObservationVerbatim, ProcessedObservation>> CreateDataValidationSummary(
             ArchiveReader archiveReader,
             int nrValidObservationsLimit = 100,
             int nrInvalidObservationsLimit = 100,
             int maxNrObservationsToRead = 100000)
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var dataProvider = new DataProvider
            {
                Id = 0,
                Identifier = "DwcaDataValidationReport",
                Type = DataProviderType.DwcA
            };

            var dwcaObservationFactory = new DwcaObservationFactory(
                dataProvider,
                _taxonById,
                _fieldMappings,
                _areaHelper);

            var observationsBatches = _dwcArchiveReader.ReadArchiveInBatchesAsync(
                archiveReader,
                dataProvider);

            var validObservations = new List<ValidObservationTuple<DwcObservationVerbatim, ProcessedObservation>>();
            var invalidObservations = new List<InvalidObservationTuple<DwcObservationVerbatim>>();
            int nrProcessedObservations = 0;
            int nrValidObservations = 0;
            int nrInvalidObservations = 0;
            await foreach (var observationsBatch in observationsBatches)
            {
                foreach (var verbatimObservation in observationsBatch)
                {
                    var processedObservation = dwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                    nrProcessedObservations++;
                    _fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation>
                        {processedObservation});

                    var observationValidation = _validationManager.ValidateObservation(processedObservation);
                    if (observationValidation.IsValid)
                    {
                        nrValidObservations++;
                        if (validObservations.Count < nrValidObservationsLimit)
                        {
                            validObservations.Add(new ValidObservationTuple<DwcObservationVerbatim, ProcessedObservation>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservation = processedObservation
                            });
                        }
                    }
                    else
                    {
                        nrInvalidObservations++;
                        if (invalidObservations.Count < nrInvalidObservationsLimit)
                        {
                            invalidObservations.Add(new InvalidObservationTuple<DwcObservationVerbatim>
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservationDefects = observationValidation.Defects
                            });
                        }
                    }

                    if (nrProcessedObservations >= maxNrObservationsToRead) break;
                }

                if (nrProcessedObservations >= maxNrObservationsToRead) break;
            }

            return new DwcaDataValidationSummary<DwcObservationVerbatim, ProcessedObservation>
            {
                NrObservationsProcessed = nrProcessedObservations,
                NrValidObservations = nrValidObservations,
                NrInvalidObservations = nrInvalidObservations,
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations
            };
        }
    }
}
