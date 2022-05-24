using System;

namespace SOS.Lib.Models.ApplicationInsights
{
    /// <summary>
    /// Application insights log row
    /// </summary>
    public class ApiLogRow
    {
        /// <summary>
        /// API management user account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public double? Duration { get; set; }

        /// <summary>
        /// Issue date
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// End point
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Result code
        /// </summary>
        public string HttpResponseCode { get; set; }

        /// <summary>
        /// Method, GET, POST PUT etc
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// True if protected observations was requested
        /// </summary>
        public string ProtectedObservations { get; set; }

        /// <summary>
        /// Request body
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Number of items returned
        /// </summary>
        public string ResponseCount { get; set; }

        /// <summary>
        /// Name of system making the request
        /// </summary>
        public string RequestingSystem { get; set; }        

        /// <summary>
        /// Successful call
        /// </summary>
        public string Success { get; set; }

        /// <summary>
        /// User making the request
        /// </summary>
        public string UserId { get; set; }
    }
}
