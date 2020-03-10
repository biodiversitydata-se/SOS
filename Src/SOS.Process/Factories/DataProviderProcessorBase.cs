using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Factories
{
    public abstract class DataProviderProcessorBase<TEntity> : ProcessBaseFactory<TEntity>
    {
        protected readonly IFieldMappingResolverHelper FieldMappingResolverHelper;
        public abstract DataProvider DataProvider { get; }

        protected DataProviderProcessorBase(
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<TEntity> logger) : base(processedSightingRepository, logger)
        {
            FieldMappingResolverHelper = fieldMappingResolverHelper ?? throw new ArgumentNullException(nameof(fieldMappingResolverHelper));
        }

        protected int CommitBatch(ICollection<ProcessedSighting> sightings)
        {
            int sightingsCount = sightings.Count;
            FieldMappingResolverHelper.ResolveFieldMappedValues(sightings);
            ProcessRepository.AddManyAsync(sightings);
            sightings.Clear();
            return sightingsCount;
        }

        protected bool IsBatchFilledToLimit(int count)
        {
            return count % ProcessRepository.BatchSize == 0;
        }
    }
}