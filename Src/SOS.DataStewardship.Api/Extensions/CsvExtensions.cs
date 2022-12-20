using SOS.DataStewardship.Api.Models;
using SOS.Lib.Helpers;
using System.Data;

namespace SOS.DataStewardship.Api.Extensions
{
    public static class CsvExtensions
    {
        private static string Concat(this IEnumerable<string> values, int maxCount = 0)
        {
            if (!values?.Any() ?? true)
            {
                return null!;
            }
           
            if (values.Count() > maxCount)
            {
                return $"{string.Join(',', values.Take(maxCount))}...";
            }

            return string.Join(',', values);
        }

        #region dataset
        /// <summary>
        /// Cast data set to csv
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this Dataset dataset)
        {
            if (dataset == null)
            {
                return null!;
            }

            return new[] { dataset }.ToCsv(); 
        }

        /// <summary>
        /// Caste data sets to csv
        /// </summary>
        /// <param name="datasets"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this IEnumerable<Dataset> datasets)
        {
            if (!datasets?.Any() ?? true)
            {
                return null!;
            }

            using var stream = new MemoryStream();
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(stream, "\t");
            csvFileHelper.WriteRow(new[] {
                "identifier",
                "license",
                "title",
                "project",
                "programme area",
                "assigner",
                "assigner",
                "creator/s",
                "owner institution code",
                "publisher",
                "data stewardship",
                "purpose",
                "description",
                "methodology",
                "start date",
                "end date",
                "spatial",
                "access rights",                                
                "metadata language",
                "language",
                "event id/s"
            });
           
            foreach(var dataset in datasets)
            {
                csvFileHelper.WriteRow(new[] {
                    dataset.Identifier,
                    dataset.License,
                    dataset.Title,
                    dataset.Project?.ToString(),
                    dataset.ProgrammeArea?.ToString(),
                    $"{dataset.Assigner?.OrganisationID}/{dataset.Assigner?.OrganisationCode}",
                    dataset.Creator?.Select(c => $"{c.OrganisationID}/{c.OrganisationCode}")?.Concat(),
                    $"{dataset.OwnerinstitutionCode?.OrganisationID}/{dataset.OwnerinstitutionCode?.OrganisationCode}",
                    $"{dataset.Publisher?.OrganisationID}/{dataset.Publisher?.OrganisationCode}",
                    dataset.DataStewardship,
                    dataset.Purpose?.ToString(),
                    dataset.Description,
                    dataset.Methodology?.Select(m => $"{m.MethodologyName}/{m.MethodologyLink}/{m.MethodologyDescription}")?.Concat(),
                    dataset.StartDate.HasValue ? dataset.StartDate.Value.ToLongDateString() : string.Empty,
                    dataset.EndDate.HasValue ? dataset.EndDate.Value.ToLongDateString() : string.Empty,
                    dataset.Spatial,
                    dataset.AccessRights?.ToString(),
                    dataset.Project?.ToString(),
                    dataset.Metadatalanguage,
                    dataset.Language,
                    dataset.EventIds?.Concat(10)
                });
            }
            csvFileHelper.Flush();
            stream.Position = 0;
            var csv = stream.ToArray();
            csvFileHelper.FinishWrite();

            return csv;
        }
        #endregion dataset

        #region event
        /// <summary>
        /// Cast event model to csv
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this EventModel @event)
        {
            if (@event == null)
            {
                return null!;
            }

            return new[] { @event }.ToCsv();
        }

        /// <summary>
        /// Caste event models to csv
        /// </summary>
        /// <param name="datasets"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this IEnumerable<EventModel> events)
        {
            if (!events?.Any() ?? true)
            {
                return null!;
            }

            using var stream = new MemoryStream();
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(stream, "\t");
            csvFileHelper.WriteRow(new[] {
                "event id",
                "type",
                "parent id",
                "start date",
                "end date",
                "location protected",
                "survey location",
                "recorder code",
                "recorder organisation",
                "sampling protocol",
                "remarks",
                "no observations",
                "data set",
                "occurrence id's"
            });

            foreach (var evt in events)
            {
                csvFileHelper.WriteRow(new[] {
                    evt.EventID,
                    evt.EventType,
                    evt.ParentEventID,
                    evt.EventStartDate?.ToLongDateString(),
                    evt.EventEndDate?.ToLongDateString(),
                    evt.LocationProtected?.ToString(),
                    $"{evt.SurveyLocation?.LocationID}/{evt.SurveyLocation?.Locality}",
                    evt.RecorderCode?.Concat(),
                    evt.RecorderOrganisation?.Select(o => $"{o.OrganisationID}/{o.OrganisationCode}").Concat(),
                    evt.SamplingProtocol,
                    evt.EventRemarks,
                    evt.NoObservations?.ToString(),
                    evt.Dataset?.Identifier,
                    evt.OccurrenceIds?.Concat(10)
                });
            }

            csvFileHelper.Flush();
            stream.Position = 0;
            var csv = stream.ToArray();
            csvFileHelper.FinishWrite();

            return csv;
        }
        #endregion event

        #region occurrence
        /// <summary>
        /// Cast event model to csv
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this OccurrenceModel occurrence)
        {
            if (occurrence == null)
            {
                return null!;
            }

            return new[] { occurrence }.ToCsv();
        }

        /// <summary>
        /// Caste event models to csv
        /// </summary>
        /// <param name="datasets"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this IEnumerable<OccurrenceModel> occurrences)
        {
            if (!occurrences?.Any() ?? true)
            {
                return null!;
            }

            using var stream = new MemoryStream();
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(stream, "\t");
            csvFileHelper.WriteRow(new[] {
                "occurrence id",
                "basis of record",
                "observation time",
                "start date",
                "end date",
                "lon/lat",
                "taxon",
                "status",
                "quantity/variable",
                "quantity",
                "unit",
                "organism",
                "remarks",
                "observationCertainty",
                "identification verification status",
                "associated media",
                "event id",
                "dataset identifier"
            });

            foreach (var occurrence in occurrences)
            {
                var point = (PointGeoShape)occurrence.ObservationPoint;
                var lon = point?.Coordinates.Longitude;
                var lat = point?.Coordinates.Latitude;

                csvFileHelper.WriteRow(new[] {
                    occurrence.OccurrenceID,
                    occurrence.BasisOfRecord?.ToString(),
                    occurrence.ObservationTime?.ToLongDateString(),
                    occurrence.EventStartDate?.ToLongDateString(),
                    occurrence.EventEndDate?.ToLongDateString(),
                    $"{lon}/{lat}",
                    $"{occurrence.Taxon?.TaxonID}/{occurrence.Taxon?.ScientificName}/{occurrence.Taxon?.VernacularName}",
                   occurrence.OccurrenceStatus?.ToString(),
                   occurrence.QuantityVariable?.ToString(),
                   occurrence.Quantity?.ToString(),
                   occurrence.Unit?.ToString(),
                   $"{occurrence.Organism?.Activity?.ToString()}/{occurrence.Organism?.LifeStage?.ToString()}/{occurrence.Organism?.Sex?.ToString()}",
                   occurrence.OccurrenceRemarks,
                   occurrence.ObservationCertainty?.ToString(),
                   occurrence.IdentificationVerificationStatus?.ToString(),
                   occurrence.AssociatedMedia?.Select(m => $"{m.AssociatedMediaName}/{m.AssociatedMediaType}/{m.AssociatedMediaLink}").Concat(),
                   occurrence.EventID,
                   occurrence.DatasetIdentifier
                });
            }

            csvFileHelper.Flush();
            stream.Position = 0;
            var csv = stream.ToArray();
            csvFileHelper.FinishWrite();

            return csv;
        }
        #endregion occurrence
    }
}
