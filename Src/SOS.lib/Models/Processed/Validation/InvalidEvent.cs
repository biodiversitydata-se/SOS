using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Validation
{
    /// <summary>
    ///     Invalid event
    /// </summary>
    public class InvalidEvent : IEntity<ObjectId>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public InvalidEvent(string datasetID, string datasetName, string eventID)
        {
            DatasetID = datasetID;
            DatasetName = datasetName;
            Defects = new List<EventDefect>();
            EventID = eventID;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Is the event valid.
        /// </summary>
        [BsonIgnore]
        public bool IsValid => !Defects.Any();

        /// <summary>
        /// Is the event invalid.
        /// </summary>
        [BsonIgnore]
        public bool IsInvalid => Defects.Any();

        /// <summary>
        ///     Id of dataset
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        ///     Name of dataset
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     List of defects
        /// </summary>
        public ICollection<EventDefect> Defects { get; set; }

        public string EventID { get; set; }

        public DateTime ModifiedDate { get; set; }

        /// <summary>
        ///     Object id
        /// </summary>
        public ObjectId Id { get; set; }
    }
}