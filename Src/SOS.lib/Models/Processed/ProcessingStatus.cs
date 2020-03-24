using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Processed
{
    public class ProcessingStatus 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        protected ProcessingStatus(ObservationProvider provider)
        {
            DataProvider = provider;
        }

        /// <summary>
        /// Number of items
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Id of data provider
        /// </summary>
        public ObservationProvider DataProvider { get; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Running status
        /// </summary>
        public RunStatus Status { get; set; }


        public static ProcessingStatus Success(
            ObservationProvider dataProvider,
            DateTime start,
            DateTime end,
            int count)
        {
            return new ProcessingStatus(dataProvider)
            {
                Status = RunStatus.Success,
                Start = start,
                End = end,
                Count = count
            };
        }

        public static ProcessingStatus Failed(
            ObservationProvider dataProvider,
            DateTime start,
            DateTime end)
        {
            return new ProcessingStatus(dataProvider)
            {
                Status = RunStatus.Failed,
                Start = start,
                End = end
            };
        }

        public static ProcessingStatus Cancelled(
            ObservationProvider dataProvider,
            DateTime start,
            DateTime end)
        {
            return new ProcessingStatus(dataProvider)
            {
                Status = RunStatus.Canceled,
                Start = start,
                End = end
            };
        }

    }
}
