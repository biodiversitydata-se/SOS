﻿using Cronos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Converters;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    ///     Data provider.
    /// </summary>
    public class DataProvider : IEntity<int>, IIdIdentifierTuple
    {        
        /// <summary>
        /// Categories
        /// </summary>
        public IEnumerable<DataProviderCategory> Categories { get; set; }

        /// <summary>
        ///     Contact person.
        /// </summary>
        public ContactPerson ContactPerson { get; set; }

        /// <summary>
        /// Default coordinate uncertainty in meters
        /// </summary>
        public int CoordinateUncertaintyInMeters { get; set; }

        /// <summary>
        ///     Some providers have multiple dataset 
        /// </summary>
        public IEnumerable<DataProviderDataset> Datasets { get; set; }

        /// <summary>
        ///     Default verification status
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public ValidationStatusId? DefaultVerificationStatus { get; set; }

        /// <summary>
        ///     Descriptions of the data provider 
        /// </summary>
        public IEnumerable<VocabularyValueTranslation> Descriptions { get; set; }

        /// <summary>
        /// Indicates that failure in harvest for this provider will stop job from processing
        /// </summary>
        public bool HarvestFailPreventProcessing { get; set; }

        /// <summary>
        /// Cron expression to calculate next run
        /// </summary>
        public string HarvestSchedule { get; set; }

        /// <summary>
        ///     Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     A unique identifer that is easier to understand than an Id number.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        ///     Decides whether the data provider should be included in search when no data provider filter is set.
        /// </summary>
        public bool IncludeInSearchByDefault { get; set; }


        /// <summary>
        /// Use this provider in healthy check
        /// </summary>
        public bool IncludeInHealthCheck { get; set; }

        /// <summary>
        ///     Decides whether the data provider should be included in scheduled harvest.
        /// </summary>
        public bool IncludeInScheduledHarvest { get; set; }

        /// <summary>
        ///     Decides whether the data provider should be included in processing of observations and available for the search
        ///     API.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Hash of latest uploaded file
        /// </summary>
        public string LatestUploadedFileHash { get; set; }

        /// <summary>
        ///     The names of the data provider 
        /// </summary>
        public IEnumerable<VocabularyValueTranslation> Names { get; set; }

        /// <summary>
        ///     The organizations 
        /// </summary>
        public IEnumerable<VocabularyValueTranslation> Organizations { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DataProviderPath> Paths { get; set; }

        /// <summary>
        /// Minimum amount processed observations in % compared to prevoius run. 0 = disable check
        /// </summary>
        public byte? PreviousProcessLimit { get; set; }

        /// <summary>
        /// Time stamp according to data source, used to see if data set has changed
        /// </summary>
        public DateTime? SourceDate { get; set; }

        /// <summary>
        /// True if checklist harvest is supported
        /// </summary>
        public bool SupportChecklists { get; set; } = false;

        /// <summary>
        /// True if dataset harvest is supported
        /// </summary>
        public bool SupportDatasets { get; set; }

        /// <summary>
        /// True if event harvest is supported
        /// </summary>
        public bool SupportEvents { get; set; }

        /// <summary>
        /// A event based DwC will be created when provider data i processed if this is set to true
        /// </summary>
        public bool CreateEventDwC { get; set; }

        /// <summary>
        /// Get identifier used for checklists
        /// </summary>
        [JsonIgnore]
        public string ChecklistIdentifier => $"{Identifier}-Checklist";

        /// <summary>
        /// Support dynamic update of eml meta data
        /// </summary>
        public bool SupportDynamicEml { get; set; }

        /// <summary>
        /// True if provider support incremental harvest
        /// </summary>
        public bool SupportIncrementalHarvest { get; set; }

        /// <summary>
        ///     The harvest data format.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public DataProviderType Type { get; set; }

        /// <summary>
        ///     URL to the data provider source.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Use verbatim file as export file
        /// </summary>
        public bool UseVerbatimFileInExport { get; set; }

        /// <summary>
        ///  Indicates that provider is ready to harvest
        /// </summary>
        /// <param name="lastSuccessfulHarvest"></param>
        /// <returns></returns>
        public bool IsReadyToHarvest(DateTime? lastSuccessfulHarvest)
        {
            var nextOccurrence = NextHarvestFrom(lastSuccessfulHarvest);

            if (!nextOccurrence.HasValue)
            {
                return false;
            }

            return IsActive && DateTime.UtcNow > nextOccurrence;
        }

        /// <summary>
        /// Get time when next harvest can run 
        /// </summary>
        /// <param name="lastSuccessfulHarvest"></param>
        /// <returns></returns>
        public DateTime? NextHarvestFrom(DateTime? lastSuccessfulHarvest)
        {
            var expression = CronExpression.Parse(string.IsNullOrEmpty(HarvestSchedule) ? "* * * * *" : HarvestSchedule);

            return expression.GetNextOccurrence(lastSuccessfulHarvest?.ToUniversalTime() ?? DateTime.MinValue.ToUniversalTime());
        }

        public bool EqualsIdOrIdentifier(string idOrIdentifier)
        {
            if (int.TryParse(idOrIdentifier, out var id))
            {
                return Id == id;
            }

            return Identifier.Equals(idOrIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            if (Names == null)
                return $"[Id={Id}, Identfier={Identifier}]";
            else
                return $"[Id={Id}, Identfier={Identifier}] - {Names.Translate("en-GB")}";
        }

        protected bool Equals(DataProvider other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataProvider)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Data provider for all data providers combined.
        /// </summary>
        public static DataProvider CompleteSosDataProvider =>
            new DataProvider
            {
                Id = -1,
                Identifier = "SosDataProvidersCombined",
                Names = new List<VocabularyValueTranslation>(),
                Descriptions = new[]
                {
                    new VocabularyValueTranslation
                    {
                        CultureCode = "en-GB",
                        Value = "This is the DwC-A for all available data providers in Species Observation System (SOS)"
                    }
                },
                Organizations = new List<VocabularyValueTranslation>(),
                ContactPerson = new ContactPerson
                {
                    FirstName = "",
                    LastName = "",
                    Email = ""
                },
                Url = "",
                Datasets = null
            };

        /// <summary>
        /// Data provider for a subset of observations created by a filter.
        /// </summary>
        public static DataProvider FilterSubsetDataProvider =>
            new DataProvider()
            {
                Id = -2,
                Identifier = "SosFilterSubset",
                Names = new List<VocabularyValueTranslation>(),
                Descriptions = new[]
                {
                    new VocabularyValueTranslation
                    {
                        CultureCode = "en-GB",
                        Value = "This data has been produced with a filter and is a subset of the data available in Species Observation System (SOS)"
                    }
                },
                Organizations = new List<VocabularyValueTranslation>(),
                ContactPerson = new ContactPerson
                {
                    FirstName = "",
                    LastName = "",
                    Email = ""
                },
                Url = "",
                Datasets = null
            };
    }
}