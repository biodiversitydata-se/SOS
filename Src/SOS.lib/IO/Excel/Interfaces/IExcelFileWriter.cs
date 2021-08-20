﻿using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Constants;
using SOS.Lib.Models.Search;

namespace SOS.Lib.IO.Excel.Interfaces
{
    public interface IExcelFileWriter
    {
        /// <summary>
        ///  Create export file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> CreateFileAync(SearchFilter filter, string exportPath, string fileName, string culture,
            IJobCancellationToken cancellationToken);
    }
}
