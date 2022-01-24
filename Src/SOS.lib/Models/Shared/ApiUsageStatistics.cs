using System;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    /// API Usage statistics.
    /// </summary>
    public class ApiUsageStatistics : IEntity<ObjectId>
    {
        /// <summary>
        /// API management user account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Email address to api management user
        /// </summary>
        public string ApiManagementUserEmail { get; set; }

        /// <summary>
        /// Api management user email domain
        /// </summary>
        public string ApiManagementUserEmailDomain =>
            (ApiManagementUserEmail?.IndexOf('@') ?? -1) > -1 ? ApiManagementUserEmail.Substring(ApiManagementUserEmail.IndexOf('@') + 1) : "";

        /// <summary>
        /// Name of api management user
        /// </summary>
        public string ApiManagementUserName { get; set; }

        /// <summary>
        /// Average duration
        /// </summary>
        public long AverageDuration { get; set; }

        /// <summary>
        /// End point
        /// </summary>
        public string BaseEndpoint => Endpoint?.Contains('/') ?? false ? Endpoint.Substring(0, Endpoint.IndexOf('/')) : Endpoint;

        /// <summary>
        /// Issue date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// End point
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Number of failed calls
        /// </summary>
        public long FailureCount { get; set; }

        /// <summary>
        /// Method, GET, POST PUT etc
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// Number of request
        /// </summary>
        public long RequestCount { get; set; }

        /// <summary>
        /// Sum of observations returned
        /// </summary>
        public long SumResponseCount { get; set; }

        /// <summary>
        /// E-mail to user making the request
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// User making the request
        /// </summary>
        public string UserEmailDomain =>
            (UserEmail?.IndexOf('@') ?? -1) > -1 ? UserEmail.Substring(UserEmail.IndexOf('@') + 1) : "";

        /// <summary>
        /// Id of user making the request
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// User making the request
        /// </summary>
        public string UserName { get; set; }
    }
}