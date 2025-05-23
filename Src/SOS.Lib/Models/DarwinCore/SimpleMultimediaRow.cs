﻿namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    ///     Simple Multimedia extension row
    ///     http://rs.gbif.org/extension/gbif/1.0/multimedia.xml
    /// </summary>
    public class SimpleMultimediaRow
    {
        public string EventId { get; set; }
        public string OccurrenceId { get; set; }
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