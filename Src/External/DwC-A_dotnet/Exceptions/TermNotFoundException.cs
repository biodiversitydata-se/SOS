using System;
using System.Runtime.Serialization;

namespace DwC_A.Exceptions
{
    internal class TermNotFoundException : Exception
    {
        public TermNotFoundException(string term) :
            base(BuildMessage(term))
        {
        }

        public TermNotFoundException(string term, Exception innerException) :
            base(BuildMessage(term), innerException)
        {
        }

        protected TermNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string BuildMessage(string term)
        {
            return $"Term {term} not found";
        }
    }
}