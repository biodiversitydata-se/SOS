using DwC_A.Meta;
using System.Collections.Generic;

namespace DwC_A
{
    /// <summary>
    /// Reads a file
    /// </summary>
    public interface IAsyncFileReader
    {
        /// <summary>
        /// Fully qualified path to file
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Collection of metadata for file
        /// </summary>
        IFileMetaData FileMetaData { get; }
        /// <summary>
        /// Enumerable collection of data row objects
        /// </summary>
        IAsyncEnumerable<IRow> GetDataRowsAsync();
        /// <summary>
        /// Enumerable collection of header row objects
        /// </summary>
        IAsyncEnumerable<IRow> GetHeaderRowsAsync();
        /// <summary>
        /// Enumerable collection of all row objects including headers and data
        /// </summary>
        IAsyncEnumerable<IRow> GetRowsAsync();
    }
}