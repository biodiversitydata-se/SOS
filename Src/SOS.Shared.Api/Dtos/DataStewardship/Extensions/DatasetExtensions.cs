using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using LibEnums = SOS.Lib.Models.Processed.DataStewardship.Enums;
using ApiEnums = SOS.Shared.Api.Dtos.DataStewardship.Enums;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions;

public static class DatasetExtensions
{
    extension(LibEnums.AccessRights? accessRightsEnum)
    {
        private ApiEnums.DsAccessRights? ToDto()
        {
            if (accessRightsEnum == null) return null;
            return (ApiEnums.DsAccessRights)accessRightsEnum;
        }
    }

    extension(IEnumerable<Methodology> methodologies)
    {
        private IEnumerable<DsMethodologyDto> ToDtos()
        {
            if (methodologies == null || !methodologies.Any()) return null;
            return methodologies.Select(m => m.ToDto());
        }
    }

    extension(LibEnums.ProgrammeArea? programmeArea)
    {
        private ApiEnums.DsProgrammeArea? ToDto()
        {
            if (programmeArea == null) return null;
            return (ApiEnums.DsProgrammeArea)programmeArea;
        }
    }

    extension(LibEnums.Purpose? purposeEnum)
    {
        private ApiEnums.DsPurpose? ToDatasetPurposeEnum()
        {
            if (purposeEnum == null) return null;
            return (ApiEnums.DsPurpose)purposeEnum;
        }
    }

    extension(Methodology methodology)
    {
        private DsMethodologyDto ToDto()
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
    }

    extension(DsDatasetDto dataset)
    {
        /// <summary>
        /// Cast data set to csv
        /// </summary>
        /// <returns></returns>
        public byte[] ToCsv()
        {
            if (dataset == null)
            {
                return null!;
            }

            return new[] { dataset }.ToCsv();
        }
    }

    extension(IEnumerable<DsDatasetDto> datasets)
    {
        /// <summary>
        /// Caste data sets to csv
        /// </summary>
        /// <returns></returns>
        public byte[] ToCsv()
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
    }

    extension(DataStewardshipInfo source)
    {
        public DsDatasetInfoDto ToDto()
        {
            if (source == null) return null;
            return new DsDatasetInfoDto
            {
                Identifier = source.DatasetIdentifier,
                Title = source.DatasetTitle,
            };
        }
    }

    extension(Dataset dataset)
    {
        public DsDatasetDto ToDto()
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
