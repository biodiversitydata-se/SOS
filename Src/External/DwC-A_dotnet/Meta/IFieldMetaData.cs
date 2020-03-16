using System.Collections.Generic;

namespace DwC_A.Meta
{
    /// <summary>
    /// Collection of meta data for fields
    /// </summary>
    public interface IFieldMetaData: IEnumerable<FieldType>
    {
        /// <summary>
        /// Retrieves index for a term
        /// </summary>
        /// <param name="term">Darwin Core Term</param>
        /// <returns>Index of column containing the term</returns>
        int IndexOf(string term);
        /// <summary>
        /// Retrieves field at index
        /// </summary>
        /// <param name="index">Index or column number</param>
        /// <returns>String representation of field</returns>
        FieldType this[int index] { get; }
        /// <summary>
        /// Retrieves field data for a term
        /// </summary>
        /// <param name="term">Darwin Core Term</param>
        /// <returns>String representation of field</returns>
        FieldType this[string term] { get; }

        bool TryGetTermIndex(string term, out int index);
    }
}