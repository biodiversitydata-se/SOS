namespace SOS.Process.Models.Processed
{
    /// <summary>
    /// A relationship of one rdfs:Resource (http://www.w3.org/2000/01/rdf-schema#Resource) to another.
    /// Resources can be thought of as identifiable records or instances of classes and may include, but need not be limited to dwc:Occurrence,
    /// dwc:Organism, dwc:MaterialSample, dwc:Event, dwc:Location, dwc:GeologicalContext, dwc:Identification, or dwc:Taxon.
    /// </summary>
    /// <example>
    /// An instance of an Organism is the mother of another instance of an Organism. A uniquely identified Occurrence represents the same Occurrence as another uniquely identified Occurrence.
    /// A MaterialSample is a subsample of another MaterialSample.
    /// </example>
    public class DarwinCoreResourceRelationship
    {
        /// <summary>
        /// An identifier for the resource that is the subject of the relationship.
        /// </summary>
        public string ResourceID { get; set; }

        /// <summary>
        /// An identifier for a related resource (the object, rather than the subject of the relationship).
        /// </summary>
        /// <example>
        /// dc609808-b09b-11e8-96f8-529269fb1459
        /// </example>
        public string RelatedResourceID { get; set; }

        /// <summary>
        /// The source (person, organization, publication, reference) establishing the relationship between the two resources.
        /// </summary>
        /// <example>
        /// Julie Woodruff
        /// </example>
        public string RelationshipAccordingTo { get; set; }

        /// <summary>
        /// The date-time on which the relationship between the two resources was established.
        /// </summary>
        /// <example>
        /// 1963-03-08T14:07-0600 (8 Mar 1963 at 2:07pm in the time zone six hours earlier than UTC).
        /// 2009-02-20T08:40Z (20 February 2009 8:40am UTC).
        /// 2018-08-29T15:19 (3:19pm local time on 29 August 2018).
        /// 1809-02-12 (some time during 12 February 1809).
        /// 1906-06 (some time in June 1906).
        /// 1971 (some time in the year 1971).
        /// 2007-03-01T13:00:00Z/2008-05-11T15:30:00Z (some time during the interval between 1 March 2007 1pm UTC and 11 May 2008 3:30pm UTC).
        /// 1900/1909 (some time during the interval between the beginning of the year 1900 and the end of the year 1909).
        /// 2007-11-13/15 (some time in the interval between 13 November 2007 and 15 November 2007).
        /// </example>
        public string RelationshipEstablishedDate { get; set; }

        /// <summary>
        /// The relationship of the resource identified by relatedResourceID to the subject (optionally identified by the resourceID).
        /// </summary>
        /// <example>
        /// sameAs, duplicate of, mother of, endoparasite of, host to, sibling of, valid synonym of, located within
        /// </example>
        public string RelationshipOfResource { get; set; }

        /// <summary>
        /// Comments or notes about the relationship between the two resources.
        /// </summary>
        /// <example>
        /// mother and offspring collected from the same nest, pollinator captured in the act
        /// </example>
        public string RelationshipRemarks { get; set; }

        /// <summary>
        /// An identifier for an instance of relationship between one resource (the subject) and another (relatedResource, the object).
        /// </summary>
        /// <example>
        /// 04b16710-b09c-11e8-96f8-529269fb1459
        /// </example>
        public string ResourceRelationshipID { get; set; }
    }
}
