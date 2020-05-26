using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Lib.Models.Verbatim.Shared
{
    public class HarvestInfo : IEntity<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="provider"></param>
        /// <param name="start"></param>
        public HarvestInfo(string id, DataProviderType provider, DateTime start)
        {
            DataProvider = provider;
            Id = id;
            Start = start;
        }

        /// <summary>
        /// Number of items
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Id of data provider
        /// </summary>
        public DataProviderType DataProvider { get; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Running status
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public RunStatus Status { get; set; }

        public static string GetIdFromDataProvider(DataProvider dataProvider)
        {
            switch (dataProvider.Type)
            {
                case DataProviderType.DwcA:
                    return $"{nameof(DwcObservationVerbatim)}-{dataProvider.Identifier}";
                case DataProviderType.ArtportalenObservations:
                    return nameof(ArtportalenVerbatimObservation);
                case DataProviderType.ClamPortalObservations:
                    return nameof(ClamObservationVerbatim);
                case DataProviderType.SharkObservations:
                    return nameof(SharkObservationVerbatim);
                case DataProviderType.KULObservations:
                    return nameof(KulObservationVerbatim);
                case DataProviderType.NorsObservations:
                    return nameof(NorsObservationVerbatim);
                case DataProviderType.SersObservations:
                    return nameof(SersObservationVerbatim);
                case DataProviderType.MvmObservations:
                    return nameof(MvmObservationVerbatim);
                case DataProviderType.VirtualHerbariumObservations:
                    return nameof(VirtualHerbariumObservationVerbatim);
                default:
                    return dataProvider.Type.ToString();
            }
        }
    }
}
