﻿using SOS.Lib.Models.DataQuality;
using System.Threading.Tasks;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Interface for Data Quality Manager
    /// </summary>
    public interface IDataQualityManager
    {
        /// <summary>
        /// Get data quality report for passed organism group
        /// </summary>
        /// <param name="organismGroup"></param>
        /// <returns></returns>
        Task<DataQualityReport> GetReportAsync(string organismGroup);
    }
}
