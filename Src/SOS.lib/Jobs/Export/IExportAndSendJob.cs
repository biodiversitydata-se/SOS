﻿using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Jobs.Export
{
    /// <summary>
    ///     Interface for DOI export job
    /// </summary>
    public interface IExportAndSendJob
    {
        /// <summary>
        ///     Run DOI export job
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <param name="description"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisplayName("Export observations. Email={1}, Description={2}")]
        Task<bool> RunAsync(SearchFilter filter, 
            string email, 
            string description,
            IJobCancellationToken cancellationToken);
    }
}