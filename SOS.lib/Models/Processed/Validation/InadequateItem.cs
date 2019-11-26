using System.Collections.Generic;
using MongoDB.Bson;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Validation
{
    /// <summary>
    /// Inadequate Darwin core
    /// </summary>
    public class InadequateItem : IEntity<ObjectId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="datasetID"></param>
        /// <param name="datasetName"></param>
        /// <param name="occurrenceID"></param>
        public InadequateItem(string datasetID, string datasetName, string occurrenceID)
        {
            DatasetID = datasetID;
            DatasetName = datasetName;
            Defects = new List<string>();
            OccurrenceID = occurrenceID;
        }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        /// Name of data set
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// List of defects
        /// </summary>
        public ICollection<string> Defects { get; set; }

        /// <summary>
        /// Object id
        /// </summary>
        public ObjectId Id { get; set; }

        public string OccurrenceID { get; set; }
    }
}
