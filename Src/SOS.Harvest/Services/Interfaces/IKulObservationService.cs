﻿using System.Xml.Linq;

namespace SOS.Harvest.Services.Interfaces
{
    public interface IKulObservationService
    {
        /// <summary>
        /// Get KUL observations from specified id.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="changeId"></param>
        /// <returns></returns>
        Task<XDocument> GetAsync(DateTime startDate, DateTime endDate, long changeId);
    }
}