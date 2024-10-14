using System;

namespace SOS.Lib.Exceptions
{
    public class TaxonValidationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public TaxonValidationException(string message) : base(message)
        {

        }
    }
}
