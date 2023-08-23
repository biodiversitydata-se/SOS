
namespace SOS.Analysis.Api.Dtos.Filter
{
    public class DataStewardshipFilterDto
    {
        /// <summary>
        /// Dataset filter
        /// </summary>
        public IEnumerable<string>? DatasetIdentifiers { get; set; }
    }
}
