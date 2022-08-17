namespace SOS.Blazor.Shared.Models
{
    public class SpeciesCountUserStatisticsQuery
    {
        public int? TaxonId { get; set; }
        public int? Year { get; set; }
        public SpeciesGroup? SpeciesGroup { get; set; }
        public AreaType? AreaType { get; set; }
        public string FeatureId { get; set; }
        public int? SiteId { get; set; }
        public int? ProjectId { get; set; }
        public bool IncludeOtherAreasSpeciesCount { get; set; }
        public string SortByFeatureId { get; set; }
    }

    public enum AreaType
    {
        Municipality = 1,
        Community = 11,
        Sea = 12,
        CountryRegion = 13,
        NatureType = 15,
        Province = 16,
        Ramsar = 17,
        BirdValidationArea = 18,
        Parish = 19,
        Spa = 20,
        County = 21,
        ProtectedNature = 22,
        SwedishForestAgencyDistricts = 24,
        EconomicZoneOfSweden = 25,
        Sci = 26,
        WaterArea = 27,
        Atlas5x5 = 29,
        Atlas10x10 = 30
    }

    /// <summary>
    /// Species group (Artgrupp)
    /// </summary>
    public enum SpeciesGroup
    {
        /// <summary>
        /// All species groups (Alla artgrupper)
        /// </summary>
        All = 0,

        /// <summary>
        /// Vascular plants (Kärlväxter)
        /// </summary>
        VascularPlants = 1,

        /// <summary>
        /// Mosses (Mossor)
        /// </summary>
        Mosses = 2,

        /// <summary>
        /// Lichens (Lavar)
        /// </summary>
        Lichens = 3,

        /// <summary>
        /// Fungi (Svampar)
        /// </summary>
        Fungi = 4,

        /// <summary>
        /// Algae and microorganisms (Alger och mikroorganismer)
        /// </summary>
        AlgaeAndMicroOrganisms = 5,

        /// <summary>
        /// Invertebrates (Ryggradslösa djur)
        /// </summary>
        Invertebrates = 6,

        /// <summary>
        /// Birds (fåglar)
        /// </summary>
        Birds = 7,

        /// <summary>
        /// Amphibians and reptiles (Grod- och kräldjur)
        /// </summary>
        AmphibiansAndReptiles = 8,

        /// <summary>
        /// Other vertebrates (Andra däggdjur)
        /// </summary>
        OtherVertebrates = 9,

        /// <summary>
        /// Bats (Fladdermöss)
        /// </summary>
        Bats = 10,

        /// <summary>
        /// Fishes (Fiskar)
        /// </summary>
        Fishes = 11
    }
}
