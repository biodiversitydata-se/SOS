using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Enums;
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
        /// <param name="outputFieldSet"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> CreateFileAync(SearchFilter filter, 
            string exportPath, 
            string fileName, 
            string culture, 
            OutputFieldSet outputFieldSet,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken);
    }
}
