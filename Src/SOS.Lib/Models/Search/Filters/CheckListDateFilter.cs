﻿using System;

namespace SOS.Lib.Models.Search.Filters
{
    /// <summary>
    /// Date related filter
    /// </summary>
    public class ChecklistDateFilter : DateFilter
    {
        /// <summary>
        /// Minimum time spent to look for taxa
        /// </summary>
        public TimeSpan MinEffortTime { get; set; }
    }
}