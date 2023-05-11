using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.Managers.Interfaces
{
    public interface IDwcaDataValidationReportManager
    {
        Task<DwcaDataValidationReport<DwcObservationVerbatim, Observation>> CreateDataValidationSummary(
            ArchiveReader archiveReader,
            int maxNrObservationsToRead = 100000,
            int nrValidObservationsInReport = 100, 
            int nrInvalidObservationsInReport = 100,
            int nrTaxaInSummary = 20);
    }
}
