﻿using Hangfire;
using SOS.Lib.Models.Shared;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    /// <summary>
    /// Interface for CreateDwcaDataValidationReportJob.
    /// </summary>
    public interface ICreateDwcaDataValidationReportJob
    {
        /// <summary>
        ///     Run create DwC-A validation report job.
        /// </summary>
        /// <returns></returns>
        [JobDisplayName("DwC-A Data Validation Report, Id: \"{0}\", File: \"{2}\"")]
        [Queue("low")]
        Task<Report> RunAsync(
            string reportId,
            string createdBy,
            int maxNrObservationsToRead,
            int nrValidObservationsInReport,
            int nrInvalidObservationsInReport,
            int nrTaxaInTaxonStatistics,
            IJobCancellationToken cancellationToken);
    }
}