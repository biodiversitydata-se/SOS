using System;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Event dataset
    /// </summary>
    [DataContract]
    public class EventDataset
    {
        /// <summary>
        /// Identifier
        /// </summary>

        [DataMember(Name = "identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Title
        /// </summary>

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }
}
