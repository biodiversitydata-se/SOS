using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    /// Audubon media description extension
    /// http://rs.gbif.org/extension/ac/audubon.xml
    /// </summary>
    public class DwcAudubonMedia
    {
        /// <summary>
        /// An arbitrary code that is unique for the resource, with the resource being either a provider, collection, or media item.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// dc:type may take as value any type term from the DCMI Type Vocabulary. Recommended terms are Collection, StillImage, Sound, MovingImage, InteractiveResource, Text. Values may be used either in their literal form, or with a full namespace (e. g. http://purl.org/dc/dcmitype/StillImage) from a controlled vocabulary, but the best practice is to use the literal form when using dc:type and use dcterms:type when you can supply the URI from a controlled vocabulary and implementers may require this practice. At least one of dc:type and dcterms:type must be supplied but, when feasible, supplying both may make the metadata more widely useful. The values of each should designate the same type, but in case of ambiguity dcterms:type prevails.
        /// </summary>
        public string TypeAc { get; set; }

        /// <summary>
        /// A full URI preferably from among the type URIs specified in the DCMI Type Vocabulary. Recommended terms are those URIs whose labels are Collection, StillImage, Sound, MovingImage, InteractiveResource, or Text (e.g. http://purl.org/dc/dcmitype/Collection). Also recommended are the full URIs of ac:PanAndZoomImage, ac:3DStillImage, and ac: 3DMovingImage. Values MUST NOT be a string, but a URI with full namespace (e. g. http://purl.org/dc/dcmitype/StillImage) from a controlled vocabulary. Implementers and communities of practice may determine whether specific controlled vocabularies must be used. If the resource is a Collection, this item does not identify what types of objects it may contain. Following the DC recommendations at http://purl.org/dc/dcmitype/Text, images of text should be with this URI.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The subtype should provide more specialization than the type. Possible values are community-defined. For exmamples see the non-normative page AC_Subtype_Examples.
        /// </summary>
        public string SubtypeLiteral { get; set; }

        /// <summary>
        /// Any URI may be used that provides for more specialization than the type. Possible values are community-defined. For exmamples see the non-normative page AC_Subtype_Examples.
        /// </summary>
        public string Subtype { get; set; }

        /// <summary>
        /// Concise title, name, or brief descriptive label of institution, resource collection, or individual resource. This field should include the complete title with all the subtitles, if any.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Date that the media resource was altered. The date and time must comply with the World Wide Web Consortium (W3C) datetime practice, which requires that date and time representation correspond to ISO 8601:1998, but with year fields always comprising 4 digits. This makes datetime records compliant with 8601:2004. AC datetime values may also follow 8601:2004 for ranges by separating two IS0 8601 datetime fields by a solidus (forward slash, '/'). See also the wikipedia IS0 8601 entry for further explanation and examples.
        /// </summary>
        public string Modified { get; set; }

        /// <summary>
        /// Point in time recording when the last modification to metadata (not necessarily the media object itself) occurred. The date and time must comply with the World Wide Web Consortium (W3C) datetime practice, which requires that date and time representation correspond to ISO 8601:1998, but with year fields always comprising 4 digits. This makes datetime records compliant with 8601:2004. AC datetime values may also follow 8601:2004 for ranges by separating two IS0 8601 datetime fields by a solidus (forward slash, '/'). See also the wikipedia IS0 8601 entry for further explanation and examples.
        /// </summary>
        public string MetadataDate { get; set; }

        /// <summary>
        /// Language of description and other metadata (but not necessarily of the image itself) represented as an ISO639-2 three letter language code. ISO639-1 two-letter codes are permitted but deprecated. Note: At least one of ac:metadataLanguage and ac:metadataLanguageLiteral must be supplied but, when feasible, supplying both may make the metadata more widely useful.
        /// </summary>
        public string MetadataLanguageLiteral { get; set; }

        /// <summary>
        /// URI from the ISO639-2 list of URIs for ISO 3-letter language codes. Note: At least one of ac:metadataLanguage and ac:metadataLanguageLiteral must be supplied but, when feasible, supplying both may make the metadata more widely useful
        /// </summary>
        public string MetadataLanguage { get; set; }

        /// <summary>
        /// A free-form identifier (a simple number, an alphanumeric code, a URL, etc.) that is unique and meaningful primarily for the data provider.
        /// </summary>
        public string ProviderManagedID { get; set; }

        /// <summary>
        /// A rating of the media resources, provided by record originators or editors, with -1 defining “rejected”, “0” defining “unrated”, and “1” (worst) to “5” (best).
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// A name or the literal anonymous (= anonymously commented).
        /// </summary>
        public string CommenterLiteral { get; set; }

        /// <summary>
        /// A URI denoting a person, using some controlled vocabulary such as FOAF. Implementers and communities of practice may produce restrictions or recommendations on the choice of vocabularies. See also the entry for ac:commenterLiteral in this document and the section Namespaces, Prefixes and Term Names for discussion of the rationale for separate terms taking URI values from those taking Literal values where both are possible. Normal practice is to use the same Label if both are provided. Labels have no effect on information discovery and are only suggestions.
        /// </summary>
        public string Commenter { get; set; }

        /// <summary>
        /// Any comment provided on the media resource, as free-form text. Best practice would also identify the commenter.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// String providing the name of a reviewer. If present, then resource is peer-reviewed, even if Reviewer Comments is absent or empty. Its presence tells whether an expert in the subject featured in the media has reviewed the media item or collection and approved its metadata description; must display a name or the literal anonymous (= anonymously reviewed).
        /// </summary>
        public string ReviewerLiteral { get; set; }

        /// <summary>
        /// URI for a reviewer. If present, then resource is peer-reviewed, even if there are Reviewer Comments is absent or empty. Its presence tells whether an expert in the subject featured in the media has reviewed the media item or collection and approved its metadata description; must display a name or the literal anonymous (= anonymously reviewed).
        /// </summary>
        public string Reviewer { get; set; }

        /// <summary>
        /// Any comment provided by a reviewer with expertise in the subject, as free-form text.
        /// </summary>
        public string ReviewerComments { get; set; }

        /// <summary>
        /// The date (often a range) that the resource became or will become available. The date and time must comply with the World Wide Web Consortium (W3C) datetime practice, which requires that date and time representation correspond to ISO 8601:1998, but with year fields always comprising 4 digits. This makes datetime records compliant with 8601:2004. AC datetime values may also follow 8601:2004 for ranges by separating two IS0 8601 datetime fields by a solidus (forward slash, '/'). See also the wikipedia IS0 8601 entry for further explanation and examples.
        /// </summary>
        public string Available { get; set; }

        /// <summary>
        /// In a chosen serialization (RDF, XML Schema, etc.) the potentially multiple service access points (e.g., for different resolutions of an image) might be provided in a referenced or in a nested object. This property identifies one such access point. That is, each of potentially multiple values of hasServiceAccessPoint identifies a set of representation-dependent metadata using the properties defined under the section Service Access Point Vocabulary.
        /// </summary>
        public string HasServiceAccessPoint { get; set; }

        /// <summary>
        /// Information about rights held in and over the resource. A full-text, readable copyright statement, as required by the national legislation of the copyright holder. On collections, this applies to all contained objects, unless the object itself has a different statement. Examples: “Copyright XY 2008, all rights reserved”, “© 2008 XY Museum” , Public Domain., Copyright unknown. Do not place just the name of the copyright holder(s) here! That belongs in a list in the xmpRights:Owner field, which should be supplied if dc:rights is not 'Public Domain', which is appropriate only if the resource is known to be not under copyright. See also the entry for dcterms:rights in this document and see the DCMI FAQ on DC and DCTERMS Namespaces for discussion of the rationale for terms in two namespaces. Normal practice is to use the same Label if both are provided. Labels have no effect on information discovery and are only suggestions.
        /// </summary>
        public string RightsAc { get; set; }

        /// <summary>
        /// A URI pointing to structured information about rights held in and over the resource. Examples include http://creativecommons.org/licenses/by/3.0/legalcode and http://creativecommons.org/publicdomain/zero/1.0/. At least one of dcterms:rights and dc:rights must be supplied but, when feasible, supplying both may make the metadata more widely useful. They must specify the same rights. In case of ambiguity, dcterms:rights prevails.
        /// </summary>
        public string Rights { get; set; }

        /// <summary>
        /// A list of the names of the owners of the copyright. 'Unknown' is an acceptable value, but 'Public Domain' is not.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The license statement defining how resources may be used. Information on a collection applies to all contained objects unless the object has a different statement.
        /// </summary>
        public string UsageTerms { get; set; }

        /// <summary>
        /// A URL defining or further elaborating on the license statement (e. g., a web page explaining the precise terms of use).
        /// </summary>
        public string WebStatement { get; set; }

        /// <summary>
        /// A URL providing access to a logo that symbolizes the License.
        /// </summary>
        public string LicenseLogoURL { get; set; }

        /// <summary>
        /// free text for Please cite this as…
        /// </summary>
        public string Credit { get; set; }

        /// <summary>
        /// The URL of the icon or logo image to appear in source attribution.
        /// </summary>
        public string AttributionLogoURL { get; set; }

        /// <summary>
        /// The URL where information about ownership, attribution, etc. of the resource may be found.
        /// </summary>
        public string AttributionLinkURL { get; set; }

        /// <summary>
        /// Organizations or individuals who funded the creation of the resource.
        /// </summary>
        public string FundingAttribution { get; set; }

        /// <summary>
        /// A string providing an identifiable source from which the described resources was derived.
        /// </summary>
        public string SourceAc { get; set; }

        /// <summary>
        /// URI for an identifiable source from which the described resources was derived.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The person or organization responsible for creating the media resource.
        /// </summary>
        public string CreatorAc { get; set; }

        /// <summary>
        /// A URI representing the person or organization responsible for creating the media resource.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// Person or organization responsible for presenting the media resource. If no separate Metadata Provider is given, this also attributes the metadata.
        /// </summary>
        public string ProviderLiteral { get; set; }

        /// <summary>
        /// URI for person or organization responsible for presenting the media resource. If no separate Metadata Provider is given, this also attributes the metadata.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Person or organization originally creating the resource metadata record.
        /// </summary>
        public string MetadataCreatorLiteral { get; set; }

        /// <summary>
        /// Person or organization originally creating the resource metadata record.
        /// </summary>
        public string MetadataCreator { get; set; }

        /// <summary>
        /// Person or organization originally responsible for providing the resource metadata record.
        /// </summary>
        public string MetadataProviderLiteral { get; set; }

        /// <summary>
        /// URI of person or organization originally responsible for providing the resource metadata record.
        /// </summary>
        public string MetadataProvider { get; set; }

        /// <summary>
        /// Description of collection or individual resource, containing the Who, What, When, Where and Why as free-form text. This normative document is silent on the nature of formatting in the text. It is the role of implementers of an AC concrete representation (e.g., an XML Schema, an RDF representation, etc.) to decide and document how formatting advice will be represented in descriptions serialized according to such representations.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// As alternative or in addition to description, a caption is free-form text to be displayed together with (rather than instead of) a resource that is suitable for captions (especially images).
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Language(s) of resource itself represented in the ISO639-2 three-letter language code. ISO639-1 two-letter codes are permitted but deprecated.
        /// </summary>
        public string LanguageAc { get; set; }

        /// <summary>
        /// URI from the ISO639-2 list of URIs for ISO 3-letter language codes.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The setting of the content represented in media such as images, sounds, and movies if the provider deems them relevant. Constrained vocabulary of: Natural = Object in its natural setting of the object (e. g. living organisms in their natural environment); Artificial = Object in an artificial environment (e. g. living organisms in an artificial environment such as a zoo, garden, greenhouse, or laboratory); Edited = Human media editing of a natural setting or media acquisition with a separate media background such as a photographic backdrop.
        /// </summary>
        public string PhysicalSetting { get; set; }

        /// <summary>
        /// Controlled vocabulary of subjects to support broad classification of media items. Terms from various controlled vocabularies may be used. AC-recommended vocabularies are preferred and may be unqualified literals (not a full URI). For terms from other vocabularies either a precise URI should be used, or, as long as all unqualified terms in all vocabularies are unique, metadata should provide the source vocabularies using the Subject Category Vocabulary term.
        /// </summary>
        public string CVterm { get; set; }

        /// <summary>
        /// Any vocabulary or formal classification from which terms in the Subject Category have been drawn.
        /// </summary>
        public string SubjectCategoryVocabulary { get; set; }

        /// <summary>
        /// General keywords or tags.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The location that is depicted the media content, irrespective of the location at which the resource has been created.
        /// </summary>
        public string LocationShown { get; set; }

        /// <summary>
        /// Name of a world region in some high level classification, such as names for continents, waterbodies, or island groups, whichever is most appropriate. The terms preferably are derived from a controlled vocabulary.
        /// </summary>
        public string WorldRegion { get; set; }

        /// <summary>
        /// The geographic location of the specific entity or entities documented by the media item, expressed through a constrained vocabulary of countries using ISO 3166-1-alpha-2 2-letter country codes (e. g. IT, SI).
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// This field can be free text, but where possible, the use of http://iptc.org/std/Iptc4xmpExt/2008-02-29/CountryCode is preferred.
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        /// Optionally, the geographic unit immediately below the country level (individual states in federal countries, provinces, or other administrative units) in which the subject of the media resource (e. g., species, habitats, or events) were located (if such information is available in separate fields).
        /// </summary>
        public string ProvinceState { get; set; }

        /// <summary>
        /// Optionally, the name of a city or place commonly found in gazetteers (such as a mountain or national park) in which the subjects (e. g., species, habitats, or events) were located.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Free-form text location details of the location of the subjects, down to the village, forest, or geographic feature etc., below the Iptc4xmpExt:City place name, especially information that could not be found in a gazetteer.
        /// </summary>
        public string Sublocation { get; set; }

        /// <summary>
        /// The coverage (extent or scope) of the content of the resource. Temporal coverage will typically include temporal period (a period label, date, or date range) to which the subjects of the media or media collection relate. If dates are mentioned, they should follow ISO 8601. When the resource is a Collection, this refers to the temporal coverage of the collection. Following dcterms:temporal , the value must be a URI.
        /// </summary>
        public string Temporal { get; set; }

        /// <summary>
        /// The date of the creation of the original resource from which the digital media was derived or created. The date and time must comply with the World Wide Web Consortium (W3C) datetime practice, which requires that date and time representation correspond to ISO 8601:1998, but with year fields always comprising 4 digits. This makes datetime records compliant with 8601:2004. AC datetime values may also follow 8601:2004 for ranges by separating two IS0 8601 datetime fields by a solidus (forward slash, '/'). See also the wikipedia IS0 8601 entry for further explanation and examples.
        /// </summary>
        public string CreateDate { get; set; }

        /// <summary>
        /// Free text information beyond exact clock times.
        /// </summary>
        public string TimeOfDay { get; set; }

        /// <summary>
        /// A higher taxon (e. g., a genus, family, or order) at the level of the genus or higher, that covers all taxa that are the primary subject of the resource (which may be a media item or a collection).
        /// </summary>
        public string TaxonCoverage { get; set; }

        /// <summary>
        /// Scientific names of taxa represented in the media resource (with date and name authorship information if available) of the lowest level taxonomic rank that can be applied.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// A brief phrase or a standard abbreviation (cf. genus, cf. species, cf. var., aff. species, etc.) to express the determiner's doubts with respect to a specified taxonomic rank about the identification given in Scientific Taxon Name.
        /// </summary>
        public string IdentificationQualifier { get; set; }

        /// <summary>
        /// Common (= vernacular) names of the subject in one or several languages. The ISO language name should be given in parentheses after the name if not all names are given by values of the Metadata Language term.
        /// </summary>
        public string VernacularName { get; set; }

        /// <summary>
        /// The taxonomic authority used to apply the name to the taxon, e. g., a person, book or web service.
        /// </summary>
        public string NameAccordingTo { get; set; }

        /// <summary>
        /// An identifier for the nomenclatural (not taxonomic) details of a scientific name.
        /// </summary>
        public string ScientificNameID { get; set; }

        /// <summary>
        /// One or several Scientific Taxon Names that are synonyms to the Scientific Taxon Name may be provided here.
        /// </summary>
        public string OtherScientificName { get; set; }

        /// <summary>
        /// The name(s) of the person(s) who applied the Scientific Taxon Name to the media item or the occurrence represented in the media item.
        /// </summary>
        public string IdentifiedBy { get; set; }

        /// <summary>
        /// The date on which the person(s) given under Identfied By applied a Scientific Taxon Name to the resource.
        /// </summary>
        public string DateIdentified { get; set; }

        /// <summary>
        /// An exact or estimated number of taxa at the lowest applicable taxon rank (usually species or infraspecific) represented by the media resource (item or collection).
        /// </summary>
        public string TaxonCount { get; set; }

        /// <summary>
        /// The portion or product of organism morphology, behaviour, environment, etc. that is either predominantly shown or particularly well exemplified by the media resource.
        /// </summary>
        public string SubjectPart { get; set; }

        /// <summary>
        /// A description of the sex of any organisms featured within the media, when relevant to the subject of the media, e. g., male, female, hermaphrodite, dioecious.
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// A description of the life-cycle stage of any organisms featured within the media, when relevant to the subject of the media, e. g., larvae, juvenile, adult.
        /// </summary>
        public string LifeStage { get; set; }

        /// <summary>
        /// Specific orientation (= direction, view angle) of the subject represented in the media resource with respect to the acquisition device.
        /// </summary>
        public string SubjectOrientation { get; set; }

        /// <summary>
        /// Free form text describing the techniques used to prepare the subject of the media, prior to or while creating the media resource.
        /// </summary>
        public string Preparations { get; set; }

        /// <summary>
        /// The location at which the media recording instrument was placed when the media was created.
        /// </summary>
        public string LocationCreated { get; set; }

        /// <summary>
        /// Date the first digital version was created, if different from Original Date and Time found in the Temporal Coverage Vocabulary. The date and time must comply with the World Wide Web Consortium (W3C) datetime practice, which requires that date and time representation correspond to ISO 8601:1998, but with year fields always comprising 4 digits. This makes datetime records compliant with 8601:2004. AC datetime values may also follow 8601:2004 for ranges by separating two IS0 8601 datetime fields by a solidus (forward slash, '/'). See also the wikipedia IS0 8601 entry for further explanation and examples.
        /// </summary>
        public string DigitizationDate { get; set; }

        /// <summary>
        /// Free form text describing the device or devices used to create the resource.
        /// </summary>
        public string CaptureDevice { get; set; }

        /// <summary>
        /// Information about technical aspects of the creation and digitization process of the resource. This includes modification steps (retouching) after the initial resource capture.
        /// </summary>
        public string ResourceCreationTechnique { get; set; }

        /// <summary>
        /// If the resource is contained in a Collection, this field identifies that Collection uniquely. Its form is not specified by this normative document, but is left to implementers of specific implementations.
        /// </summary>
        public string IDofContainingCollection { get; set; }

        /// <summary>
        /// Resource related in ways not specified through a collection, e.g., before-after images; time-lapse series; different orientations/angles of view
        /// </summary>
        public string RelatedResourceID { get; set; }

        /// <summary>
        /// A globally unique ID of the provider of the current AC metadata record.
        /// </summary>
        public string ProviderID { get; set; }

        /// <summary>
        /// A reference to an original resource from which the current one is derived.
        /// </summary>
        public string DerivedFrom { get; set; }

        /// <summary>
        /// A reference to a specimen associated with this resource.
        /// </summary>
        public string AssociatedSpecimenReference { get; set; }

        /// <summary>
        /// A reference to an observation associated with this resource.
        /// </summary>
        public string AssociatedObservationReference { get; set; }

        /// <summary>
        /// A URI that uniquely identifies a service that provides a representation of the underlying resource. If this resource can be acquired by an http request, its http URL should be given. If not, but it has some URI in another URI scheme, that may be given here.
        /// </summary>
        public string AccessURI { get; set; }

        /// <summary>
        /// A string describing the technical format of the resource (file format or physical medium).
        /// </summary>
        public string FormatAc { get; set; }

        /// <summary>
        /// URI referencing the technical format of the resource (file format or physical medium).
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Text that describes this Service Access Point variant.
        /// </summary>
        public string VariantLiteral { get; set; }

        /// <summary>
        /// A URI designating what this Service Access Point provides. Some suggested values are the URIs ac:Thumbnail, ac:Trailer, ac:LowerQuality, ac:MediumQuality, ac:GoodQuality, ac:BestQuality, and ac:Offline. Additional URIs from communities of practice may be introduced.
        /// </summary>
        public string Variant { get; set; }

        /// <summary>
        /// Text that describes this Service Access Point variant
        /// </summary>
        public string VariantDescription { get; set; }

        /// <summary>
        /// The URL of a Web site that provides additional information about the version of the media resource that is provided by the Service Access Point.
        /// </summary>
        public string FurtherInformationURL { get; set; }

        /// <summary>
        /// The licensing statement for this variant of the media resource if different from that given in the License Statement property of the resource.
        /// </summary>
        public string LicensingException { get; set; }

        /// <summary>
        /// A term that describes what service expectations users may have of the ac:accessURL. Recommended terms include online (denotes that the URL is expected to deliver the resource), authenticate (denotes that the URL delivers a login or other authentication interface requiring completion before delivery of the resource) published(non digital) (denotes that the URL is the identifier of a non-digital published work, for example a doi.) Communities should develop their own controlled vocabularies for Service Expectations.
        /// </summary>
        public string ServiceExpectation { get; set; }

        /// <summary>
        /// The cryptographic hash function used to compute the value given in the Hash Value.
        /// </summary>
        public string HashFunction { get; set; }

        /// <summary>
        /// The value computed by a hash function applied to the media that will be delivered at the access point.
        /// </summary>
        public string HashValue { get; set; }

        /// <summary>
        /// The width in pixels of the media specified by the access point.
        /// </summary>
        public string PixelXDimension { get; set; }

        /// <summary>
        /// The height in pixels of the media specified by the access point.
        /// </summary>
        public string PixelYDimension { get; set; }

       
    }
}