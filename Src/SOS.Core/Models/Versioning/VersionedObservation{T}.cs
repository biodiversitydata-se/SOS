using System;
using System.Collections.Generic;
using MongoDB.Bson;
using SOS.Core.Models.Observations;

namespace SOS.Core.Models.Versioning
{
    public class VersionedObservation<T> where T : class, IObservationKey
    {
        public ObjectId Id { get; set; }
        public int DataProviderId { get; set; }
        public string CatalogNumber { get; set; }
        public string CompositeId { get; set; }

        public T Current { get; set; }

        public DateTime UtcDate { get; set; }

        //[BsonElement("v")]
        public int Version { get; set; }

        public bool IsDeleted { get; set; }

        //[BsonElement("t")]
        public string Type { get; set; }

        public List<VersionHistory> Prev { get; set; }

        public VersionedObservation(T current)
        {
            Current = current;
            UtcDate = DateTime.UtcNow;
            Version = 1;
            IsDeleted = false;
            Prev = new List<VersionHistory>();
            Type = current.GetType().ToString();
            DataProviderId = current.DataProviderId;
            CatalogNumber = current.CatalogNumber;
            CompositeId = $"{current.DataProviderId}_{current.CatalogNumber}";
            //Type = current.GetType().AssemblyQualifiedName;
        }

    }
}
