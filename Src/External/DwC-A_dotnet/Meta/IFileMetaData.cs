using System.Text;

namespace DwC_A.Meta
{
    /// <summary>
    /// Reads various file meta data
    /// </summary>
    public interface IFileMetaData
    {
        /// <summary>
        /// Date format for dates
        /// </summary>
        string DateFormat { get; }
        /// <summary>
        /// File encdoing.  Default UTF-8
        /// </summary>
        Encoding Encoding { get; }
        /// <summary>
        /// Collection of metadata for fields
        /// </summary>
        /// <see cref="IFileMetaData"/>
        IFieldMetaData Fields { get; }
        /// <summary>
        /// Field quote character
        /// </summary>
        string FieldsEnclosedBy { get; }
        /// <summary>
        /// Field delimiter
        /// </summary>
        string FieldsTerminatedBy { get; }
        /// <summary>
        /// Filename only without path info
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Number of header rows
        /// </summary>
        int HeaderRowCount { get; }
        /// <summary>
        /// Index field information
        /// </summary>
        IdFieldType Id { get; }
        /// <summary>
        /// Line end style
        /// </summary>
        string LinesTerminatedBy { get; }
        /// <summary>
        /// Number of characters for line end.  
        /// </summary>
        int LineTerminatorLength { get; }
        /// <summary>
        /// File row type
        /// </summary>
        string RowType { get; }
    }
}