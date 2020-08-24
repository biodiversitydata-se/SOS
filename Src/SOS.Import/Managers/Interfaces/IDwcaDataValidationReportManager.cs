using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Managers.Interfaces
{
    public interface IDwcaDataValidationReportManager
    {
        Task<DwcaDataValidationSummary<DwcObservationVerbatim, ProcessedObservation>> CreateDataValidationSummary(
            ArchiveReader archiveReader,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 100, 
            int nrInvalidObservationsInReport = 100);
    }
}
