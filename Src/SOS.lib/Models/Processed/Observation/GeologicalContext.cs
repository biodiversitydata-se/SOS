namespace SOS.Lib.Models.Processed.Observation
{
    public class GeologicalContext
    {
        /// <summary>
        ///     Darwin Core term name: bed.
        ///     The full name of the lithostratigraphic bed from which
        ///     the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string Bed { get; set; }

        /// <summary>
        ///     Darwin Core term name: earliestAgeOrLowestStage.
        ///     The full name of the earliest possible geochronologic
        ///     age or lowest chronostratigraphic stage attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string EarliestAgeOrLowestStage { get; set; }

        /// <summary>
        ///     Darwin Core term name: earliestEonOrLowestEonothem.
        ///     The full name of the earliest possible geochronologic eon
        ///     or lowest chrono-stratigraphic eonothem or the informal
        ///     name ("Precambrian") attributable to the stratigraphic
        ///     horizon from which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string EarliestEonOrLowestEonothem { get; set; }

        /// <summary>
        ///     Darwin Core term name: earliestEpochOrLowestSeries.
        ///     The full name of the earliest possible geochronologic
        ///     epoch or lowest chronostratigraphic series attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string EarliestEpochOrLowestSeries { get; set; }

        /// <summary>
        ///     Darwin Core term name: earliestEraOrLowestErathem.
        ///     The full name of the earliest possible geochronologic
        ///     era or lowest chronostratigraphic erathem attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string EarliestEraOrLowestErathem { get; set; }

        /// <summary>
        ///     Use to link a dwc:GeologicalContext instance to chronostratigraphic time
        ///     periods at the lowest possible level in a standardized hierarchy. Use this
        ///     property to point to the earliest possible geological time period from which
        ///     the cataloged item was collected.
        /// </summary>
        public string EarliestGeochronologicalEra { get; set; }

        /// <summary>
        ///     Darwin Core term name: earliestPeriodOrLowestSystem.
        ///     The full name of the earliest possible geochronologic
        ///     period or lowest chronostratigraphic system attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string EarliestPeriodOrLowestSystem { get; set; }

        /// <summary>
        ///     Darwin Core term name: formation.
        ///     The full name of the lithostratigraphic formation from
        ///     which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string Formation { get; set; }

        /// <summary>
        ///     Darwin Core term name: geologicalContextID.
        ///     An identifier for the set of information associated
        ///     with a GeologicalContext (the location within a geological
        ///     context, such as stratigraphy). May be a global unique
        ///     identifier or an identifier specific to the data set.
        ///     This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string GeologicalContextId { get; set; }

        /// <summary>
        ///     Darwin Core term name: group.
        ///     The full name of the lithostratigraphic group from
        ///     which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        ///     Darwin Core term name: highestBiostratigraphicZone.
        ///     The full name of the highest possible geological
        ///     biostratigraphic zone of the stratigraphic horizon
        ///     from which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string HighestBiostratigraphicZone { get; set; }

        /// <summary>
        ///     Darwin Core term name: latestAgeOrHighestStage.
        ///     The full name of the latest possible geochronologic
        ///     age or highest chronostratigraphic stage attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LatestAgeOrHighestStage { get; set; }

        /// <summary>
        ///     Darwin Core term name: latestEonOrHighestEonothem.
        ///     The full name of the latest possible geochronologic eon
        ///     or highest chrono-stratigraphic eonothem or the informal
        ///     name ("Precambrian") attributable to the stratigraphic
        ///     horizon from which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LatestEonOrHighestEonothem { get; set; }

        /// <summary>
        ///     Darwin Core term name: latestEpochOrHighestSeries.
        ///     The full name of the latest possible geochronologic
        ///     epoch or highest chronostratigraphic series attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LatestEpochOrHighestSeries { get; set; }

        /// <summary>
        ///     Darwin Core term name: latestEraOrHighestErathem.
        ///     The full name of the latest possible geochronologic
        ///     era or highest chronostratigraphic erathem attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LatestEraOrHighestErathem { get; set; }

        /// <summary>
        ///     Use to link a dwc:GeologicalContext instance to chronostratigraphic time periods at the lowest possible
        ///     level in a standardized hierarchy. Use this property to point to the latest possible geological time period
        ///     from which the cataloged item was collected.
        /// </summary>
        public string LatestGeochronologicalEra { get; set; }

        /// <summary>
        ///     Darwin Core term name: latestPeriodOrHighestSystem.
        ///     The full name of the latest possible geochronologic
        ///     period or highest chronostratigraphic system attributable
        ///     to the stratigraphic horizon from which the cataloged
        ///     item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LatestPeriodOrHighestSystem { get; set; }

        /// <summary>
        ///     Darwin Core term name: lithostratigraphicTerms.
        ///     The combination of all litho-stratigraphic names for
        ///     the rock from which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LithostratigraphicTerms { get; set; }

        /// <summary>
        ///     Darwin Core term name: lowestBiostratigraphicZone.
        ///     The full name of the lowest possible geological
        ///     biostratigraphic zone of the stratigraphic horizon
        ///     from which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string LowestBiostratigraphicZone { get; set; }

        /// <summary>
        ///     Darwin Core term name: member.
        ///     The full name of the lithostratigraphic member from
        ///     which the cataloged item was collected.
        ///     This property is currently not used.
        /// </summary>
        public string Member { get; set; }
    }
}