using System;

namespace SOS.Lib.Exceptions
{
    /// <summary>
    /// Thrown when authentication is required, used when accessing protected observations
    /// </summary>
    public class AuthenticationRequiredException : Exception
    {
        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="message"></param>
        public AuthenticationRequiredException(string message) : base(message)
        {
        }

        /// <summary>
        ///  Standard constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AuthenticationRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
