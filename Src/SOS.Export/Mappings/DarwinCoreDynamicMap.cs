using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CsvHelper.Configuration;
using SOS.Export.IO.Csv.Converters;
using SOS.Export.Models;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public sealed class DarwinCoreDynamicMap : ClassMap<DarwinCore>
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public DarwinCoreDynamicMap(IEnumerable<FieldDescription> fieldDescriptions)
            : this(fieldDescriptions.Select(m => (FieldDescriptionId) m.Id))
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        public DarwinCoreDynamicMap(IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            var mappingbyId = CreateMappingDictionary(fieldDescriptionIds);

            Map(m => m.Occurrence.OccurrenceID)
                .Name(mappingbyId[FieldDescriptionId.OccurrenceID].Name)
                .Index(mappingbyId[FieldDescriptionId.OccurrenceID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OccurrenceID].Ignore);

            Map(m => m.Occurrence.CatalogNumber)
                .Name(mappingbyId[FieldDescriptionId.CatalogNumber].Name)
                .Index(mappingbyId[FieldDescriptionId.CatalogNumber].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CatalogNumber].Ignore);

            Map(m => m.Occurrence.RecordNumber)
                .Name(mappingbyId[FieldDescriptionId.RecordNumber].Name)
                .Index(mappingbyId[FieldDescriptionId.RecordNumber].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RecordNumber].Ignore);

            Map(m => m.Occurrence.RecordedBy)
                .Name(mappingbyId[FieldDescriptionId.RecordedBy].Name)
                .Index(mappingbyId[FieldDescriptionId.RecordedBy].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RecordedBy].Ignore);

            Map(m => m.Occurrence.IndividualCount)
                .Name(mappingbyId[FieldDescriptionId.IndividualCount].Name)
                .Index(mappingbyId[FieldDescriptionId.IndividualCount].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IndividualCount].Ignore);

            Map(m => m.Occurrence.OrganismQuantity)
                .Name(mappingbyId[FieldDescriptionId.OrganismQuantity].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismQuantity].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismQuantity].Ignore);

            Map(m => m.Occurrence.OrganismQuantityType)
                .Name(mappingbyId[FieldDescriptionId.OrganismQuantityType].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismQuantityType].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismQuantityType].Ignore);

            Map(m => m.Occurrence.Sex)
                .Name(mappingbyId[FieldDescriptionId.Sex].Name)
                .Index(mappingbyId[FieldDescriptionId.Sex].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Sex].Ignore);

            Map(m => m.Occurrence.LifeStage)
                .Name(mappingbyId[FieldDescriptionId.LifeStage].Name)
                .Index(mappingbyId[FieldDescriptionId.LifeStage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LifeStage].Ignore);

            Map(m => m.Occurrence.ReproductiveCondition)
                .Name(mappingbyId[FieldDescriptionId.ReproductiveCondition].Name)
                .Index(mappingbyId[FieldDescriptionId.ReproductiveCondition].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ReproductiveCondition].Ignore);

            Map(m => m.Occurrence.Behavior)
                .Name(mappingbyId[FieldDescriptionId.Behavior].Name)
                .Index(mappingbyId[FieldDescriptionId.Behavior].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Behavior].Ignore);

            Map(m => m.Occurrence.EstablishmentMeans)
                .Name(mappingbyId[FieldDescriptionId.EstablishmentMeans].Name)
                .Index(mappingbyId[FieldDescriptionId.EstablishmentMeans].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EstablishmentMeans].Ignore);

            Map(m => m.Occurrence.OccurrenceStatus)
                .Name(mappingbyId[FieldDescriptionId.OccurrenceStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.OccurrenceStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OccurrenceStatus].Ignore);

            Map(m => m.Occurrence.Preparations)
                .Name(mappingbyId[FieldDescriptionId.Preparations].Name)
                .Index(mappingbyId[FieldDescriptionId.Preparations].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Preparations].Ignore);

            Map(m => m.Occurrence.Disposition)
                .Name(mappingbyId[FieldDescriptionId.Disposition].Name)
                .Index(mappingbyId[FieldDescriptionId.Disposition].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Disposition].Ignore);

            Map(m => m.Occurrence.AssociatedMedia)
                .Name(mappingbyId[FieldDescriptionId.AssociatedMedia].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedMedia].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedMedia].Ignore);

            Map(m => m.Occurrence.AssociatedReferences)
                .Name(mappingbyId[FieldDescriptionId.AssociatedReferences].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedReferences].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedReferences].Ignore);

            Map(m => m.Occurrence.AssociatedSequences)
                .Name(mappingbyId[FieldDescriptionId.AssociatedSequences].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedSequences].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedSequences].Ignore);

            Map(m => m.Occurrence.AssociatedTaxa)
                .Name(mappingbyId[FieldDescriptionId.AssociatedTaxa].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedTaxa].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedTaxa].Ignore);

            Map(m => m.Occurrence.OtherCatalogNumbers)
                .Name(mappingbyId[FieldDescriptionId.OtherCatalogNumbers].Name)
                .Index(mappingbyId[FieldDescriptionId.OtherCatalogNumbers].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OtherCatalogNumbers].Ignore);

            Map(m => m.Occurrence.OccurrenceRemarks)
                .Name(mappingbyId[FieldDescriptionId.OccurrenceRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.OccurrenceRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OccurrenceRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.DatasetID)
                .Name(mappingbyId[FieldDescriptionId.DatasetID].Name)
                .Index(mappingbyId[FieldDescriptionId.DatasetID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DatasetID].Ignore);

            Map(m => m.Type)
                .Name(mappingbyId[FieldDescriptionId.Type].Name)
                .Index(mappingbyId[FieldDescriptionId.Type].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Type].Ignore);

            Map(m => m.Modified)
                .Name(mappingbyId[FieldDescriptionId.Modified].Name)
                .Index(mappingbyId[FieldDescriptionId.Modified].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Modified].Ignore)
                .TypeConverterOption.Format("s");

            Map(m => m.Language)
                .Name(mappingbyId[FieldDescriptionId.Language].Name)
                .Index(mappingbyId[FieldDescriptionId.Language].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Language].Ignore);

            Map(m => m.License)
                .Name(mappingbyId[FieldDescriptionId.License].Name)
                .Index(mappingbyId[FieldDescriptionId.License].Index)
                .Ignore(mappingbyId[FieldDescriptionId.License].Ignore);

            Map(m => m.RightsHolder)
                .Name(mappingbyId[FieldDescriptionId.RightsHolder].Name)
                .Index(mappingbyId[FieldDescriptionId.RightsHolder].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RightsHolder].Ignore);

            Map(m => m.AccessRights)
                .Name(mappingbyId[FieldDescriptionId.AccessRights].Name)
                .Index(mappingbyId[FieldDescriptionId.AccessRights].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AccessRights].Ignore);

            Map(m => m.BibliographicCitation)
                .Name(mappingbyId[FieldDescriptionId.BibliographicCitation].Name)
                .Index(mappingbyId[FieldDescriptionId.BibliographicCitation].Index)
                .Ignore(mappingbyId[FieldDescriptionId.BibliographicCitation].Ignore);

            Map(m => m.References)
                .Name(mappingbyId[FieldDescriptionId.References].Name)
                .Index(mappingbyId[FieldDescriptionId.References].Index)
                .Ignore(mappingbyId[FieldDescriptionId.References].Ignore);

            Map(m => m.InstitutionID)
                .Name(mappingbyId[FieldDescriptionId.InstitutionID].Name)
                .Index(mappingbyId[FieldDescriptionId.InstitutionID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.InstitutionID].Ignore);

            Map(m => m.CollectionID)
                .Name(mappingbyId[FieldDescriptionId.CollectionID].Name)
                .Index(mappingbyId[FieldDescriptionId.CollectionID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CollectionID].Ignore);

            Map(m => m.InstitutionCode)
                .Name(mappingbyId[FieldDescriptionId.InstitutionCode].Name)
                .Index(mappingbyId[FieldDescriptionId.InstitutionCode].Index)
                .Ignore(mappingbyId[FieldDescriptionId.InstitutionCode].Ignore);

            Map(m => m.CollectionCode)
                .Name(mappingbyId[FieldDescriptionId.CollectionCode].Name)
                .Index(mappingbyId[FieldDescriptionId.CollectionCode].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CollectionCode].Ignore);

            Map(m => m.DatasetName)
                .Name(mappingbyId[FieldDescriptionId.DatasetName].Name)
                .Index(mappingbyId[FieldDescriptionId.DatasetName].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DatasetName].Ignore);

            Map(m => m.OwnerInstitutionCode)
                .Name(mappingbyId[FieldDescriptionId.OwnerInstitutionCode].Name)
                .Index(mappingbyId[FieldDescriptionId.OwnerInstitutionCode].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OwnerInstitutionCode].Ignore);

            Map(m => m.BasisOfRecord)
                .Name(mappingbyId[FieldDescriptionId.BasisOfRecord].Name)
                .Index(mappingbyId[FieldDescriptionId.BasisOfRecord].Index)
                .Ignore(mappingbyId[FieldDescriptionId.BasisOfRecord].Ignore);

            Map(m => m.InformationWithheld)
                .Name(mappingbyId[FieldDescriptionId.InformationWithheld].Name)
                .Index(mappingbyId[FieldDescriptionId.InformationWithheld].Index)
                .Ignore(mappingbyId[FieldDescriptionId.InformationWithheld].Ignore);

            Map(m => m.DataGeneralizations)
                .Name(mappingbyId[FieldDescriptionId.DataGeneralizations].Name)
                .Index(mappingbyId[FieldDescriptionId.DataGeneralizations].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DataGeneralizations].Ignore);

            Map(m => m.DynamicProperties)
                .Name(mappingbyId[FieldDescriptionId.DynamicProperties].Name)
                .Index(mappingbyId[FieldDescriptionId.DynamicProperties].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DynamicProperties].Ignore)
                .TypeConverter<JsonConverter<DynamicProperties>>();

            Map(m => m.Event.EventID)
                .Name(mappingbyId[FieldDescriptionId.EventID].Name)
                .Index(mappingbyId[FieldDescriptionId.EventID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EventID].Ignore);

            Map(m => m.Event.ParentEventID)
                .Name(mappingbyId[FieldDescriptionId.ParentEventID].Name)
                .Index(mappingbyId[FieldDescriptionId.ParentEventID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ParentEventID].Ignore);

            Map(m => m.Event.FieldNumber)
                .Name(mappingbyId[FieldDescriptionId.FieldNumber].Name)
                .Index(mappingbyId[FieldDescriptionId.FieldNumber].Index)
                .Ignore(mappingbyId[FieldDescriptionId.FieldNumber].Ignore);

            Map(m => m.Event.EventDate)
                .Name(mappingbyId[FieldDescriptionId.EventDate].Name)
                .Index(mappingbyId[FieldDescriptionId.EventDate].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EventDate].Ignore);

            Map(m => m.Event.EventTime)
                .Name(mappingbyId[FieldDescriptionId.EventTime].Name)
                .Index(mappingbyId[FieldDescriptionId.EventTime].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EventTime].Ignore);

            Map(m => m.Event.StartDayOfYear)
                .Name(mappingbyId[FieldDescriptionId.StartDayOfYear].Name)
                .Index(mappingbyId[FieldDescriptionId.StartDayOfYear].Index)
                .Ignore(mappingbyId[FieldDescriptionId.StartDayOfYear].Ignore);

            Map(m => m.Event.EndDayOfYear)
                .Name(mappingbyId[FieldDescriptionId.EndDayOfYear].Name)
                .Index(mappingbyId[FieldDescriptionId.EndDayOfYear].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EndDayOfYear].Ignore);

            Map(m => m.Event.Year)
                .Name(mappingbyId[FieldDescriptionId.Year].Name)
                .Index(mappingbyId[FieldDescriptionId.Year].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Year].Ignore);

            Map(m => m.Event.Month)
                .Name(mappingbyId[FieldDescriptionId.Month].Name)
                .Index(mappingbyId[FieldDescriptionId.Month].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Month].Ignore);

            Map(m => m.Event.Day)
                .Name(mappingbyId[FieldDescriptionId.Day].Name)
                .Index(mappingbyId[FieldDescriptionId.Day].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Day].Ignore);

            Map(m => m.Event.VerbatimEventDate)
                .Name(mappingbyId[FieldDescriptionId.VerbatimEventDate].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimEventDate].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimEventDate].Ignore);

            Map(m => m.Event.Habitat)
                .Name(mappingbyId[FieldDescriptionId.Habitat].Name)
                .Index(mappingbyId[FieldDescriptionId.Habitat].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Habitat].Ignore);

            Map(m => m.Event.SamplingProtocol)
                .Name(mappingbyId[FieldDescriptionId.SamplingProtocol].Name)
                .Index(mappingbyId[FieldDescriptionId.SamplingProtocol].Index)
                .Ignore(mappingbyId[FieldDescriptionId.SamplingProtocol].Ignore);

            Map(m => m.Event.SampleSizeValue)
                .Name(mappingbyId[FieldDescriptionId.SampleSizeValue].Name)
                .Index(mappingbyId[FieldDescriptionId.SampleSizeValue].Index)
                .Ignore(mappingbyId[FieldDescriptionId.SampleSizeValue].Ignore);

            Map(m => m.Event.SampleSizeUnit)
                .Name(mappingbyId[FieldDescriptionId.SampleSizeUnit].Name)
                .Index(mappingbyId[FieldDescriptionId.SampleSizeUnit].Index)
                .Ignore(mappingbyId[FieldDescriptionId.SampleSizeUnit].Ignore);

            Map(m => m.Event.SamplingEffort)
                .Name(mappingbyId[FieldDescriptionId.SamplingEffort].Name)
                .Index(mappingbyId[FieldDescriptionId.SamplingEffort].Index)
                .Ignore(mappingbyId[FieldDescriptionId.SamplingEffort].Ignore);

            Map(m => m.Event.FieldNotes)
                .Name(mappingbyId[FieldDescriptionId.FieldNotes].Name)
                .Index(mappingbyId[FieldDescriptionId.FieldNotes].Index)
                .Ignore(mappingbyId[FieldDescriptionId.FieldNotes].Ignore);

            Map(m => m.Event.EventRemarks)
                .Name(mappingbyId[FieldDescriptionId.EventRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.EventRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EventRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Location.LocationID)
                .Name(mappingbyId[FieldDescriptionId.LocationID].Name)
                .Index(mappingbyId[FieldDescriptionId.LocationID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LocationID].Ignore);

            Map(m => m.Location.HigherGeographyID)
                .Name(mappingbyId[FieldDescriptionId.HigherGeographyID].Name)
                .Index(mappingbyId[FieldDescriptionId.HigherGeographyID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.HigherGeographyID].Ignore);

            Map(m => m.Location.HigherGeography)
                .Name(mappingbyId[FieldDescriptionId.HigherGeography].Name)
                .Index(mappingbyId[FieldDescriptionId.HigherGeography].Index)
                .Ignore(mappingbyId[FieldDescriptionId.HigherGeography].Ignore);

            Map(m => m.Location.Continent)
                .Name(mappingbyId[FieldDescriptionId.Continent].Name)
                .Index(mappingbyId[FieldDescriptionId.Continent].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Continent].Ignore);

            Map(m => m.Location.WaterBody)
                .Name(mappingbyId[FieldDescriptionId.WaterBody].Name)
                .Index(mappingbyId[FieldDescriptionId.WaterBody].Index)
                .Ignore(mappingbyId[FieldDescriptionId.WaterBody].Ignore);

            Map(m => m.Location.IslandGroup)
                .Name(mappingbyId[FieldDescriptionId.IslandGroup].Name)
                .Index(mappingbyId[FieldDescriptionId.IslandGroup].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IslandGroup].Ignore);

            Map(m => m.Location.Island)
                .Name(mappingbyId[FieldDescriptionId.Island].Name)
                .Index(mappingbyId[FieldDescriptionId.Island].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Island].Ignore);

            Map(m => m.Location.Country)
                .Name(mappingbyId[FieldDescriptionId.Country].Name)
                .Index(mappingbyId[FieldDescriptionId.Country].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Country].Ignore);

            Map(m => m.Location.CountryCode)
                .Name(mappingbyId[FieldDescriptionId.CountryCode].Name)
                .Index(mappingbyId[FieldDescriptionId.CountryCode].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CountryCode].Ignore);

            Map(m => m.Location.StateProvince)
                .Name(mappingbyId[FieldDescriptionId.StateProvince].Name)
                .Index(mappingbyId[FieldDescriptionId.StateProvince].Index)
                .Ignore(mappingbyId[FieldDescriptionId.StateProvince].Ignore);

            Map(m => m.Location.County)
                .Name(mappingbyId[FieldDescriptionId.County].Name)
                .Index(mappingbyId[FieldDescriptionId.County].Index)
                .Ignore(mappingbyId[FieldDescriptionId.County].Ignore);

            Map(m => m.Location.Municipality)
                .Name(mappingbyId[FieldDescriptionId.Municipality].Name)
                .Index(mappingbyId[FieldDescriptionId.Municipality].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Municipality].Ignore);

            Map(m => m.Location.Locality)
                .Name(mappingbyId[FieldDescriptionId.Locality].Name)
                .Index(mappingbyId[FieldDescriptionId.Locality].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Locality].Ignore);

            Map(m => m.Location.VerbatimLocality)
                .Name(mappingbyId[FieldDescriptionId.VerbatimLocality].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimLocality].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimLocality].Ignore);

            Map(m => m.Location.MinimumElevationInMeters)
                .Name(mappingbyId[FieldDescriptionId.MinimumElevationInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MinimumElevationInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MinimumElevationInMeters].Ignore);

            Map(m => m.Location.MaximumElevationInMeters)
                .Name(mappingbyId[FieldDescriptionId.MaximumElevationInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MaximumElevationInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MaximumElevationInMeters].Ignore);

            Map(m => m.Location.VerbatimElevation)
                .Name(mappingbyId[FieldDescriptionId.VerbatimElevation].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimElevation].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimElevation].Ignore);

            Map(m => m.Location.MinimumDepthInMeters)
                .Name(mappingbyId[FieldDescriptionId.MinimumDepthInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MinimumDepthInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MinimumDepthInMeters].Ignore);

            Map(m => m.Location.MaximumDepthInMeters)
                .Name(mappingbyId[FieldDescriptionId.MaximumDepthInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MaximumDepthInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MaximumDepthInMeters].Ignore);

            Map(m => m.Location.VerbatimDepth)
                .Name(mappingbyId[FieldDescriptionId.VerbatimDepth].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimDepth].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimDepth].Ignore);

            Map(m => m.Location.MinimumDistanceAboveSurfaceInMeters)
                .Name(mappingbyId[FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters].Ignore);

            Map(m => m.Location.MaximumDistanceAboveSurfaceInMeters)
                .Name(mappingbyId[FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters].Ignore);

            Map(m => m.Location.LocationAccordingTo)
                .Name(mappingbyId[FieldDescriptionId.LocationAccordingTo].Name)
                .Index(mappingbyId[FieldDescriptionId.LocationAccordingTo].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LocationAccordingTo].Ignore);

            Map(m => m.Location.LocationRemarks)
                .Name(mappingbyId[FieldDescriptionId.LocationRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.LocationRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LocationRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Location.DecimalLatitude)
                .Name(mappingbyId[FieldDescriptionId.DecimalLatitude].Name)
                .Index(mappingbyId[FieldDescriptionId.DecimalLatitude].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DecimalLatitude].Ignore)
                .TypeConverterOption.Format("F5");

            Map(m => m.Location.DecimalLongitude)
                .Name(mappingbyId[FieldDescriptionId.DecimalLongitude].Name)
                .Index(mappingbyId[FieldDescriptionId.DecimalLongitude].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DecimalLongitude].Ignore)
                .TypeConverterOption.Format("F5");

            Map(m => m.Location.GeodeticDatum)
                .Name(mappingbyId[FieldDescriptionId.GeodeticDatum].Name)
                .Index(mappingbyId[FieldDescriptionId.GeodeticDatum].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeodeticDatum].Ignore);

            Map(m => m.Location.CoordinateUncertaintyInMeters)
                .Name(mappingbyId[FieldDescriptionId.CoordinateUncertaintyInMeters].Name)
                .Index(mappingbyId[FieldDescriptionId.CoordinateUncertaintyInMeters].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CoordinateUncertaintyInMeters].Ignore)
                .TypeConverter<CoordinateUncertaintyConverter<int?>>();

            Map(m => m.Location.CoordinatePrecision)
                .Name(mappingbyId[FieldDescriptionId.CoordinatePrecision].Name)
                .Index(mappingbyId[FieldDescriptionId.CoordinatePrecision].Index)
                .Ignore(mappingbyId[FieldDescriptionId.CoordinatePrecision].Ignore);

            Map(m => m.Location.PointRadiusSpatialFit)
                .Name(mappingbyId[FieldDescriptionId.PointRadiusSpatialFit].Name)
                .Index(mappingbyId[FieldDescriptionId.PointRadiusSpatialFit].Index)
                .Ignore(mappingbyId[FieldDescriptionId.PointRadiusSpatialFit].Ignore);

            Map(m => m.Location.VerbatimCoordinates)
                .Name(mappingbyId[FieldDescriptionId.VerbatimCoordinates].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimCoordinates].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimCoordinates].Ignore);

            Map(m => m.Location.VerbatimLatitude)
                .Name(mappingbyId[FieldDescriptionId.VerbatimLatitude].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimLatitude].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimLatitude].Ignore);

            Map(m => m.Location.VerbatimLongitude)
                .Name(mappingbyId[FieldDescriptionId.VerbatimLongitude].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimLongitude].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimLongitude].Ignore);

            Map(m => m.Location.VerbatimCoordinateSystem)
                .Name(mappingbyId[FieldDescriptionId.VerbatimCoordinateSystem].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimCoordinateSystem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimCoordinateSystem].Ignore);

            Map(m => m.Location.VerbatimSRS)
                .Name(mappingbyId[FieldDescriptionId.VerbatimSRS].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimSRS].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimSRS].Ignore);

            Map(m => m.Location.FootprintWKT)
                .Name(mappingbyId[FieldDescriptionId.FootprintWKT].Name)
                .Index(mappingbyId[FieldDescriptionId.FootprintWKT].Index)
                .Ignore(mappingbyId[FieldDescriptionId.FootprintWKT].Ignore);

            Map(m => m.Location.FootprintSRS)
                .Name(mappingbyId[FieldDescriptionId.FootprintSRS].Name)
                .Index(mappingbyId[FieldDescriptionId.FootprintSRS].Index)
                .Ignore(mappingbyId[FieldDescriptionId.FootprintSRS].Ignore);

            Map(m => m.Location.FootprintSpatialFit)
                .Name(mappingbyId[FieldDescriptionId.FootprintSpatialFit].Name)
                .Index(mappingbyId[FieldDescriptionId.FootprintSpatialFit].Index)
                .Ignore(mappingbyId[FieldDescriptionId.FootprintSpatialFit].Ignore);

            Map(m => m.Location.GeoreferencedBy)
                .Name(mappingbyId[FieldDescriptionId.GeoreferencedBy].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferencedBy].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferencedBy].Ignore);

            Map(m => m.Location.GeoreferencedDate)
                .Name(mappingbyId[FieldDescriptionId.GeoreferencedDate].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferencedDate].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferencedDate].Ignore);

            Map(m => m.Location.GeoreferenceProtocol)
                .Name(mappingbyId[FieldDescriptionId.GeoreferenceProtocol].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferenceProtocol].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferenceProtocol].Ignore);

            Map(m => m.Location.GeoreferenceSources)
                .Name(mappingbyId[FieldDescriptionId.GeoreferenceSources].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferenceSources].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferenceSources].Ignore);

            Map(m => m.Location.GeoreferenceVerificationStatus)
                .Name(mappingbyId[FieldDescriptionId.GeoreferenceVerificationStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferenceVerificationStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferenceVerificationStatus].Ignore);

            Map(m => m.Location.GeoreferenceRemarks)
                .Name(mappingbyId[FieldDescriptionId.GeoreferenceRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.GeoreferenceRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeoreferenceRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Taxon.TaxonID)
                .Name(mappingbyId[FieldDescriptionId.TaxonID].Name)
                .Index(mappingbyId[FieldDescriptionId.TaxonID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TaxonID].Ignore);

            Map(m => m.Taxon.ScientificNameID)
                .Name(mappingbyId[FieldDescriptionId.ScientificNameID].Name)
                .Index(mappingbyId[FieldDescriptionId.ScientificNameID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ScientificNameID].Ignore);

            Map(m => m.Taxon.AcceptedNameUsageID)
                .Name(mappingbyId[FieldDescriptionId.AcceptedNameUsageID].Name)
                .Index(mappingbyId[FieldDescriptionId.AcceptedNameUsageID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AcceptedNameUsageID].Ignore);

            Map(m => m.Taxon.ParentNameUsageID)
                .Name(mappingbyId[FieldDescriptionId.ParentNameUsageID].Name)
                .Index(mappingbyId[FieldDescriptionId.ParentNameUsageID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ParentNameUsageID].Ignore);

            Map(m => m.Taxon.OriginalNameUsageID)
                .Name(mappingbyId[FieldDescriptionId.OriginalNameUsageID].Name)
                .Index(mappingbyId[FieldDescriptionId.OriginalNameUsageID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OriginalNameUsageID].Ignore);

            Map(m => m.Taxon.NameAccordingToID)
                .Name(mappingbyId[FieldDescriptionId.NameAccordingToID].Name)
                .Index(mappingbyId[FieldDescriptionId.NameAccordingToID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NameAccordingToID].Ignore);

            Map(m => m.Taxon.NamePublishedInID)
                .Name(mappingbyId[FieldDescriptionId.NamePublishedInID].Name)
                .Index(mappingbyId[FieldDescriptionId.NamePublishedInID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NamePublishedInID].Ignore);

            Map(m => m.Taxon.TaxonConceptID)
                .Name(mappingbyId[FieldDescriptionId.TaxonConceptID].Name)
                .Index(mappingbyId[FieldDescriptionId.TaxonConceptID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TaxonConceptID].Ignore);

            Map(m => m.Taxon.ScientificName)
                .Name(mappingbyId[FieldDescriptionId.ScientificName].Name)
                .Index(mappingbyId[FieldDescriptionId.ScientificName].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ScientificName].Ignore);

            Map(m => m.Taxon.AcceptedNameUsage)
                .Name(mappingbyId[FieldDescriptionId.AcceptedNameUsage].Name)
                .Index(mappingbyId[FieldDescriptionId.AcceptedNameUsage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AcceptedNameUsage].Ignore);

            Map(m => m.Taxon.ParentNameUsage)
                .Name(mappingbyId[FieldDescriptionId.ParentNameUsage].Name)
                .Index(mappingbyId[FieldDescriptionId.ParentNameUsage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ParentNameUsage].Ignore);

            Map(m => m.Taxon.OriginalNameUsage)
                .Name(mappingbyId[FieldDescriptionId.OriginalNameUsage].Name)
                .Index(mappingbyId[FieldDescriptionId.OriginalNameUsage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OriginalNameUsage].Ignore);

            Map(m => m.Taxon.NameAccordingTo)
                .Name(mappingbyId[FieldDescriptionId.NameAccordingTo].Name)
                .Index(mappingbyId[FieldDescriptionId.NameAccordingTo].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NameAccordingTo].Ignore);

            Map(m => m.Taxon.NamePublishedIn)
                .Name(mappingbyId[FieldDescriptionId.NamePublishedIn].Name)
                .Index(mappingbyId[FieldDescriptionId.NamePublishedIn].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NamePublishedIn].Ignore);

            Map(m => m.Taxon.NamePublishedInYear)
                .Name(mappingbyId[FieldDescriptionId.NamePublishedInYear].Name)
                .Index(mappingbyId[FieldDescriptionId.NamePublishedInYear].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NamePublishedInYear].Ignore);

            Map(m => m.Taxon.HigherClassification)
                .Name(mappingbyId[FieldDescriptionId.HigherClassification].Name)
                .Index(mappingbyId[FieldDescriptionId.HigherClassification].Index)
                .Ignore(mappingbyId[FieldDescriptionId.HigherClassification].Ignore);

            Map(m => m.Taxon.Kingdom)
                .Name(mappingbyId[FieldDescriptionId.Kingdom].Name)
                .Index(mappingbyId[FieldDescriptionId.Kingdom].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Kingdom].Ignore);

            Map(m => m.Taxon.Phylum)
                .Name(mappingbyId[FieldDescriptionId.Phylum].Name)
                .Index(mappingbyId[FieldDescriptionId.Phylum].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Phylum].Ignore);

            Map(m => m.Taxon.Class)
                .Name(mappingbyId[FieldDescriptionId.Class].Name)
                .Index(mappingbyId[FieldDescriptionId.Class].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Class].Ignore);

            Map(m => m.Taxon.Order)
                .Name(mappingbyId[FieldDescriptionId.Order].Name)
                .Index(mappingbyId[FieldDescriptionId.Order].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Order].Ignore);

            Map(m => m.Taxon.Family)
                .Name(mappingbyId[FieldDescriptionId.Family].Name)
                .Index(mappingbyId[FieldDescriptionId.Family].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Family].Ignore);

            Map(m => m.Taxon.Genus)
                .Name(mappingbyId[FieldDescriptionId.Genus].Name)
                .Index(mappingbyId[FieldDescriptionId.Genus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Genus].Ignore);

            Map(m => m.Taxon.Subgenus)
                .Name(mappingbyId[FieldDescriptionId.Subgenus].Name)
                .Index(mappingbyId[FieldDescriptionId.Subgenus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Subgenus].Ignore);

            Map(m => m.Taxon.SpecificEpithet)
                .Name(mappingbyId[FieldDescriptionId.SpecificEpithet].Name)
                .Index(mappingbyId[FieldDescriptionId.SpecificEpithet].Index)
                .Ignore(mappingbyId[FieldDescriptionId.SpecificEpithet].Ignore);

            Map(m => m.Taxon.InfraspecificEpithet)
                .Name(mappingbyId[FieldDescriptionId.InfraspecificEpithet].Name)
                .Index(mappingbyId[FieldDescriptionId.InfraspecificEpithet].Index)
                .Ignore(mappingbyId[FieldDescriptionId.InfraspecificEpithet].Ignore);

            Map(m => m.Taxon.TaxonRank)
                .Name(mappingbyId[FieldDescriptionId.TaxonRank].Name)
                .Index(mappingbyId[FieldDescriptionId.TaxonRank].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TaxonRank].Ignore);

            Map(m => m.Taxon.VerbatimTaxonRank)
                .Name(mappingbyId[FieldDescriptionId.VerbatimTaxonRank].Name)
                .Index(mappingbyId[FieldDescriptionId.VerbatimTaxonRank].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VerbatimTaxonRank].Ignore);

            Map(m => m.Taxon.ScientificNameAuthorship)
                .Name(mappingbyId[FieldDescriptionId.ScientificNameAuthorship].Name)
                .Index(mappingbyId[FieldDescriptionId.ScientificNameAuthorship].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ScientificNameAuthorship].Ignore);

            Map(m => m.Taxon.VernacularName)
                .Name(mappingbyId[FieldDescriptionId.VernacularName].Name)
                .Index(mappingbyId[FieldDescriptionId.VernacularName].Index)
                .Ignore(mappingbyId[FieldDescriptionId.VernacularName].Ignore);

            Map(m => m.Taxon.NomenclaturalCode)
                .Name(mappingbyId[FieldDescriptionId.NomenclaturalCode].Name)
                .Index(mappingbyId[FieldDescriptionId.NomenclaturalCode].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NomenclaturalCode].Ignore);

            Map(m => m.Taxon.TaxonomicStatus)
                .Name(mappingbyId[FieldDescriptionId.TaxonomicStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.TaxonomicStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TaxonomicStatus].Ignore);

            Map(m => m.Taxon.NomenclaturalStatus)
                .Name(mappingbyId[FieldDescriptionId.NomenclaturalStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.NomenclaturalStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.NomenclaturalStatus].Ignore);

            Map(m => m.Taxon.TaxonRemarks)
                .Name(mappingbyId[FieldDescriptionId.TaxonRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.TaxonRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TaxonRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Identification.IdentificationID)
                .Name(mappingbyId[FieldDescriptionId.IdentificationID].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentificationID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentificationID].Ignore);

            Map(m => m.Identification.IdentificationQualifier)
                .Name(mappingbyId[FieldDescriptionId.IdentificationQualifier].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentificationQualifier].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentificationQualifier].Ignore);

            Map(m => m.Identification.TypeStatus)
                .Name(mappingbyId[FieldDescriptionId.TypeStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.TypeStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.TypeStatus].Ignore);

            Map(m => m.Identification.IdentifiedBy)
                .Name(mappingbyId[FieldDescriptionId.IdentifiedBy].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentifiedBy].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentifiedBy].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Identification.DateIdentified)
                .Name(mappingbyId[FieldDescriptionId.DateIdentified].Name)
                .Index(mappingbyId[FieldDescriptionId.DateIdentified].Index)
                .Ignore(mappingbyId[FieldDescriptionId.DateIdentified].Ignore);

            Map(m => m.Identification.IdentificationReferences)
                .Name(mappingbyId[FieldDescriptionId.IdentificationReferences].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentificationReferences].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentificationReferences].Ignore);

            Map(m => m.Identification.IdentificationVerificationStatus)
                .Name(mappingbyId[FieldDescriptionId.IdentificationVerificationStatus].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentificationVerificationStatus].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentificationVerificationStatus].Ignore);

            Map(m => m.Identification.IdentificationRemarks)
                .Name(mappingbyId[FieldDescriptionId.IdentificationRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.IdentificationRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.IdentificationRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.MaterialSample.MaterialSampleID)
                .Name(mappingbyId[FieldDescriptionId.MaterialSampleID].Name)
                .Index(mappingbyId[FieldDescriptionId.MaterialSampleID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MaterialSampleID].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementID)
                .Name(mappingbyId[FieldDescriptionId.MeasurementID].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementID].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementType)
                .Name(mappingbyId[FieldDescriptionId.MeasurementType].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementType].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementType].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementValue)
                .Name(mappingbyId[FieldDescriptionId.MeasurementValue].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementValue].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementValue].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementAccuracy)
                .Name(mappingbyId[FieldDescriptionId.MeasurementAccuracy].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementAccuracy].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementAccuracy].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementUnit)
                .Name(mappingbyId[FieldDescriptionId.MeasurementUnit].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementUnit].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementUnit].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementDeterminedBy)
                .Name(mappingbyId[FieldDescriptionId.MeasurementDeterminedBy].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementDeterminedBy].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementDeterminedBy].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementDeterminedDate)
                .Name(mappingbyId[FieldDescriptionId.MeasurementDeterminedDate].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementDeterminedDate].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementDeterminedDate].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementMethod)
                .Name(mappingbyId[FieldDescriptionId.MeasurementMethod].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementMethod].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementMethod].Ignore);

            Map(m => m.MeasurementOrFact.MeasurementRemarks)
                .Name(mappingbyId[FieldDescriptionId.MeasurementRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.MeasurementRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.MeasurementRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.ResourceRelationship.ResourceRelationshipID)
                .Name(mappingbyId[FieldDescriptionId.ResourceRelationshipID].Name)
                .Index(mappingbyId[FieldDescriptionId.ResourceRelationshipID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ResourceRelationshipID].Ignore);

            Map(m => m.ResourceRelationship.ResourceID)
                .Name(mappingbyId[FieldDescriptionId.ResourceID].Name)
                .Index(mappingbyId[FieldDescriptionId.ResourceID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.ResourceID].Ignore);

            Map(m => m.ResourceRelationship.RelatedResourceID)
                .Name(mappingbyId[FieldDescriptionId.RelatedResourceID].Name)
                .Index(mappingbyId[FieldDescriptionId.RelatedResourceID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RelatedResourceID].Ignore);

            Map(m => m.ResourceRelationship.RelationshipOfResource)
                .Name(mappingbyId[FieldDescriptionId.RelationshipOfResource].Name)
                .Index(mappingbyId[FieldDescriptionId.RelationshipOfResource].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RelationshipOfResource].Ignore);

            Map(m => m.ResourceRelationship.RelationshipAccordingTo)
                .Name(mappingbyId[FieldDescriptionId.RelationshipAccordingTo].Name)
                .Index(mappingbyId[FieldDescriptionId.RelationshipAccordingTo].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RelationshipAccordingTo].Ignore);

            Map(m => m.ResourceRelationship.RelationshipEstablishedDate)
                .Name(mappingbyId[FieldDescriptionId.RelationshipEstablishedDate].Name)
                .Index(mappingbyId[FieldDescriptionId.RelationshipEstablishedDate].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RelationshipEstablishedDate].Ignore);

            Map(m => m.ResourceRelationship.RelationshipRemarks)
                .Name(mappingbyId[FieldDescriptionId.RelationshipRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.RelationshipRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.RelationshipRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.GeologicalContext.GeologicalContextID)
                .Name(mappingbyId[FieldDescriptionId.GeologicalContextID].Name)
                .Index(mappingbyId[FieldDescriptionId.GeologicalContextID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.GeologicalContextID].Ignore);

            Map(m => m.GeologicalContext.EarliestEonOrLowestEonothem)
                .Name(mappingbyId[FieldDescriptionId.EarliestEonOrLowestEonothem].Name)
                .Index(mappingbyId[FieldDescriptionId.EarliestEonOrLowestEonothem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EarliestEonOrLowestEonothem].Ignore);

            Map(m => m.GeologicalContext.LatestEonOrHighestEonothem)
                .Name(mappingbyId[FieldDescriptionId.LatestEonOrHighestEonothem].Name)
                .Index(mappingbyId[FieldDescriptionId.LatestEonOrHighestEonothem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LatestEonOrHighestEonothem].Ignore);

            Map(m => m.GeologicalContext.EarliestEraOrLowestErathem)
                .Name(mappingbyId[FieldDescriptionId.EarliestEraOrLowestErathem].Name)
                .Index(mappingbyId[FieldDescriptionId.EarliestEraOrLowestErathem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EarliestEraOrLowestErathem].Ignore);

            Map(m => m.GeologicalContext.LatestEraOrHighestErathem)
                .Name(mappingbyId[FieldDescriptionId.LatestEraOrHighestErathem].Name)
                .Index(mappingbyId[FieldDescriptionId.LatestEraOrHighestErathem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LatestEraOrHighestErathem].Ignore);

            Map(m => m.GeologicalContext.EarliestPeriodOrLowestSystem)
                .Name(mappingbyId[FieldDescriptionId.EarliestPeriodOrLowestSystem].Name)
                .Index(mappingbyId[FieldDescriptionId.EarliestPeriodOrLowestSystem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EarliestPeriodOrLowestSystem].Ignore);

            Map(m => m.GeologicalContext.LatestPeriodOrHighestSystem)
                .Name(mappingbyId[FieldDescriptionId.LatestPeriodOrHighestSystem].Name)
                .Index(mappingbyId[FieldDescriptionId.LatestPeriodOrHighestSystem].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LatestPeriodOrHighestSystem].Ignore);

            Map(m => m.GeologicalContext.EarliestEpochOrLowestSeries)
                .Name(mappingbyId[FieldDescriptionId.EarliestEpochOrLowestSeries].Name)
                .Index(mappingbyId[FieldDescriptionId.EarliestEpochOrLowestSeries].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EarliestEpochOrLowestSeries].Ignore);

            Map(m => m.GeologicalContext.LatestEpochOrHighestSeries)
                .Name(mappingbyId[FieldDescriptionId.LatestEpochOrHighestSeries].Name)
                .Index(mappingbyId[FieldDescriptionId.LatestEpochOrHighestSeries].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LatestEpochOrHighestSeries].Ignore);

            Map(m => m.GeologicalContext.EarliestAgeOrLowestStage)
                .Name(mappingbyId[FieldDescriptionId.EarliestAgeOrLowestStage].Name)
                .Index(mappingbyId[FieldDescriptionId.EarliestAgeOrLowestStage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.EarliestAgeOrLowestStage].Ignore);

            Map(m => m.GeologicalContext.LatestAgeOrHighestStage)
                .Name(mappingbyId[FieldDescriptionId.LatestAgeOrHighestStage].Name)
                .Index(mappingbyId[FieldDescriptionId.LatestAgeOrHighestStage].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LatestAgeOrHighestStage].Ignore);

            Map(m => m.GeologicalContext.LowestBiostratigraphicZone)
                .Name(mappingbyId[FieldDescriptionId.LowestBiostratigraphicZone].Name)
                .Index(mappingbyId[FieldDescriptionId.LowestBiostratigraphicZone].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LowestBiostratigraphicZone].Ignore);

            Map(m => m.GeologicalContext.HighestBiostratigraphicZone)
                .Name(mappingbyId[FieldDescriptionId.HighestBiostratigraphicZone].Name)
                .Index(mappingbyId[FieldDescriptionId.HighestBiostratigraphicZone].Index)
                .Ignore(mappingbyId[FieldDescriptionId.HighestBiostratigraphicZone].Ignore);

            Map(m => m.GeologicalContext.LithostratigraphicTerms)
                .Name(mappingbyId[FieldDescriptionId.LithostratigraphicTerms].Name)
                .Index(mappingbyId[FieldDescriptionId.LithostratigraphicTerms].Index)
                .Ignore(mappingbyId[FieldDescriptionId.LithostratigraphicTerms].Ignore);

            Map(m => m.GeologicalContext.Group)
                .Name(mappingbyId[FieldDescriptionId.Group].Name)
                .Index(mappingbyId[FieldDescriptionId.Group].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Group].Ignore);

            Map(m => m.GeologicalContext.Formation)
                .Name(mappingbyId[FieldDescriptionId.Formation].Name)
                .Index(mappingbyId[FieldDescriptionId.Formation].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Formation].Ignore);

            Map(m => m.GeologicalContext.Member)
                .Name(mappingbyId[FieldDescriptionId.Member].Name)
                .Index(mappingbyId[FieldDescriptionId.Member].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Member].Ignore);

            Map(m => m.GeologicalContext.Bed)
                .Name(mappingbyId[FieldDescriptionId.Bed].Name)
                .Index(mappingbyId[FieldDescriptionId.Bed].Index)
                .Ignore(mappingbyId[FieldDescriptionId.Bed].Ignore);

            Map(m => m.Organism.OrganismID)
                .Name(mappingbyId[FieldDescriptionId.OrganismID].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismID].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismID].Ignore);

            Map(m => m.Organism.AssociatedOrganisms)
                .Name(mappingbyId[FieldDescriptionId.AssociatedOrganisms].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedOrganisms].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedOrganisms].Ignore);

            Map(m => m.Organism.AssociatedOccurrences)
                .Name(mappingbyId[FieldDescriptionId.AssociatedOccurrences].Name)
                .Index(mappingbyId[FieldDescriptionId.AssociatedOccurrences].Index)
                .Ignore(mappingbyId[FieldDescriptionId.AssociatedOccurrences].Ignore);

            Map(m => m.Organism.OrganismName)
                .Name(mappingbyId[FieldDescriptionId.OrganismName].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismName].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismName].Ignore);

            Map(m => m.Organism.OrganismRemarks)
                .Name(mappingbyId[FieldDescriptionId.OrganismRemarks].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismRemarks].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismRemarks].Ignore)
                .TypeConverter<LineBreakTabStringConverter<string>>();

            Map(m => m.Organism.OrganismScope)
                .Name(mappingbyId[FieldDescriptionId.OrganismScope].Name)
                .Index(mappingbyId[FieldDescriptionId.OrganismScope].Index)
                .Ignore(mappingbyId[FieldDescriptionId.OrganismScope].Ignore);

            Map(m => m.Organism.PreviousIdentifications)
                .Name(mappingbyId[FieldDescriptionId.PreviousIdentifications].Name)
                .Index(mappingbyId[FieldDescriptionId.PreviousIdentifications].Index)
                .Ignore(mappingbyId[FieldDescriptionId.PreviousIdentifications].Ignore);
        }


        private Dictionary<FieldDescriptionId, MappingItem> CreateMappingDictionary(
            IEnumerable<FieldDescriptionId> fieldDescriptionIds)
        {
            var dic = new Dictionary<FieldDescriptionId, MappingItem>();
            var index = 1;
            foreach (var fieldDescriptionId in fieldDescriptionIds)
            {
                var mappingItem = new MappingItem
                {
                    FieldDescriptionId = fieldDescriptionId,
                    Ignore = false,
                    Index = index,
                    Name = FieldDescriptionHelper.GetFieldDescription(fieldDescriptionId).Name
                };

                dic.Add(fieldDescriptionId, mappingItem);
                index++;
            }

            var missingFieldDescriptionIds = FieldDescriptionHelper.GetMissingFieldDescriptionIds(fieldDescriptionIds);
            foreach (var fieldDescriptionId in missingFieldDescriptionIds)
            {
                var mappingItem = new MappingItem
                {
                    FieldDescriptionId = fieldDescriptionId,
                    Ignore = true,
                    Index = int.MaxValue,
                    Name = FieldDescriptionHelper.GetFieldDescription(fieldDescriptionId).Name
                };

                dic.Add(fieldDescriptionId, mappingItem);
            }

            return dic;
        }

        public static Dictionary<FieldDescriptionId, Expression<Func<DarwinCore, object>>>
            CreateVocabularyDictionary()
        {
            var dic = new Dictionary<FieldDescriptionId, Expression<Func<DarwinCore, object>>>();

            // Occurrence
            dic.Add(FieldDescriptionId.OccurrenceID, m => m.Occurrence.OccurrenceID);
            dic.Add(FieldDescriptionId.CatalogNumber, m => m.Occurrence.CatalogNumber);
            dic.Add(FieldDescriptionId.RecordNumber, m => m.Occurrence.RecordNumber);
            dic.Add(FieldDescriptionId.RecordedBy, m => m.Occurrence.RecordedBy);
            dic.Add(FieldDescriptionId.IndividualCount, m => m.Occurrence.IndividualCount);
            dic.Add(FieldDescriptionId.OrganismQuantity, m => m.Occurrence.OrganismQuantity);
            dic.Add(FieldDescriptionId.OrganismQuantityType, m => m.Occurrence.OrganismQuantityType);
            dic.Add(FieldDescriptionId.Sex, m => m.Occurrence.Sex);
            dic.Add(FieldDescriptionId.LifeStage, m => m.Occurrence.LifeStage);
            dic.Add(FieldDescriptionId.ReproductiveCondition, m => m.Occurrence.ReproductiveCondition);
            dic.Add(FieldDescriptionId.Behavior, m => m.Occurrence.Behavior);
            dic.Add(FieldDescriptionId.EstablishmentMeans, m => m.Occurrence.EstablishmentMeans);
            dic.Add(FieldDescriptionId.OccurrenceStatus, m => m.Occurrence.OccurrenceStatus);
            dic.Add(FieldDescriptionId.Preparations, m => m.Occurrence.Preparations);
            dic.Add(FieldDescriptionId.Disposition, m => m.Occurrence.Disposition);
            dic.Add(FieldDescriptionId.AssociatedMedia, m => m.Occurrence.AssociatedMedia);
            dic.Add(FieldDescriptionId.AssociatedReferences, m => m.Occurrence.AssociatedReferences);
            dic.Add(FieldDescriptionId.AssociatedSequences, m => m.Occurrence.AssociatedSequences);
            dic.Add(FieldDescriptionId.AssociatedTaxa, m => m.Occurrence.AssociatedTaxa);
            dic.Add(FieldDescriptionId.OtherCatalogNumbers, m => m.Occurrence.OtherCatalogNumbers);
            dic.Add(FieldDescriptionId.OccurrenceRemarks, m => m.Occurrence.OccurrenceRemarks);

            // DarwinCore (Record level)
            dic.Add(FieldDescriptionId.DatasetID, m => m.DatasetID);
            dic.Add(FieldDescriptionId.Type, m => m.Type);
            dic.Add(FieldDescriptionId.Modified, m => m.Modified);
            dic.Add(FieldDescriptionId.Language, m => m.Language);
            dic.Add(FieldDescriptionId.License, m => m.License);
            dic.Add(FieldDescriptionId.RightsHolder, m => m.RightsHolder);
            dic.Add(FieldDescriptionId.AccessRights, m => m.AccessRights);
            dic.Add(FieldDescriptionId.BibliographicCitation, m => m.BibliographicCitation);
            dic.Add(FieldDescriptionId.References, m => m.References);
            dic.Add(FieldDescriptionId.InstitutionID, m => m.InstitutionID);
            dic.Add(FieldDescriptionId.CollectionID, m => m.CollectionID);
            dic.Add(FieldDescriptionId.InstitutionCode, m => m.InstitutionCode);
            dic.Add(FieldDescriptionId.CollectionCode, m => m.CollectionCode);
            dic.Add(FieldDescriptionId.DatasetName, m => m.DatasetName);
            dic.Add(FieldDescriptionId.OwnerInstitutionCode, m => m.OwnerInstitutionCode);
            dic.Add(FieldDescriptionId.BasisOfRecord, m => m.BasisOfRecord);
            dic.Add(FieldDescriptionId.InformationWithheld, m => m.InformationWithheld);
            dic.Add(FieldDescriptionId.DataGeneralizations, m => m.DataGeneralizations);
            dic.Add(FieldDescriptionId.DynamicProperties, m => m.DynamicProperties);

            // Event
            dic.Add(FieldDescriptionId.EventID, m => m.Event.EventID);
            dic.Add(FieldDescriptionId.ParentEventID, m => m.Event.ParentEventID);
            dic.Add(FieldDescriptionId.FieldNumber, m => m.Event.FieldNumber);
            dic.Add(FieldDescriptionId.EventDate, m => m.Event.EventDate);
            dic.Add(FieldDescriptionId.EventTime, m => m.Event.EventTime);
            dic.Add(FieldDescriptionId.StartDayOfYear, m => m.Event.StartDayOfYear);
            dic.Add(FieldDescriptionId.EndDayOfYear, m => m.Event.EndDayOfYear);
            dic.Add(FieldDescriptionId.Year, m => m.Event.Year);
            dic.Add(FieldDescriptionId.Month, m => m.Event.Month);
            dic.Add(FieldDescriptionId.Day, m => m.Event.Day);
            dic.Add(FieldDescriptionId.VerbatimEventDate, m => m.Event.VerbatimEventDate);
            dic.Add(FieldDescriptionId.Habitat, m => m.Event.Habitat);
            dic.Add(FieldDescriptionId.SamplingProtocol, m => m.Event.SamplingProtocol);
            dic.Add(FieldDescriptionId.SampleSizeValue, m => m.Event.SampleSizeValue);
            dic.Add(FieldDescriptionId.SampleSizeUnit, m => m.Event.SampleSizeUnit);
            dic.Add(FieldDescriptionId.SamplingEffort, m => m.Event.SamplingEffort);
            dic.Add(FieldDescriptionId.FieldNotes, m => m.Event.FieldNotes);
            dic.Add(FieldDescriptionId.EventRemarks, m => m.Event.EventRemarks);

            // Location
            dic.Add(FieldDescriptionId.LocationID, m => m.Location.LocationID);
            dic.Add(FieldDescriptionId.HigherGeographyID, m => m.Location.HigherGeographyID);
            dic.Add(FieldDescriptionId.HigherGeography, m => m.Location.HigherGeography);
            dic.Add(FieldDescriptionId.Continent, m => m.Location.Continent);
            dic.Add(FieldDescriptionId.WaterBody, m => m.Location.WaterBody);
            dic.Add(FieldDescriptionId.IslandGroup, m => m.Location.IslandGroup);
            dic.Add(FieldDescriptionId.Island, m => m.Location.Island);
            dic.Add(FieldDescriptionId.Country, m => m.Location.Country);
            dic.Add(FieldDescriptionId.CountryCode, m => m.Location.CountryCode);
            dic.Add(FieldDescriptionId.StateProvince, m => m.Location.StateProvince);
            dic.Add(FieldDescriptionId.County, m => m.Location.County);
            dic.Add(FieldDescriptionId.Municipality, m => m.Location.Municipality);
            dic.Add(FieldDescriptionId.Locality, m => m.Location.Locality);
            dic.Add(FieldDescriptionId.VerbatimLocality, m => m.Location.VerbatimLocality);
            dic.Add(FieldDescriptionId.MinimumElevationInMeters, m => m.Location.MinimumElevationInMeters);
            dic.Add(FieldDescriptionId.MaximumElevationInMeters, m => m.Location.MaximumElevationInMeters);
            dic.Add(FieldDescriptionId.VerbatimElevation, m => m.Location.VerbatimElevation);
            dic.Add(FieldDescriptionId.MinimumDepthInMeters, m => m.Location.MinimumDepthInMeters);
            dic.Add(FieldDescriptionId.MaximumDepthInMeters, m => m.Location.MaximumDepthInMeters);
            dic.Add(FieldDescriptionId.VerbatimDepth, m => m.Location.VerbatimDepth);
            dic.Add(FieldDescriptionId.MinimumDistanceAboveSurfaceInMeters,
                m => m.Location.MinimumDistanceAboveSurfaceInMeters);
            dic.Add(FieldDescriptionId.MaximumDistanceAboveSurfaceInMeters,
                m => m.Location.MaximumDistanceAboveSurfaceInMeters);
            dic.Add(FieldDescriptionId.LocationAccordingTo, m => m.Location.LocationAccordingTo);
            dic.Add(FieldDescriptionId.LocationRemarks, m => m.Location.LocationRemarks);
            dic.Add(FieldDescriptionId.DecimalLatitude, m => m.Location.DecimalLatitude);
            dic.Add(FieldDescriptionId.DecimalLongitude, m => m.Location.DecimalLongitude);
            dic.Add(FieldDescriptionId.GeodeticDatum, m => m.Location.GeodeticDatum);
            dic.Add(FieldDescriptionId.CoordinateUncertaintyInMeters, m => m.Location.CoordinateUncertaintyInMeters);
            dic.Add(FieldDescriptionId.CoordinatePrecision, m => m.Location.CoordinatePrecision);
            dic.Add(FieldDescriptionId.PointRadiusSpatialFit, m => m.Location.PointRadiusSpatialFit);
            dic.Add(FieldDescriptionId.VerbatimCoordinates, m => m.Location.VerbatimCoordinates);
            dic.Add(FieldDescriptionId.VerbatimLatitude, m => m.Location.VerbatimLatitude);
            dic.Add(FieldDescriptionId.VerbatimLongitude, m => m.Location.VerbatimLongitude);
            dic.Add(FieldDescriptionId.VerbatimCoordinateSystem, m => m.Location.VerbatimCoordinateSystem);
            dic.Add(FieldDescriptionId.VerbatimSRS, m => m.Location.VerbatimSRS);
            dic.Add(FieldDescriptionId.FootprintWKT, m => m.Location.FootprintWKT);
            dic.Add(FieldDescriptionId.FootprintSRS, m => m.Location.FootprintSRS);
            dic.Add(FieldDescriptionId.FootprintSpatialFit, m => m.Location.FootprintSpatialFit);
            dic.Add(FieldDescriptionId.GeoreferencedBy, m => m.Location.GeoreferencedBy);
            dic.Add(FieldDescriptionId.GeoreferencedDate, m => m.Location.GeoreferencedDate);
            dic.Add(FieldDescriptionId.GeoreferenceProtocol, m => m.Location.GeoreferenceProtocol);
            dic.Add(FieldDescriptionId.GeoreferenceSources, m => m.Location.GeoreferenceSources);
            dic.Add(FieldDescriptionId.GeoreferenceVerificationStatus, m => m.Location.GeoreferenceVerificationStatus);
            dic.Add(FieldDescriptionId.GeoreferenceRemarks, m => m.Location.GeoreferenceRemarks);

            // Taxon
            dic.Add(FieldDescriptionId.TaxonID, m => m.Taxon.TaxonID);
            dic.Add(FieldDescriptionId.ScientificNameID, m => m.Taxon.ScientificNameID);
            dic.Add(FieldDescriptionId.AcceptedNameUsageID, m => m.Taxon.AcceptedNameUsageID);
            dic.Add(FieldDescriptionId.ParentNameUsageID, m => m.Taxon.ParentNameUsageID);
            dic.Add(FieldDescriptionId.OriginalNameUsageID, m => m.Taxon.OriginalNameUsageID);
            dic.Add(FieldDescriptionId.NameAccordingToID, m => m.Taxon.NameAccordingToID);
            dic.Add(FieldDescriptionId.NamePublishedInID, m => m.Taxon.NamePublishedInID);
            dic.Add(FieldDescriptionId.TaxonConceptID, m => m.Taxon.TaxonConceptID);
            dic.Add(FieldDescriptionId.ScientificName, m => m.Taxon.ScientificName);
            dic.Add(FieldDescriptionId.AcceptedNameUsage, m => m.Taxon.AcceptedNameUsage);
            dic.Add(FieldDescriptionId.ParentNameUsage, m => m.Taxon.ParentNameUsage);
            dic.Add(FieldDescriptionId.OriginalNameUsage, m => m.Taxon.OriginalNameUsage);
            dic.Add(FieldDescriptionId.NameAccordingTo, m => m.Taxon.NameAccordingTo);
            dic.Add(FieldDescriptionId.NamePublishedIn, m => m.Taxon.NamePublishedIn);
            dic.Add(FieldDescriptionId.NamePublishedInYear, m => m.Taxon.NamePublishedInYear);
            dic.Add(FieldDescriptionId.HigherClassification, m => m.Taxon.HigherClassification);
            dic.Add(FieldDescriptionId.Kingdom, m => m.Taxon.Kingdom);
            dic.Add(FieldDescriptionId.Phylum, m => m.Taxon.Phylum);
            dic.Add(FieldDescriptionId.Class, m => m.Taxon.Class);
            dic.Add(FieldDescriptionId.Order, m => m.Taxon.Order);
            dic.Add(FieldDescriptionId.Family, m => m.Taxon.Family);
            dic.Add(FieldDescriptionId.Genus, m => m.Taxon.Genus);
            dic.Add(FieldDescriptionId.Subgenus, m => m.Taxon.Subgenus);
            dic.Add(FieldDescriptionId.SpecificEpithet, m => m.Taxon.SpecificEpithet);
            dic.Add(FieldDescriptionId.InfraspecificEpithet, m => m.Taxon.InfraspecificEpithet);
            dic.Add(FieldDescriptionId.TaxonRank, m => m.Taxon.TaxonRank);
            dic.Add(FieldDescriptionId.VerbatimTaxonRank, m => m.Taxon.VerbatimTaxonRank);
            dic.Add(FieldDescriptionId.ScientificNameAuthorship, m => m.Taxon.ScientificNameAuthorship);
            dic.Add(FieldDescriptionId.VernacularName, m => m.Taxon.VernacularName);
            dic.Add(FieldDescriptionId.NomenclaturalCode, m => m.Taxon.NomenclaturalCode);
            dic.Add(FieldDescriptionId.TaxonomicStatus, m => m.Taxon.TaxonomicStatus);
            dic.Add(FieldDescriptionId.NomenclaturalStatus, m => m.Taxon.NomenclaturalStatus);
            dic.Add(FieldDescriptionId.TaxonRemarks, m => m.Taxon.TaxonRemarks);

            // Identification
            dic.Add(FieldDescriptionId.IdentificationID, m => m.Identification.IdentificationID);
            dic.Add(FieldDescriptionId.IdentificationQualifier, m => m.Identification.IdentificationQualifier);
            dic.Add(FieldDescriptionId.TypeStatus, m => m.Identification.TypeStatus);
            dic.Add(FieldDescriptionId.IdentifiedBy, m => m.Identification.IdentifiedBy);
            dic.Add(FieldDescriptionId.DateIdentified, m => m.Identification.DateIdentified);
            dic.Add(FieldDescriptionId.IdentificationReferences, m => m.Identification.IdentificationReferences);
            dic.Add(FieldDescriptionId.IdentificationVerificationStatus,
                m => m.Identification.IdentificationVerificationStatus);
            dic.Add(FieldDescriptionId.IdentificationRemarks, m => m.Identification.IdentificationRemarks);

            // Material sample
            dic.Add(FieldDescriptionId.MaterialSampleID, m => m.MaterialSample.MaterialSampleID);

            // MeasurementOrFact
            dic.Add(FieldDescriptionId.MeasurementID, m => m.MeasurementOrFact.MeasurementID);
            dic.Add(FieldDescriptionId.MeasurementType, m => m.MeasurementOrFact.MeasurementType);
            dic.Add(FieldDescriptionId.MeasurementValue, m => m.MeasurementOrFact.MeasurementValue);
            dic.Add(FieldDescriptionId.MeasurementAccuracy, m => m.MeasurementOrFact.MeasurementAccuracy);
            dic.Add(FieldDescriptionId.MeasurementUnit, m => m.MeasurementOrFact.MeasurementUnit);
            dic.Add(FieldDescriptionId.MeasurementDeterminedBy, m => m.MeasurementOrFact.MeasurementDeterminedBy);
            dic.Add(FieldDescriptionId.MeasurementDeterminedDate, m => m.MeasurementOrFact.MeasurementDeterminedDate);
            dic.Add(FieldDescriptionId.MeasurementMethod, m => m.MeasurementOrFact.MeasurementMethod);
            dic.Add(FieldDescriptionId.MeasurementRemarks, m => m.MeasurementOrFact.MeasurementRemarks);

            // ResourceRelationship
            dic.Add(FieldDescriptionId.ResourceRelationshipID, m => m.ResourceRelationship.ResourceRelationshipID);
            dic.Add(FieldDescriptionId.ResourceID, m => m.ResourceRelationship.ResourceID);
            dic.Add(FieldDescriptionId.RelatedResourceID, m => m.ResourceRelationship.RelatedResourceID);
            dic.Add(FieldDescriptionId.RelationshipOfResource, m => m.ResourceRelationship.RelationshipOfResource);
            dic.Add(FieldDescriptionId.RelationshipAccordingTo, m => m.ResourceRelationship.RelationshipAccordingTo);
            dic.Add(FieldDescriptionId.RelationshipEstablishedDate,
                m => m.ResourceRelationship.RelationshipEstablishedDate);
            dic.Add(FieldDescriptionId.RelationshipRemarks, m => m.ResourceRelationship.RelationshipRemarks);


            // GeologicalContext
            dic.Add(FieldDescriptionId.GeologicalContextID, m => m.GeologicalContext.GeologicalContextID);
            dic.Add(FieldDescriptionId.EarliestEonOrLowestEonothem,
                m => m.GeologicalContext.EarliestEonOrLowestEonothem);
            dic.Add(FieldDescriptionId.LatestEonOrHighestEonothem, m => m.GeologicalContext.LatestEonOrHighestEonothem);
            dic.Add(FieldDescriptionId.EarliestEraOrLowestErathem, m => m.GeologicalContext.EarliestEraOrLowestErathem);
            dic.Add(FieldDescriptionId.LatestEraOrHighestErathem, m => m.GeologicalContext.LatestEraOrHighestErathem);
            dic.Add(FieldDescriptionId.EarliestPeriodOrLowestSystem,
                m => m.GeologicalContext.EarliestPeriodOrLowestSystem);
            dic.Add(FieldDescriptionId.LatestPeriodOrHighestSystem,
                m => m.GeologicalContext.LatestPeriodOrHighestSystem);
            dic.Add(FieldDescriptionId.EarliestEpochOrLowestSeries,
                m => m.GeologicalContext.EarliestEpochOrLowestSeries);
            dic.Add(FieldDescriptionId.LatestEpochOrHighestSeries, m => m.GeologicalContext.LatestEpochOrHighestSeries);
            dic.Add(FieldDescriptionId.EarliestAgeOrLowestStage, m => m.GeologicalContext.EarliestAgeOrLowestStage);
            dic.Add(FieldDescriptionId.LatestAgeOrHighestStage, m => m.GeologicalContext.LatestAgeOrHighestStage);
            dic.Add(FieldDescriptionId.LowestBiostratigraphicZone, m => m.GeologicalContext.LowestBiostratigraphicZone);
            dic.Add(FieldDescriptionId.HighestBiostratigraphicZone,
                m => m.GeologicalContext.HighestBiostratigraphicZone);
            dic.Add(FieldDescriptionId.LithostratigraphicTerms, m => m.GeologicalContext.LithostratigraphicTerms);
            dic.Add(FieldDescriptionId.Group, m => m.GeologicalContext.Group);
            dic.Add(FieldDescriptionId.Formation, m => m.GeologicalContext.Formation);
            dic.Add(FieldDescriptionId.Member, m => m.GeologicalContext.Member);
            dic.Add(FieldDescriptionId.Bed, m => m.GeologicalContext.Bed);

            // Organism
            dic.Add(FieldDescriptionId.OrganismID, m => m.Organism.OrganismID);
            dic.Add(FieldDescriptionId.AssociatedOrganisms, m => m.Organism.AssociatedOrganisms);
            dic.Add(FieldDescriptionId.AssociatedOccurrences, m => m.Organism.AssociatedOccurrences);
            dic.Add(FieldDescriptionId.OrganismName, m => m.Organism.OrganismName);
            dic.Add(FieldDescriptionId.OrganismRemarks, m => m.Organism.OrganismRemarks);
            dic.Add(FieldDescriptionId.OrganismScope, m => m.Organism.OrganismScope);
            dic.Add(FieldDescriptionId.PreviousIdentifications, m => m.Organism.PreviousIdentifications);

            return dic;
        }

        private class MappingItem
        {
            public FieldDescriptionId FieldDescriptionId { get; set; }
            public bool Ignore { get; set; }
            public string Name { get; set; }
            public int Index { get; set; }
        }
    }
}