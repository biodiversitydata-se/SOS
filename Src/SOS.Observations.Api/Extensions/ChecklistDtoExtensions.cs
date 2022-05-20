using SOS.Lib.Models.Processed.CheckList;
using SOS.Observations.Api.Dtos.Checklist;

namespace SOS.Observations.Api.Extensions
{
    public static class ChecklistDtoExtensions
    {
        /// <summary>
        /// Cast Ap internal to dto
        /// </summary>
        /// <param name="apInternal"></param>
        /// <returns></returns>
        private static ApInternalDto ToDto(this ApInternal apInternal)
        {
            return apInternal == null ? null : new ApInternalDto
            {
                CheckListId = apInternal.CheckListId,
                ParentTaxonId = apInternal.ParentTaxonId,
                UserId = apInternal.UserId
            };
        }

        /// <summary>
        /// Cast check list to dto
        /// </summary>
        /// <param name="checklist"></param>
        /// <returns></returns>
        public static CheckListDto ToDto(this CheckList checklist)
        {
            return checklist == null ? null : new CheckListDto
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
        /// Cast check list to internal dto
        /// </summary>
        /// <param name="checklist"></param>
        /// <returns></returns>
        public static CheckListInternalDto ToInternalDto(this CheckList checklist)
        {
            return checklist == null ? null : new CheckListInternalDto
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
