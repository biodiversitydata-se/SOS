using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
{
    public class InvalidObservationDto
    {
        public string DatasetID { get; set; }

        /// <summary>
        ///     Name of data set
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     List of defects
        /// </summary>
        public ICollection<string> Defects { get; set; }

        public string OccurrenceID { get; set; }

        public DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }
    }
}
