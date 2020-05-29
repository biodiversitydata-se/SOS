using System.Collections.Generic;

namespace DwC_A
{
    /// <summary>
    ///     Interface for tokenizer used to split fielded text rows
    /// </summary>
    public interface ITokenizer
    {
        /// <summary>
        ///     Method to split a line of text into fields
        /// </summary>
        /// <param name="line">One line of text read from file</param>
        /// <returns>An enumerable list of field data</returns>
        IEnumerable<string> Split(string line);
    }
}