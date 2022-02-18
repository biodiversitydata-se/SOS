using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Validation
{
    /// <summary>
    ///     Invalid observation
    /// </summary>
    public class InvalidObservation : IEntity<ObjectId>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="datasetID"></param>
        /// <param name="datasetName"></param>
        /// <param name="occurrenceID"></param>
        public InvalidObservation(string datasetID, string datasetName, string occurrenceID)
        {
            DatasetID = datasetID;
            DatasetName = datasetName;
            Defects = new List<ObservationDefect>();
            OccurrenceID = occurrenceID;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Is the observation valid.
        /// </summary>
        [BsonIgnore]
        public bool IsValid => !Defects.Any();

        /// <summary>
        /// Is the observation invalid.
        /// </summary>
        [BsonIgnore]
        public bool IsInvalid => Defects.Any();

        /// <summary>
        ///     Id of data set
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        ///     Name of data set
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     List of defects
        /// </summary>
        public ICollection<ObservationDefect> Defects { get; set; }

        public string OccurrenceID { get; set; }

        public DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }
    }
}