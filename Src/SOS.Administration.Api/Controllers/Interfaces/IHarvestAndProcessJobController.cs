﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces
{
    /// <summary>
    ///     Harvest and process job controller
    /// </summary>
    public interface IHarvestAndProcessJobController
    {
        /// <summary>
        ///     Schedule harvest and process job
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyObservationHarvestAndProcessJob(int hour, int minute);

        /// <summary>
        ///     Run harvest and process job
        /// </summary>
        /// <returns></returns>
        IActionResult RunObservationHarvestAndProcessJob();


        /// <summary>
        ///  Run incremaental harvest and processing
        /// </summary>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        IActionResult RunIncrementalObservationHarvestAndProcessJob(DateTime? fromDate);

        /// <summary>
        /// Run incremaental harvest and processing of specified Artportalen observations
        /// </summary>
        /// <returns></returns>
        IActionResult RunIncrementalArtportalenObservationHarvestAndProcessJob(List<int> ids);

        /// <summary>
        ///  Incremental harvest and processing
        /// </summary>
        /// <param name="runIntervalInMinutes"></param>
        /// <returns></returns>
        IActionResult ScheduleIncrementalObservationHarvestAndProcessJob(byte runIntervalInMinutes);

        /// <summary>
        ///     Schedule checklists harvest and process job
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <returns></returns>
        IActionResult AddDailyChecklistHarvestAndProcessJob(int hour, int minute);

        /// <summary>
        ///     Run checklists harvest and process job
        /// </summary>
        /// <returns></returns>
        IActionResult RunChecklistHarvestAndProcessJob();
    }
}