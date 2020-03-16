﻿using DwC_A.Meta;
using System.Collections.Generic;

namespace DwC_A
{
    /// <summary>
    /// Collection of fields
    /// </summary>
    public interface IRow
    {
        /// <summary>
        /// List of meta data properties for the fields in this row
        /// </summary>
        IFieldMetaData FieldMetaData { get; }
        /// <summary>
        /// An enumerable collection of field values
        /// </summary>
        IEnumerable<string> Fields { get; }
        /// <summary>
        /// Returns field value for a specified term
        /// </summary>
        /// <param name="term">Darwin Core Term</param>
        /// <returns>Field value</returns>
        string this[string term] { get; }
        /// <summary>
        /// Returns field value for a specified field index
        /// </summary>
        /// <param name="index">Index into collection of fields</param>
        /// <returns>Field value</returns>
        string this[int index] { get; }
    }
}
