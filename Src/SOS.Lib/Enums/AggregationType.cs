﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace SOS.Lib.Enums
{
    /// <summary>
    ///     Type of aggregation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AggregationType
    {
        /// <summary>
        /// 
        /// </summary>
        SightingsPerWeek = 0,
        /// <summary>
        /// 
        /// </summary>
        SightingsPerYear = 1,
        /// <summary>
        /// 
        /// </summary>
        QuantityPerWeek = 2,
        /// <summary>
        /// 
        /// </summary>
        QuantityPerYear = 3,
        /// <summary>
        /// 
        /// </summary>
        SpeciesSightingsList = 4,
        /// <summary>
        /// 
        /// </summary>
        SpeciesSightingsListTaxonCount = 5,
        /// <summary>
        /// Special case where every month is divided in 4 weeks, total 48 weeks a year
        /// </summary>
        SightingsPerWeek48 = 6
    }

    public static class AggregationTypeExtension
    {
        public static bool IsDateHistogram(this AggregationType aggregationType)
        {
            return new List<AggregationType>
            {
                AggregationType.QuantityPerWeek,
                AggregationType.QuantityPerYear,
                AggregationType.SightingsPerWeek,
                AggregationType.SightingsPerYear
            }.Contains(aggregationType);
        }

        public static bool IsYearHistogram(this AggregationType aggregationType)
        {
            return new List<AggregationType>
            {
                AggregationType.QuantityPerYear,
                AggregationType.SightingsPerYear
            }.Contains(aggregationType);
        }

        public static bool IsWeekHistogram(this AggregationType aggregationType)
        {
            return new List<AggregationType>
            {
                AggregationType.QuantityPerWeek,
                AggregationType.SightingsPerWeek,
                AggregationType.SightingsPerWeek48
            }.Contains(aggregationType);
        }

        public static bool IsSpeciesSightingsList(this AggregationType aggregationType)
        {
            return new List<AggregationType>
            {
                AggregationType.SpeciesSightingsList,
                AggregationType.SpeciesSightingsListTaxonCount
            }.Contains(aggregationType);
        }
    }
}