using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.ArtportalenApiService
{    
    public class SightingOutput
    {
        /// <summary>
        /// Gets or sets the activity id.
        /// </summary>
        public int? ActivityId { get; set; }

        /// <summary>
        /// Gets or sets taxon author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the biotope id
        /// </summary>
        public int? BiotopeId { get; set; }

        /// <summary>
        /// Gets or sets the biotope description
        /// </summary>
        public string BiotopeDescription { get; set; }

        /// <summary>
        /// Gets or sets taxon commonname
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        /// Gets or sets DiscoveryMethod.
        /// </summary>
        public string DiscoveryMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the sighting is editable
        /// </summary>
        public bool Editable { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the gender id.
        /// </summary>
        public int? GenderId { get; set; }

        /// <summary>
        /// Gets or sets the has images.
        /// </summary>
        public bool HasImages { get; set; }

        /// <summary>
        /// Gets or sets the hidden by provider date.
        /// </summary>
        public DateTime? HiddenByProvider { get; set; }

        /// <summary>
        /// Gets or sets Sighting Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Length.
        /// Report form sv-SE name: Längd
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets MaxDepth.
        /// </summary>
        public int? MaxDepth { get; set; }

        /// <summary>
        /// Gets or sets MaxHeight.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets MinDepth.
        /// </summary>
        public int? MinDepth { get; set; }

        /// <summary>
        /// Gets or sets MinHeight.
        /// </summary>
        public int? MinHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not present.
        /// </summary>
        public bool NotPresent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not recovered.
        /// </summary>
        public bool NotRecovered { get; set; }

        /// <summary>
        /// Gets or sets a string that represents the owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the private comment.
        /// </summary>
        public string PrivateComment { get; set; }

        /// <summary>
        /// Gets or sets the project ids.
        /// </summary>
        public List<ProjectWithParameterValues> Projects { get; set; }

        /// <summary>
        /// Gets or sets system protection
        /// </summary>
        public bool ProtectedBySystem { get; set; }

        /// <summary>
        /// Gets or sets the public comment.
        /// </summary>
        public string PublicComment { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets QuantityOfSubstrate.
        /// TODO: Discussion, will this be calculated from relation to substrate...
        /// Report form sv-SE name: Antal substrat
        /// </summary>
        public int? QuantityOfSubstrate { get; set; }

        /// <summary>
        /// Gets or sets RuleValidationMessages
        /// </summary>
        public List<RuleValidationMessage> RuleValidationMessages { get; set; }

        /// <summary>
        /// Gets or sets taxon scientificname
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// Gets or sets SightingRelations.
        /// </summary>
        public string SightingObservers { get; set; }

        /// <summary>
        /// Gets or sets SightingState.
        /// </summary>
        public string SightingState { get; set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        /// Gets or sets the stage id.
        /// </summary>
        public int? StageId { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the substrate description
        /// </summary>
        public string SubstrateDescription { get; set; }

        /// <summary>
        /// Gets or sets the substrate id
        /// </summary>
        public int? SubstrateId { get; set; }

        /// <summary>
        /// Gets or sets the substrate species description
        /// </summary>
        public string SubstrateSpeciesDescription { get; set; }

        /// <summary>
        /// Gets or sets the substrate species id
        /// </summary>
        public int? SubstrateSpeciesId { get; set; }

        /// <summary>
        /// Gets or sets the taxon id.
        /// </summary>
        public int TaxonId { get; set; }

        /// <summary>
        /// Gets or sets the taxon.
        /// </summary>
        public Taxon Taxon { get; set; }

        /// <summary>
        /// Gets or sets Unit.
        /// Report form sv-SE name: Enhet
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sighting is unspontaneous.
        /// </summary>
        public bool Unspontaneous { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unsure determination.
        /// </summary>
        public bool UnsureDetermination { get; set; }

        /// <summary>
        /// Gets or sets ValidationStatus
        /// </summary>
        public string ValidationStatus { get; set; }

        /// <summary>
        /// Gets or sets Weight.
        /// Report form sv-SE name: Vikt
        /// </summary>
        public int? Weight { get; set; }
    }

    public class Taxon
    {
        private string _name;

        /// <summary>
        /// Gets or sets Type.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets HasChildren
        /// </summary>
        public int HasChildren { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? ScientificName : _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets ScientificName.
        /// </summary>
        public string ScientificName { get; set; }

        /// <summary>
        /// Gets or sets Auctor.
        /// </summary>
        public string Auctor { get; set; }

        /// <summary>
        /// Gets or sets SpeciesGroupId.
        /// </summary>
        public int SpeciesGroupId { get; set; }

        /// <summary>
        /// Gets or sets SystematicOrder.
        /// </summary>
        public int SystematicOrder { get; set; }

        /// <summary>
        /// Gets or sets SightingName.
        /// </summary>
        public string SightingName { get; set; }

        /// <summary>
        /// Gets or sets ProtectionLevel.
        /// </summary>
        public int ProtectionLevelId { get; set; }

        /// <summary>
        /// Gets or sets IncludedByTaxonId.
        /// </summary>
        public int IncludedByTaxonId { get; set; }

        /// <summary>
        /// Gets or sets SightingCount
        /// </summary>
        public int SightingCount { get; set; }

        /// <summary>
        /// Gets or sets SightingCountAggregated
        /// </summary>
        public int SightingCountAggregated { get; set; }

    }

    public class Site
    {
        public Site()
        {
            Coordinates = new List<Coordinate>();
        }

        /// <summary>
        /// Gets or sets the accuracy.
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the diffusion
        /// </summary>
        public int Diffusion { get; set; }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets list of coordinates
        /// </summary>
        public List<Coordinate> Coordinates { get; set; }

        /// <summary>
        /// Gets or sets Distance
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Gets or sets ExternalId
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets Forsamling
        /// </summary>
        public string Forsamling { get; set; }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Kommun
        /// </summary>
        public string Kommun { get; set; }

        /// <summary>
        /// Gets or sets Län
        /// </summary>
        public string Lan { get; set; }

        /// <summary>
        /// Gets or sets Landskap
        /// </summary>
        public string Landskap { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets presentation name
        /// </summary>
        public string PresentationName { get; set; }

        /// <summary>
        /// Gets or sets ParentId
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets ProjectId
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets Socken
        /// </summary>
        public string Socken { get; set; }

        /// <summary>
        /// Gets or sets IsPublicBirdSite
        /// </summary>
        public bool IsPublicBirdSite { get; set; }
    }

    public class RuleValidationMessage
    {
        public string Type { get; set; }
        public string Level { get; set; }
        public string Description { get; set; }

    }

    public class ProjectWithParameterValues : ProjectBase
    {
        /// <summary>
        /// Gets or sets the ProjectParameterValue
        /// </summary>
        public List<ProjectParameterValue> ProjectParameterValues { get; set; }
    }

    public class ProjectParameterValue
    {
        /// <summary>
        /// The project parameter id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The project parameter name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The project parameter value for a certain sighting
        /// </summary>
        public string Value { get; set; }
    }

    public class ProjectBase
    {
        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets StartDate
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets EndDate
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value AllowMembersToCreateSites 
        /// </summary>
        public bool AllowMembersToCreateSites { get; set; }

        /// <summary>
        /// Gets or sets the AreaTypeId
        /// </summary>
        public int? AreaTypeId { get; set; }
    }

    /// <summary>
    /// A general coordinate.
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// Gets or sets X wich represents any of "X", "East" or "Longitude" depending on coordinate system.
        /// </summary>
        public double Easting { get; set; }

        /// <summary>
        /// Gets or sets Y, wich represents any of "Y", "North" or "Latitude" depending on coordinate system.
        /// </summary>
        public double Northing { get; set; }

        /// <summary>
        /// Is coordinate diffused
        /// </summary>
        public bool IsCoordinateDiffused { get; set; }

        /// <summary>
        /// Gets or sets CoordinateSystemName.
        /// </summary>
        public string CoordinateSystemName { get; set; }

        /// <summary>
        /// Epsg
        /// </summary>
        public int Epsg { get; set; }

        /// <summary>
        /// Coordinatesystem id
        /// </summary>
        public int CoordinateSystemId { get; set; }
    }
}
