using SOS.Lib.Models.Processed.Checklist;
using SOS.Shared.Api.Dtos.Checklist;

namespace SOS.Shared.Api.Extensions.Dto
{
    public static class ChecklistExtensions
    {
        /// <summary>
        /// Cast Ap internal to dto
        /// </summary>
        /// <param name="apInternal"></param>
        /// <returns></returns>
        private static ApInternalDto ToDto(this ApInternal apInternal)
        {
            return new ApInternalDto
            {
                ChecklistId = apInternal.ChecklistId,
                ParentTaxonId = apInternal.ParentTaxonId,
                UserId = apInternal.UserId
            };
        }

        /// <summary>
        /// Cast checklist to dto
        /// </summary>
        /// <param name="checklist"></param>
        /// <returns></returns>
        public static ChecklistDto ToDto(this Checklist checklist)
        {
            return new ChecklistDto
            {
                DataProviderId = checklist.DataProviderId,
                EffortTime = !string.IsNullOrEmpty(checklist.SamplingEffortTime) || checklist.Event?.EndDate == null || checklist.Event?.StartDate == null ?
                    checklist.SamplingEffortTime : checklist.Event.EndDate.Value.Subtract(checklist.Event.StartDate.Value).ToString("g"),
                Event = checklist.Event,
                Id = checklist.Id,
                Location = checklist.Location.ToDto(),
                Modified = checklist.Modified,
                Name = checklist.Name,
                OccurrenceIds = checklist.OccurrenceIds,
                Project = checklist.Project.ToDto(),
                RecordedBy = checklist.RecordedBy,
                RegisterDate = checklist.RegisterDate,
                TaxonIds = checklist.TaxonIds,
                TaxonIdsFound = checklist.TaxonIdsFound
            };
        }

        /// <summary>
        /// Cast checklist to internal dto
        /// </summary>
        /// <param name="checklist"></param>
        /// <returns></returns>
        public static ChecklistInternalDto ToInternalDto(this Checklist checklist)
        {
            return  new ChecklistInternalDto
            {
                ArtportalenInternal = checklist.ArtportalenInternal.ToDto(),
                DataProviderId = checklist.DataProviderId,
                EffortTime = !string.IsNullOrEmpty(checklist.SamplingEffortTime) || checklist.Event?.EndDate == null || checklist.Event?.StartDate == null ?
                    checklist.SamplingEffortTime : checklist.Event.EndDate.Value.Subtract(checklist.Event.StartDate.Value).ToString("g"),
                Event = checklist.Event,
                Id = checklist.Id,
                Location = checklist.Location.ToDto(),
                Modified = checklist.Modified,
                Name = checklist.Name,
                OccurrenceIds = checklist.OccurrenceIds,
                Project = checklist.Project.ToDto(),
                RecordedBy = checklist.RecordedBy,
                RegisterDate = checklist.RegisterDate,
                TaxonIds = checklist.TaxonIds,
                TaxonIdsFound = checklist.TaxonIdsFound
            };
        }
    }
}
