using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using System.Collections.Generic;
using System.Linq;
using LibEnums = SOS.Lib.Models.Processed.DataStewardship.Enums;
using ApiEnums = SOS.Observations.Api.Dtos.DataStewardship.Enums;
using SOS.Lib.Helpers;
using System.IO;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class DatasetExtensions
    {
        private static ApiEnums.DsAccessRights? ToDto(this LibEnums.AccessRights? accessRightsEnum)
        {
            if (accessRightsEnum == null) return null;
            return (ApiEnums.DsAccessRights)accessRightsEnum;
        }

        private static IEnumerable<DsMethodologyDto> ToDtos(this IEnumerable<Methodology> methodologies)
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToDto());
        }

        private static ApiEnums.DsProgrammeArea? ToDto(this LibEnums.ProgrammeArea? programmeArea)
        {
            if (programmeArea == null) return null;
            return (ApiEnums.DsProgrammeArea)programmeArea;
        }

        private static ApiEnums.DsPurpose? ToDatasetPurposeEnum(this LibEnums.Purpose? purposeEnum)
        {
            if (purposeEnum == null) return null;
            return (ApiEnums.DsPurpose)purposeEnum;
        }

        private static DsMethodologyDto ToDto(this Methodology methodology)
        {
            if (methodology == null) return null;
            return new DsMethodologyDto
            {
                MethodologyDescription = methodology.MethodologyDescription,
                MethodologyLink = methodology.MethodologyLink,
                MethodologyName = methodology.MethodologyName,
                SpeciesList = methodology.SpeciesList
            };
        }

        /// <summary>
        /// Cast data set to csv
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this DsDatasetDto dataset)
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
        public static byte[] ToCsv(this IEnumerable<DsDatasetDto> datasets)
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

            foreach (var dataset in datasets)
            {
                csvFileHelper.WriteRow(new[] {
                    dataset.Identifier,
                    dataset.License,
                    dataset.Title,
                    dataset.Projects?.FirstOrDefault()?.ToString(),
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

        public static DsDatasetInfoDto ToDto(this DataStewardshipInfo source)
        {
            if (source == null) return null;
            return new DsDatasetInfoDto
            {
                Identifier = source.DatasetIdentifier,
                Title = source.DatasetTitle,
            };
        }

        public static DsDatasetDto ToDto(this Dataset dataset)
        {
            if (dataset == null) return null;

            return new DsDatasetDto
            {
                AccessRights = dataset.AccessRights.ToDto(),
                DescriptionAccessRights = dataset.DescriptionAccessRights,
                Assigner = dataset.Assigner.ToDto(),
                Creator = dataset?.Creator?.Select(m => m.ToDto())?.ToList(),
                DataStewardship = dataset.DataStewardship,
                Description = dataset.Description,
                EndDate = dataset.EndDate,
                EventIds = dataset.EventIds,
                Identifier = dataset.Identifier,
                Language = dataset.Language,
                Metadatalanguage = dataset.Metadatalanguage,
                Methodology = dataset.Methodology.ToDtos(),
                OwnerinstitutionCode = dataset.OwnerinstitutionCode.ToDto(),
                Projects = dataset.Project?.Select(m => m.ToDto()),
                ProgrammeArea = dataset.ProgrammeArea.ToDto(),
                Publisher = dataset.Publisher.ToDto(),
                Purpose = dataset.Purpose.ToDatasetPurposeEnum(),
                Spatial = dataset.Spatial,
                StartDate = dataset.StartDate,
                Title = dataset.Title
            };
        }
    }
}
