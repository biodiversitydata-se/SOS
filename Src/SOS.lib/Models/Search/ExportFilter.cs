namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Filter for export
    /// </summary>
    public class ExportFilter : FilterBase
    {
        public new ExportFilter Clone()
        {
            var exportFilter = (ExportFilter)MemberwiseClone();
            return exportFilter;
        }
    }
}