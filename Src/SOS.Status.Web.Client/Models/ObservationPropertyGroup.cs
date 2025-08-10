using SOS.Status.Web.Client.Dtos.SosObsApi;

namespace SOS.Status.Web.Client.Models;

public class ObservationPropertyGroup
{
    public string Title { get; set; } = "";
    public List<ObservationPropertyRow> Rows { get; set; } = new();

    public static List<ObservationPropertyGroup> Groups = new()
    {
        new ObservationPropertyGroup
        {
            Title = "Record level",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "Dataset name", ValueSelector = o => o.DatasetName },
                //new() { Label = "Collection code", ValueSelector = o => o.CollectionCode },
                new() { Label = "Owner institution code", ValueSelector = o => o.OwnerInstitutionCode },
                new() { Label = "Rights holder", ValueSelector = o => o.RightsHolder },
                new() { Label = "Modified", ValueSelector = o => o.Modified }
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Occurrence",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "OccurrenceId", ValueSelector = o => o.Occurrence.OccurrenceId },
                new() { Label = "Occurrence status", ValueSelector = o => o.Occurrence.OccurrenceStatus?.Value, DefaultValues = ["present"] },
                new() { Label = "Occurrence remarks", ValueSelector = o => o.Occurrence.OccurrenceRemarks },
                new() { Label = "Recorded by", ValueSelector = o => o.Occurrence.RecordedBy },
                //new() { Label = "Reported by", ValueSelector = o => o.Occurrence.ReportedBy },
                //new() { Label = "Individual count", ValueSelector = o => o.Occurrence.IndividualCount, DefaultValues = [1] },
                new() { Label = "Organism quantity", ValueSelector = o => o.Occurrence.OrganismQuantity, DefaultValues = [1] },
                new() { Label = "Organism quantity (integer)", ValueSelector = o => o.Occurrence.OrganismQuantityInt, DefaultValues = [1] },
                new() { Label = "Organism quantity unit", ValueSelector = o => o.Occurrence.OrganismQuantityUnit },
                new() { Label = "Url", ValueSelector = o => o.Occurrence.Url, IsLink = true },
                new() { Label = "Activity", ValueSelector = o => o.Occurrence.Activity?.Value },
                //new() { Label = "Behavior", ValueSelector = o => o.Occurrence.Behavior?.Value },
                new() { Label = "Lifestage", ValueSelector = o => o.Occurrence.LifeStage?.Value },
                new() { Label = "Reproductive condition", ValueSelector = o => o.Occurrence.ReproductiveCondition?.Value },
                new() { Label = "Sex", ValueSelector = o => o.Occurrence.Sex?.Value },
                new() { Label = "Biotope", ValueSelector = o => o.Occurrence.Biotope?.Value },
                new() { Label = "Biotope description", ValueSelector = o => o.Occurrence.BiotopeDescription },
                new() { Label = "Sensitivity category", ValueSelector = o => o.Occurrence.SensitivityCategory, DefaultValues = [1] },
                new() { Label = "Substrate name", ValueSelector = o => o.Occurrence.Substrate?.Name },
                new() { Label = "Length", ValueSelector = o => o.Occurrence.Length },
                new() { Label = "Weight", ValueSelector = o => o.Occurrence.Weight },
                new() { Label = "Is positive observation", ValueSelector = o => o.Occurrence.IsPositiveObservation, DefaultValues = [true] },
                new() { Label = "Is natural occurrence", ValueSelector = o => o.Occurrence.IsNaturalOccurrence, DefaultValues = [true] },
                new() { Label = "Is never found observation", ValueSelector = o => o.Occurrence.IsNeverFoundObservation, DefaultValues = [false] },
                new() { Label = "Is not rediscovered observation", ValueSelector = o => o.Occurrence.IsNotRediscoveredObservation, DefaultValues = [false] }
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Event",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "EventId", ValueSelector = o => o.Event.EventId },
                new() { Label = "Start date", ValueSelector = o => o.Event.StartDate },
                //new() { Label = "Start day of year", ValueSelector = o => o.Event.StartDayOfYear },
                //new() { Label = "Start histogram week", ValueSelector = o => o.Event.StartHistogramWeek },
                //new() { Label = "Start year", ValueSelector = o => o.Event.StartYear },
                //new() { Label = "Start month", ValueSelector = o => o.Event.StartMonth },
                //new() { Label = "Start day", ValueSelector = o => o.Event.StartDay },
                new() { Label = "End date", ValueSelector = o => o.Event.EndDate },
                //new() { Label = "End day of year", ValueSelector = o => o.Event.EndDayOfYear },
                //new() { Label = "End histogram week", ValueSelector = o => o.Event.EndHistogramWeek },
                //new() { Label = "End year", ValueSelector = o => o.Event.EndYear },
                //new() { Label = "End month", ValueSelector = o => o.Event.EndMonth },
                //new() { Label = "End day", ValueSelector = o => o.Event.EndDay },
                //new() { Label = "Plain start date", ValueSelector = o => o.Event.PlainStartDate },
                //new() { Label = "Plain end date", ValueSelector = o => o.Event.PlainEndDate },
                //new() { Label = "Plain start time", ValueSelector = o => o.Event.PlainStartTime },
                //new() { Label = "Plain end time", ValueSelector = o => o.Event.PlainEndTime },
                //new() { Label = "Discovery method", ValueSelector = o => o.Event.DiscoveryMethod?.Value },
                new() { Label = "Event remarks", ValueSelector = o => o.Event.EventRemarks },
                new() { Label = "Field notes", ValueSelector = o => o.Event.FieldNotes },
                new() { Label = "Field number", ValueSelector = o => o.Event.FieldNumber },
                new() { Label = "Habitat", ValueSelector = o => o.Event.Habitat },
                //new() { Label = "Parent event id", ValueSelector = o => o.Event.ParentEventId },
                //new() { Label = "Sampling effort", ValueSelector = o => o.Event.SamplingEffort },
                //new() { Label = "Sampling protocol", ValueSelector = o => o.Event.SamplingProtocol },
                //new() { Label = "Sample size unit", ValueSelector = o => o.Event.SampleSizeUnit },
                //new() { Label = "Sample size value", ValueSelector = o => o.Event.SampleSizeValue },
                //new() { Label = "Verbatim event date", ValueSelector = o => o.Event.VerbatimEventDate }                
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Identification",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "IdentificationId", ValueSelector = o => o.Identification.IdentificationId },
                new() { Label = "Identification qualifier", ValueSelector = o => o.Identification.IdentificationQualifier },
                new() { Label = "Identification references", ValueSelector = o => o.Identification.IdentificationReferences },
                new() { Label = "Identification remarks", ValueSelector = o => o.Identification.IdentificationRemarks },
                new() { Label = "Determination method", ValueSelector = o => o.Identification.DeterminationMethod?.Value },
                new() { Label = "Confirmed date", ValueSelector = o => o.Identification.ConfirmedDate },
                new() { Label = "Date identified", ValueSelector = o => o.Identification.DateIdentified },
                new() { Label = "Confirmed by", ValueSelector = o => o.Identification.ConfirmedBy },
                new() { Label = "Identified by", ValueSelector = o => o.Identification.IdentifiedBy },
                new() { Label = "Verified by", ValueSelector = o => o.Identification.VerifiedBy },
                new() { Label = "Verified", ValueSelector = o => o.Identification.Verified, DefaultValues = [false] },
                new() { Label = "Verification status", ValueSelector = o => o.Identification.VerificationStatus?.Value },
                new() { Label = "Uncertain identification", ValueSelector = o => o.Identification.UncertainIdentification, DefaultValues = [false] },
                new() { Label = "Type status", ValueSelector = o => o.Identification.TypeStatus }
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Location",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "LocationId", ValueSelector = o => o.Location.LocationId },
                new() { Label = "Locality", ValueSelector = o => o.Location.Locality },
                new() { Label = "Location remarks", ValueSelector = o => o.Location.LocationRemarks },
                new() { Label = "Coordinate uncertainty", ValueSelector = o => o.Location.CoordinateUncertaintyInMeters },
                new() { Label = "Decimal latitude", ValueSelector = o => o.Location.DecimalLatitude },
                new() { Label = "Decimal longitude", ValueSelector = o => o.Location.DecimalLongitude },
                //new() { Label = "Geodetic datum", ValueSelector = o => o.Location.GeodeticDatum },
                new() { Label = "Municipality", ValueSelector = o => o.Location.Municipality?.Name },
                new() { Label = "County", ValueSelector = o => o.Location.County?.Name },
                new() { Label = "Province", ValueSelector = o => o.Location.Province?.Name },
                //new() { Label = "Verbatim coordinates", ValueSelector = o => o.Location.VerbatimCoordinates },
                //new() { Label = "Verbatim locality", ValueSelector = o => o.Location.VerbatimLocality },
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Taxon",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "TaxonId", ValueSelector = o => o.Taxon.Id },
                new() { Label = "Vernacular name", ValueSelector = o => o.Taxon.VernacularName },
                new() { Label = "Scientific name", ValueSelector = o => o.Taxon.ScientificName },
                //new() { Label = "Scientific name author", ValueSelector = o => o.Taxon.ScientificNameAuthorship },
                new() { Label = "Taxon category", ValueSelector = o => o.Taxon.Attributes.TaxonCategory?.Value },
                new() { Label = "Organism group", ValueSelector = o => o.Taxon.Attributes.OrganismGroup },
                new() { Label = "Species group", ValueSelector = o => o.Taxon.Attributes.SpeciesGroup },
                //new() { Label = "Taxon rank", ValueSelector = o => o.Taxon.TaxonRank },
                //new() { Label = "Kingdom", ValueSelector = o => o.Taxon.Kingdom },
                //new() { Label = "Phylum", ValueSelector = o => o.Taxon.Phylum },
                //new() { Label = "Class", ValueSelector = o => o.Taxon.Class },
                //new() { Label = "Order", ValueSelector = o => o.Taxon.Order },
                //new() { Label = "Family", ValueSelector = o => o.Taxon.Family },
                //new() { Label = "Genus", ValueSelector = o => o.Taxon.Genus }
            }
        },
        new ObservationPropertyGroup
        {
            Title = "Taxon attributes",
            Rows = new List<ObservationPropertyRow>
            {
                new() { Label = "Disturbance radius", ValueSelector = o => o.Taxon.Attributes.DisturbanceRadius },
                new() { Label = "Dyntaxa taxon id", ValueSelector = o => o.Taxon.Attributes.DyntaxaTaxonId },
                new() { Label = "GBIF taxon id", ValueSelector = o => o.Taxon.Attributes.GbifTaxonId },
                //new() { Label = "Redlist category", ValueSelector = o => o.Taxon.Attributes.RedlistCategory },
                new() { Label = "Is redlisted", ValueSelector = o => o.Taxon.Attributes.IsRedlisted, DefaultValues = new object[]{ false } },
                new() { Label = "Redlist category", ValueSelector = o => o.Taxon.Attributes.RedlistCategoryDerived },
                new() { Label = "Protected by law", ValueSelector = o => o.Taxon.Attributes.ProtectedByLaw, DefaultValues = [false] },
                new() { Label = "Sensitivity category", ValueSelector = o => o.Taxon.Attributes.SensitivityCategory?.Value },
                new() { Label = "Swedish occurrence", ValueSelector = o => o.Taxon.Attributes.SwedishOccurrence },
                new() { Label = "Swedish history", ValueSelector = o => o.Taxon.Attributes.SwedishHistory },

                new() { Label = "Is invasive according to EU regulation", ValueSelector = o => o.Taxon.Attributes.IsInvasiveAccordingToEuRegulation, DefaultValues = new object[]{ false } },
                new() { Label = "Is invasive in Sweden", ValueSelector = o => o.Taxon.Attributes.IsInvasiveInSweden, DefaultValues = new object[]{ false } },
                new() { Label = "Invasive risk assessment category", ValueSelector = o => o.Taxon.Attributes.InvasiveRiskAssessmentCategory },
                //new() { Label = "Action plan", ValueSelector = o => o.Taxon.Attributes.ActionPlan },
                //new() { Label = "Natura2000 - Article 2", ValueSelector = o => o.Taxon.Attributes.Natura2000HabitatsDirectiveArticle2, DefaultValues = new object[]{ false } },
                //new() { Label = "Natura2000 - Article 4", ValueSelector = o => o.Taxon.Attributes.Natura2000HabitatsDirectiveArticle4, DefaultValues = new object[]{ false } },
                //new() { Label = "Natura2000 - Article 5", ValueSelector = o => o.Taxon.Attributes.Natura2000HabitatsDirectiveArticle5, DefaultValues = new object[]{ false } },                
                //new() { Label = "Parent Dyntaxa taxon id", ValueSelector = o => o.Taxon.Attributes.ParentDyntaxaTaxonId },                
                //new() { Label = "Sort order", ValueSelector = o => o.Taxon.Attributes.SortOrder, DefaultValues = new object[]{ 0 } },                                
                //new() { Label = "Synonyms", ValueSelector = o => o.Taxon.Attributes.Synonyms != null ? string.Join(", ", o.Taxon.Attributes.Synonyms) : null },                
                //new() { Label = "Vernacular names", ValueSelector = o => o.Taxon.Attributes.VernacularNames != null ? string.Join(", ", o.Taxon.Attributes.VernacularNames) : null }
            }
        }
    };

}

public class ObservationPropertyRow
{
    public string Label { get; set; } = "";
    public Func<Observation, object?> ValueSelector { get; set; } = _ => null;
    public bool IsLink { get; set; } = false;
    public object[] DefaultValues { get; set; } = Array.Empty<object>();
}