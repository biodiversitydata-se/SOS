﻿using System.Collections.Generic;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Simple Multimedia extension
    ///     http://rs.gbif.org/extension/gbif/1.0/multimedia.xml
    /// </summary>
    public class Multimedia
    {
        public IEnumerable<MultimediaComment> Comments { get; set; }
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
        public override string ToString()
        {
            return $"[{nameof(Format)}: {Format}, {nameof(Identifier)}: {Identifier}, {nameof(License)}: {License}, {nameof(RightsHolder)}: {RightsHolder}]";
        }
    }
}