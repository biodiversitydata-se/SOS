using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Shared
{
    public class RunInfo 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        public RunInfo(DataProvider provider)
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
        public DataProvider DataProvider { get; }

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


        public static RunInfo Success(
            DataProvider dataProvider,
            DateTime start,
            DateTime end,
            int count)
        {
            return new RunInfo(dataProvider)
            {
                Status = RunStatus.Success,
                Start = start,
                End = end,
                Count = count
            };
        }

        public static RunInfo Failed(
            DataProvider dataProvider,
            DateTime start,
            DateTime end)
        {
            return new RunInfo(dataProvider)
            {
                Status = RunStatus.Failed,
                Start = start,
                End = end
            };
        }

        public static RunInfo Cancelled(
            DataProvider dataProvider,
            DateTime start,
            DateTime end)
        {
            return new RunInfo(dataProvider)
            {
                Status = RunStatus.Canceled,
                Start = start,
                End = end
            };
        }

    }
}
