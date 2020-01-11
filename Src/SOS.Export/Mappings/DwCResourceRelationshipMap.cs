using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    /// Mapping of Darwin Core to csv
    /// </summary>
    public class DwCResourceRelationshipMap : ClassMap<DwCResourceRelationship>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DwCResourceRelationshipMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreID");
            Map(m => m.ResourceRelationshipID).Index(1).Name("resourceRelationshipID");
            Map(m => m.ResourceID).Index(2).Name("resourceID");
            Map(m => m.RelatedResourceID).Index(3).Name("relatedResourceID");
            Map(m => m.RelationshipOfResource).Index(4).Name("relationshipOfResource");
            Map(m => m.RelationshipAccordingTo).Index(5).Name("relationshipAccordingTo");
            Map(m => m.RelationshipEstablishedDate).Index(6).Name("relationshipEstablishedDate");
            Map(m => m.RelationshipRemarks).Index(7).Name("relationshipRemarks");
        }
    }
}