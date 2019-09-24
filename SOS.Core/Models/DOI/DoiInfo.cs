using System;
using MongoDB.Bson;

namespace SOS.Core.Models.DOI
{
    public class DoiInfo
    {
        private const string DoiPrefix = "10.1000";
        public ObjectId Id { get; set; }
        /// <summary>
        /// </summary>
        /// <remarks>
        /// A DOI is a type of Handle System handle, which takes the form of a character string divided into two parts, a prefix and a suffix, separated by a slash.
        /// prefix/suffix
        /// </remarks>
        public string DoiId { get; set; }
        public DoiMetadata Metadata { get; set; }
        public string FileName => $"{DoiId}.zip";
        public DateTime DateCreated { get; set; }
        public static DoiInfo Create(DoiMetadata doiMetadata)
        {
            DoiInfo doiInfo = new DoiInfo
            {
                DateCreated = DateTime.UtcNow,
                DoiId = $"{DoiPrefix}/{Guid.NewGuid().ToString()}",
                Metadata = doiMetadata
            };

            return doiInfo;
        }
    }
}
