using System.Collections.Generic;

namespace SOS.Lib.Models.DataCite
{
    public class DOIAttributes
    {
        /// <summary>
        /// Descriptions of DOI
        /// </summary>
        public IEnumerable<DOIDescription> Descriptions { get; set; }

        /// <summary>
        /// Full DOI
        /// </summary>
        public string DOI { get; set; }

        /// <summary>
        /// DOI Event
        /// publish - Triggers a state move from draft or registered to findable
        /// register - Triggers a state move from draft to registered
        /// hide - Triggers a state move from findable to registered
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Content formats. I.e. application/zip
        /// </summary>
        public IEnumerable<string> Formats { get; set; }

        /// <summary>
        /// DOI Identifiers
        /// </summary>
        public IEnumerable<DOIIdentifier> Identifiers { get; set; }

        /// <summary>
        /// Language, eng etc.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// DOI Prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Publisher attribute
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// Year of publication
        /// </summary>
        public int PublicationYear { get; set; }

        /// <summary>
        /// DOI state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// DOI suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Titles of DOI
        /// </summary>
        public IEnumerable<DOITitle> Titles { get; set; }

        /// <summary>
        /// DOI Url
        /// </summary>
        public string Url { get; set; }
    }
}
