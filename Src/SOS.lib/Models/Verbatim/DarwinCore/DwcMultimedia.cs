namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    ///     Simple Multimedia extension
    ///     http://rs.gbif.org/extension/gbif/1.0/multimedia.xml
    /// </summary>
    public class DwcMultimedia
    {
        public string Type { get; set; }
        public string Format { get; set; }
        public string Identifier { get; set; }
        public string References { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Created { get; set; }
        public string Creator { get; set; }
        public string Contributor { get; set; }
        public string Publisher { get; set; }
        public string Audience { get; set; }
        public string Source { get; set; }
        public string License { get; set; }
        public string RightsHolder { get; set; }
        public string DatasetID { get; set; }
    }
}