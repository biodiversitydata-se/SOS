using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    /// Darwin Core Resource Relationship exension
    /// </summary>
    public class DwcResourceRelationship
    {
        public string ResourceRelationshipID { get; set; }
        public string RelatedResourceID { get; set; }
        public string RelationshipOfResource { get; set; }
        public string RelationshipAccordingTo { get; set; }
        public string RelationshipEstablishedDate { get; set; }
        public string RelationshipRemarks { get; set; }
        public string ScientificName { get; set; }
    }
}
