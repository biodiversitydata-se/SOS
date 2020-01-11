namespace SOS.Lib.Models.Processed.DarwinCore
{
    /// <summary>
    /// Geological information, such as stratigraphy, that qualifies a region or place.
    /// </summary>
    /// <example>
    /// A lithostratigraphic layer
    /// </example>
    public class DarwinCoreGeologicalContext
    {
        /// <summary>
        /// The full name of the lithostratigraphic bed from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Harlem coal
        /// </example>
        public string Bed { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic age or lowest chronostratigraphic stage attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Atlantic, Boreal, Skullrockian
        /// </example>
        public string EarliestAgeOrLowestStage { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic eon or lowest chrono-stratigraphic eonothem or the informal name ("Precambrian") attributable
        /// to the stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Phanerozoic, Proterozoic
        /// </example>
        public string EarliestEonOrLowestEonothem { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic epoch or lowest chronostratigraphic series attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Holocene, Pleistocene, Ibexian Series
        /// </example>
        public string EarliestEpochOrLowestSeries { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic era or lowest chronostratigraphic erathem attributable to the stratigraphic horizon
        /// from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Cenozoic, Mesozoic
        /// </example>
        public string EarliestEraOrLowestErathem { get; set; }

        /// <summary>
        /// The full name of the earliest possible geochronologic period or lowest chronostratigraphic system attributable to the
        /// stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Neogene, Tertiary, Quaternary
        /// </example>
        public string EarliestPeriodOrLowestSystem { get; set; }

        /// <summary>
        /// The full name of the lithostratigraphic formation from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Notch Peak Formation, House Limestone, Fillmore Formation
        /// </example>
        public string Formation { get; set; }

        /// <summary>
        /// An identifier for the set of information associated with a GeologicalContext (the location within a geological context, such as stratigraphy).
        /// May be a global unique identifier or an identifier specific to the data set.
        /// </summary>
        /// <example>
        /// https://opencontext.org/subjects/e54377f7-4452-4315-b676-40679b10c4d9
        /// </example>
        public string GeologicalContextID { get; set; }

        /// <summary>
        /// The full name of the lithostratigraphic group from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Bathurst, Lower Wealden
        /// </example>
        public string Group { get; set; }

        /// <summary>
        /// The full name of the highest possible geological biostratigraphic zone of the stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Blancan
        /// </example>
        public string HighestBiostratigraphicZone { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic age or highest chronostratigraphic stage attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Atlantic, Boreal, Skullrockian
        /// </example>
        public string LatestAgeOrHighestStage { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic eon or highest chrono-stratigraphic eonothem or the informal name ("Precambrian")
        /// attributable to the stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Phanerozoic, Proterozoic
        /// </example>
        public string LatestEonOrHighestEonothem { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic epoch or highest chronostratigraphic series attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Holocene, Pleistocene, Ibexian Series
        /// </example>
        public string LatestEpochOrHighestSeries { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic era or highest chronostratigraphic erathem attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Cenozoic, Mesozoic
        /// </example>
        public string LatestEraOrHighestErathem { get; set; }

        /// <summary>
        /// The full name of the latest possible geochronologic period or highest chronostratigraphic system attributable to the stratigraphic
        /// horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Neogene, Tertiary, Quaternary
        /// </example>
        public string LatestPeriodOrHighestSystem { get; set; }

        /// <summary>
        /// The combination of all litho-stratigraphic names for the rock from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Pleistocene-Weichselien
        /// </example>
        public string LithostratigraphicTerms { get; set; }

        /// <summary>
        /// The full name of the lowest possible geological biostratigraphic zone of the stratigraphic horizon from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Maastrichtian
        /// </example>
        public string LowestBiostratigraphicZone { get; set; }

        /// <summary>
        /// The full name of the lithostratigraphic member from which the cataloged item was collected.
        /// </summary>
        /// <example>
        /// Lava Dam Member, Hellnmaria Member
        /// </example>
        public string Member { get; set; }
    }
}
