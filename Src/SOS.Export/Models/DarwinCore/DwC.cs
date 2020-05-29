using System;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    ///     This class contains information about a species
    ///     observation in Darwin Core 1.5 compatible format.
    ///     Further information about the properties can
    ///     be found at http://rs.tdwg.org/dwc/terms/.
    /// </summary>
    public class DwC
    {
        /// <summary>
        ///     Darwin Core term name: dcterms:accessRights.
        ///     Information about who can access the resource or
        ///     an indication of its security status.
        ///     Access Rights may include information regarding
        ///     access or restrictions based on privacy, security,
        ///     or other policies.
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        ///     Darwin Core term name: basisOfRecord.
        ///     The specific nature of the data record -
        ///     a subtype of the dcterms:type.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Darwin Core Type Vocabulary
        ///     (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        ///     In Species Gateway this property has the value
        ///     HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:bibliographicCitation.
        ///     A bibliographic reference for the resource as a statement
        ///     indicating how this record should be cited (attributed)
        ///     when used.
        ///     Recommended practice is to include sufficient
        ///     bibliographic detail to identify the resource as
        ///     unambiguously as possible.
        /// </summary>
        public string BibliographicCitation { get; set; }

        /// <summary>
        ///     Darwin Core term name: collectionCode.
        ///     The name, acronym, coden, or initialism identifying the
        ///     collection or data set from which the record was derived.
        /// </summary>
        public string CollectionCode { get; set; }

        /// <summary>
        ///     Darwin Core term name: collectionID.
        ///     An identifier for the collection or dataset from which
        ///     the record was derived.
        ///     For physical specimens, the recommended best practice is
        ///     to use the identifier in a collections registry such as
        ///     the Biodiversity Collections Index
        ///     (http://www.biodiversitycollectionsindex.org/).
        /// </summary>
        public string CollectionID { get; set; }

        /// <summary>
        ///     Darwin Core term name: dataGeneralizations.
        ///     Actions taken to make the shared data less specific or
        ///     complete than in its original form.
        ///     Suggests that alternative data of higher quality
        ///     may be available on request.
        /// </summary>
        public string DataGeneralizations { get; set; }

        /// <summary>
        ///     Darwin Core term name: datasetID.
        ///     An identifier for the set of data.
        ///     May be a global unique identifier or an identifier
        ///     specific to a collection or institution.
        /// </summary>
        public string DatasetID { get; set; }

        /// <summary>
        ///     Darwin Core term name: datasetName.
        ///     The name identifying the data set
        ///     from which the record was derived.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     Darwin Core term name: dynamicProperties.
        ///     A list (concatenated and separated) of additional
        ///     measurements, facts, characteristics, or assertions
        ///     about the record. Meant to provide a mechanism for
        ///     structured content such as key-value pairs.
        /// </summary>
        public string DynamicProperties { get; set; }

        /// <summary>
        ///     Darwin Core term name: informationWithheld.
        ///     Additional information that exists, but that has
        ///     not been shared in the given record.
        /// </summary>
        public string InformationWithheld { get; set; }

        /// <summary>
        ///     Darwin Core term name: institutionCode.
        ///     The name (or acronym) in use by the institution
        ///     having custody of the object(s) or information
        ///     referred to in the record.
        /// </summary>
        public string InstitutionCode { get; set; }

        /// <summary>
        ///     Darwin Core term name: institutionID.
        ///     An identifier for the institution having custody of
        ///     the object(s) or information referred to in the record.
        /// </summary>
        public string InstitutionID { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:language.
        ///     A language of the resource.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as RFC 4646 [RFC4646].
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     A legal document giving official permission to do something with the resource.
        /// </summary>
        /// <example>
        ///     http://creativecommons.org/publicdomain/zero/1.0/legalcode,
        ///     http://creativecommons.org/licenses/by/4.0/legalcode
        /// </example>
        public string License { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:modified.
        ///     The most recent date-time on which the resource was changed.
        ///     For Darwin Core, recommended best practice is to use an
        ///     encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        ///     Darwin Core term name: ownerInstitutionCode.
        ///     The name (or acronym) in use by the institution having
        ///     ownership of the object(s) or information referred
        ///     to in the record.
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:references.
        ///     A related resource that is referenced, cited,
        ///     or otherwise pointed to by the described resource.
        /// </summary>
        public string References { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:rights.
        ///     Information about rights held in and over the resource.
        ///     Typically, rights information includes a statement
        ///     about various property rights associated with the resource,
        ///     including intellectual property rights.
        /// </summary>
        public string Rights { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:rightsHolder.
        ///     A person or organization owning or
        ///     managing rights over the resource.
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        ///     Darwin Core term name: dcterms:type.
        ///     The nature or genre of the resource.
        ///     For Darwin Core, recommended best practice is
        ///     to use the name of the class that defines the
        ///     root of the record.
        /// </summary>
        public string Type { get; set; }
    }
}