using DwC_A.Meta;
using System.Collections.Generic;

namespace DwC_A
{
    /// <summary>
    /// Reads a file
    /// </summary>
    public interface IFileReader
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
        IEnumerable<IRow> DataRows { get; }
        /// <summary>
        /// Enumerable collection of header row objects
        /// </summary>
        IEnumerable<IRow> HeaderRows { get; }
        /// <summary>
        /// Enumerable collection of all row objects including headers and data
        /// </summary>
        IEnumerable<IRow> Rows { get; }
    }
}