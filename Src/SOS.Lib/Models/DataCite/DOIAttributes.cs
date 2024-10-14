using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.DataCite
{
    public class DOIAttributes
    {
        /// <summary>
        ///  Creation date
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// LIst of DOI contributors
        /// </summary>
        [JsonPropertyName("contributors")]
        public IEnumerable<DOIContributor> Contributors { get; set; }

        /// <summary>
        /// LIst of DOI creators
        /// </summary>
        [JsonPropertyName("creators")]
        public IEnumerable<DOICreator> Creators { get; set; }

        /// <summary>
        /// Descriptions of DOI
        /// </summary>
        [JsonPropertyName("descriptions")]
        public IEnumerable<DOIDescription> Descriptions { get; set; }

        /// <summary>
        /// Full DOI
        /// </summary>
        [JsonPropertyName("doi")]
        public string DOI { get; set; }

        /// <summary>
        /// Event used for changing state
        /// </summary>
        [JsonPropertyName("event")]
        public string Event { get; set; }

        /// <summary>
        /// Content formats. I.e. application/zip
        /// </summary>
        [JsonPropertyName("formats")]
        public IEnumerable<string> Formats { get; set; }

        /// <summary>
        /// DOI Identifiers
        /// </summary>
        [JsonPropertyName("identifiers")]
        public IEnumerable<DOIIdentifier> Identifiers { get; set; }

        /// <summary>
        /// Language, eng etc.
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; }

        /// <summary>
        /// DOI prefix
        /// </summary>
        [JsonIgnore]
        public string Prefix { get; set; }

        /// <summary>
        /// Publisher attribute
        /// </summary>
        [JsonPropertyName("publisher")]
        public string Publisher { get; set; }

        /// <summary>
        /// Year of publication
        /// </summary>
        [JsonPropertyName("publicationYear")]
        public int? PublicationYear { get; set; }

        /// <summary>
        /// DOI state
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Subjects
        /// </summary>
        public IEnumerable<DOISubject> Subjects { get; set; }

        /// <summary>
        /// DOI suffix
        /// </summary>
        [JsonIgnore]
        public string Suffix { get; set; }

        /// <summary>
        /// Titles of DOI
        /// </summary>
        [JsonPropertyName("titles")]
        public IEnumerable<DOITitle> Titles { get; set; }

        /// <summary>
        /// Types
        /// </summary>
        [JsonPropertyName("types")]
        public DOITypes Types { get; set; }

        /// <summary>
        /// DOI Url
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
