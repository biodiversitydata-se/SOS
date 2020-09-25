using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.DataCite
{
    public class DOIAttributes
    {
        /// <summary>
        ///  Creation date
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// LIst of DOI creators
        /// </summary>
        public IEnumerable<DOICreators> Creators { get; set; }

        /// <summary>
        /// Descriptions of DOI
        /// </summary>
        public IEnumerable<DOIDescription> Descriptions { get; set; }

        /// <summary>
        /// Full DOI
        /// </summary>
        public string DOI { get; set; }

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
        /// Titles of DOI
        /// </summary>
        public IEnumerable<DOITitle> Titles { get; set; }

        /// <summary>
        /// DOI Url
        /// </summary>
        public string Url { get; set; }
    }
}
