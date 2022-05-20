namespace SOS.Observations.Api.Dtos.Checklist
{
    public class CheckListInternalDto : CheckListDto
    {
        /// <summary>
        /// Values used internal in Artportalen
        /// </summary>
        public ApInternalDto ArtportalenInternal { get; set; }
    }
}