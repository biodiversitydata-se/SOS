using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.Excel.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Export.IO.Excel
{
    public class ExcelFileWriter : IExcelFileWriter
    {
        private readonly IProcessedPublicObservationRepository _processedPublicObservationRepository;
        private readonly IFileService _fileService;
        private readonly IVocabularyValueResolver _vocabularyValueResolver;
        private readonly ILogger<ExcelFileWriter> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="logger"></param>
        public ExcelFileWriter(IProcessedPublicObservationRepository processedPublicObservationRepository, 
            IFileService fileService,
            IVocabularyValueResolver vocabularyValueResolver,
            ILogger<ExcelFileWriter> logger)
        {
            _processedPublicObservationRepository = processedPublicObservationRepository ??
                                                    throw new ArgumentNullException(
                                                        nameof(processedPublicObservationRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _vocabularyValueResolver = vocabularyValueResolver ??
                                       throw new ArgumentNullException(nameof(vocabularyValueResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> CreateFileAync(SearchFilter filter, string exportPath, string fileName,
            IJobCancellationToken cancellationToken)
        {
            string temporaryZipExportFolderPath = null;

            try
            {
                temporaryZipExportFolderPath = Path.Combine(exportPath, fileName);
                var zipFilePath = _fileService.CompressFolder(exportPath, fileName);
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
                return zipFilePath;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Excel File.");
                throw;
            }
            finally
            {
                _fileService.DeleteFolder(temporaryZipExportFolderPath);
            }
        }
    }
}
