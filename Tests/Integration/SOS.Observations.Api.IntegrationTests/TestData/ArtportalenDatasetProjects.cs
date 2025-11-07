using SOS.Lib.Models.Verbatim.Artportalen;

namespace SOS.Observations.Api.IntegrationTests.TestData;

internal static class ArtportalenDatasetProjects
{
    public const string TreeDatasetIdentifier = "Artportalen - GDP Skyddsvärda träd";
    public const string UtterDatasetIdentifier = "Artportalen - GDP Utter barmarksinventering";
    public const string MouseDatasetIdentifier = "Artportalen - Buskmus";
    public const int TreeProjectId = 5367;
    public const int UtterProjectId = 5998;
    public const int MouseProjectId = 3446;

    public class Mapping
    {
        public Dictionary<int, Project> ProjectById { get; } = GetProjectById();
        public Dictionary<string, Lib.Models.Processed.Observation.ArtportalenDatasetMetadata> DatasetByIdentifier { get; } = GetArtportalenDatasetByIdentifier();
        public Dictionary<int, string> DatasetIdentifierByProjectId { get; init; }
        public Dictionary<string, List<int>> ProjectIdsByDatasetIdentifier { get; init; }
        private static readonly Lazy<Mapping> _instance = new(() => new Mapping());
        public static Mapping Instance => _instance.Value;
        protected Mapping()
        {
            DatasetIdentifierByProjectId = ProjectById.ToDictionary(
                kvp => kvp.Key,
                kvp => DatasetByIdentifier.Values!
                    .FirstOrDefault(d => d.Projects.Any(p => p.ApProjectId == kvp.Key))!.Identifier
            );

            ProjectIdsByDatasetIdentifier = DatasetIdentifierByProjectId
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList());
        }

        public static Mapping CreateInstance()
        {
            return new Mapping();
        }
    }

    public static Dictionary<int, Project> GetProjectById()
    {         
        return _projects.ToDictionary(p => p.Id, p => p);
    }

    public static Dictionary<string, Lib.Models.Processed.Observation.ArtportalenDatasetMetadata> GetArtportalenDatasetByIdentifier()
    {
        return _artportalenDatasetMetadatas.ToDictionary(d => d.Identifier, d => d);
    }

    private static List<Project> _projects = new List<Project>
    {
        new Project
        {
            Id = 5367,
            IsPublic = false,
            Category = "Environmental management",
            CategorySwedish = "Naturvård",
            Description = "Trädportalen är skyddsvärda träd som registrerades i Trädportalen och utgick från Åtgärdsprogrammet för särskilt skyddsvärda träd under perioden 2004–2016.",
            Name = "Trädportalen",
            Owner = "Karin Sandberg",
            ProjectURL = "https://www.artportalen.se/Project/View/5367",
            StartDate = new DateTime(2004, 1, 1),
            EndDate = new DateTime(2016, 12, 31),
            SurveyMethod = "Fältinventering",
            SurveyMethodUrl = "https://www.artportalen.se/Project/View/5367",
            ProjectParameters = new List<ProjectParameter>
            {
                new ProjectParameter { Name = "Skyddsvärde", Description = "Anledningen till utpekandet som observatören upplever det.", DataType = "string" },
                new ProjectParameter { Name = "Ålder", Description = "Uppskattad eller uppmätt totalålder.", Unit = "år", DataType = "double" },
                new ProjectParameter { Name = "Stamomkrets", Description = "Stammens omkrets i centimeter.", Unit = "cm", DataType = "double" },
                new ProjectParameter { Name = "Höjd", Description = "Uppskattad eller uppmätt höjd.", Unit = "m", DataType = "double" },
                new ProjectParameter { Name = "Trädstatus", Description = "Kommentar om trädets kondition.", DataType = "string" }
            }
        },
        new Project
        {
            Id = 5368,
            IsPublic = true,
            Category = "Environmental management",
            CategorySwedish = "Naturvård",
            Description = "Skyddsvärda träd utgick från Åtgärdsprogrammet för särskilt skyddsvärda träd under perioden 2004–2016.",
            Name = "Skyddsvärda träd",
            Owner = "Karin Sandberg",
            ProjectURL = "https://www.artportalen.se/Project/View/5368",
            StartDate = new DateTime(2004, 1, 1),
            EndDate = new DateTime(2016, 12, 31),
            SurveyMethod = "Fältinventering",
            SurveyMethodUrl = "https://www.artportalen.se/Project/View/5368",
            ProjectParameters = new List<ProjectParameter>
            {
                new ProjectParameter { Name = "Trädstatus", Description = "Kommentar om trädets tillstånd.", DataType = "string" },
                new ProjectParameter { Name = "Vitalitet levande träd (%)", Description = "Bedömd vitalitet i procent.", Unit = "%", DataType = "double" },
                new ProjectParameter { Name = "Stamomkrets (cm)", Description = "Omkrets mätt vid 1,3 m höjd.", Unit = "cm", DataType = "double" },
                new ProjectParameter { Name = "Hålstadium", Description = "Största ingångshål till hålighet.", DataType = "string" },
                new ProjectParameter { Name = "Mulmvolym", Description = "Uppskattad mängd mulm.", DataType = "string" }
            }
        },
        new Project
        {
            Id = 5998,
            IsPublic = false,
            Category = "Regional monitoring",
            CategorySwedish = "Regional miljöövervakning",
            Description = "Fynddata från barmarksinventering av utter inom regional miljöövervakning.",
            Name = "Utter barmarksinventering RMÖ/GDP",
            Owner = "Länsstyrelsen Norrbotten",
            ProjectURL = "https://www.artportalen.se/Project/View/5998",
            StartDate = new DateTime(2010, 1, 1),
            SurveyMethod = "Barmarksinventering",
            SurveyMethodUrl = "https://www.artportalen.se/Project/View/5998",
            ProjectParameters = new List<ProjectParameter>
            {
                new ProjectParameter { Name = "Vattenstånd", Description = "Välj alternativ. Obligatorisk.", DataType = "string" },
                new ProjectParameter { Name = "Inventeringssträcka", Description = "Inventerad sträcka i meter.", Unit = "m", DataType = "double" },
                new ProjectParameter { Name = "Uttertecken", Description = "Typ av spår/fynd.", DataType = "string" },
                new ProjectParameter { Name = "Färsk spillning", Description = "Antal observationer av färsk spillning.", Unit = "st", DataType = "double" },
                new ProjectParameter { Name = "Miljö Sjö", Description = "Förekomst av sjömiljö (Ja/Nej).", DataType = "string" }
            }
        },
        new Project
        {
            Id = 3446,
            IsPublic = false,
            Category = "Biogeographical monitoring",
            CategorySwedish = "Biogeografisk uppföljning",
            Description = "Projekt inom biogeografisk uppföljning av buskmus i Sverige.",
            Name = "Biogeografisk uppföljning av buskmus",
            Owner = "SLU Artdatabanken",
            ProjectURL = "https://www.artportalen.se/Project/View/3446",
            StartDate = new DateTime(2013, 1, 1),
            EndDate = new DateTime(2021, 12, 31),
            SurveyMethod = "Fångst och observation",
            SurveyMethodUrl = "https://www.artportalen.se/Project/View/3446",
            ProjectParameters = new List<ProjectParameter>
            {
                new ProjectParameter { Name = "Fångstmetod", Description = "Typ av fälla/metod som användes.", DataType = "string" },
                new ProjectParameter { Name = "Antal individer", Description = "Antal fångade individer.", Unit = "st", DataType = "int" },
                new ProjectParameter { Name = "Koordinatkvalitet", Description = "Bedömning av positionens noggrannhet.", DataType = "string" },
                new ProjectParameter { Name = "Temperatur", Description = "Lufttemperatur vid observation.", Unit = "°C", DataType = "double" },
                new ProjectParameter { Name = "Kommentar", Description = "Fältanteckningar.", DataType = "string" }
            }
        },
        new Project
        {
            Id = 3455,
            IsPublic = false,
            Category = "Biogeographical monitoring",
            CategorySwedish = "Biogeografisk uppföljning",
            Description = "Projekt som omfattar bifångst av buskmus under biogeografisk uppföljning.",
            Name = "Biogeografisk uppföljning av buskmus - bifångst",
            Owner = "SLU Artdatabanken",
            ProjectURL = "https://www.artportalen.se/Project/View/3455",
            StartDate = new DateTime(2013, 1, 1),
            EndDate = new DateTime(2021, 12, 31),
            SurveyMethod = "Fångst och observation",
            SurveyMethodUrl = "https://www.artportalen.se/Project/View/3455",
            ProjectParameters = new List<ProjectParameter>
            {
                new ProjectParameter { Name = "Fångstmetod", Description = "Typ av fälla/metod som användes.", DataType = "string" },
                new ProjectParameter { Name = "Artbestämning", Description = "Bedömning av artidentitet.", DataType = "string" },
                new ProjectParameter { Name = "Individens kön", Description = "Hane/hona/okänt.", DataType = "string" },
                new ProjectParameter { Name = "Antal individer", Description = "Antal fångade individer.", Unit = "st", DataType = "int" },
                new ProjectParameter { Name = "Kommentar", Description = "Fältanteckningar.", DataType = "string" }
            }
        }
    };

    private static List<Lib.Models.Processed.Observation.ArtportalenDatasetMetadata> _artportalenDatasetMetadatas = new List<Lib.Models.Processed.Observation.ArtportalenDatasetMetadata>
    {
        new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata
        {
            Id = 12,
            Identifier = "Artportalen - GDP Skyddsvärda träd",
            Metadatalanguage = "Swedish",
            Language = "Swedish",
            DataStewardship = "Datavärdskap Naturdata: Arter",
            StartDate = DateTime.Parse("1918-12-31T23:00:00.000Z"),
            Description = "Alla träd och fynd från gamla trädportalen",
            Title = "GDP Skyddsvärda träd",
            Spatial = "Sverige",
            DescriptionAccessRights = "",
            Projects = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Project
                {
                    Id = 23,
                    ProjectId = "Artportalen ProjectId:5367",
                    ProjectCode = "Trädportalen",
                    ProjectType = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProjectType { Id = 0, Value = "Artportalen projekt" },
                    ApProjectId = 5367
                },
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Project
                {
                    Id = 24,
                    ProjectId = "Artportalen ProjectId:5368",
                    ProjectCode = "Skyddsvärda träd",
                    ProjectType = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProjectType { Id = 0, Value = "Artportalen projekt" },
                    ApProjectId = 5368
                }
            },
            Methodologies = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Methodology
                {
                    Id = 5,
                    Description = "Se: https://www.naturvardsverket.se/4908d7/globalassets/vagledning/miljoovervakning/handledning/undersokningstyper/inventering-av-skyddsvarda-trad-1-3-21-09-27-final-2.pdf",
                    Link = "https://www.naturvardsverket.se/4908d7/globalassets/vagledning/miljoovervakning/handledning/undersokningstyper/inventering-av-skyddsvarda-trad-1-3-21-09-27-final-2.pdf",
                    Name = "Metod för Skyddsvärda träd"
                }
            },
            Creators = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" }
            },
            Assigner = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 12, Code = "Länsstyrelsen" },
            OwnerInstitution = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" },
            Publisher = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" },
            DatasetAccessRights = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.AccessRights { Id = 0, Value = "Publik" },
            DatasetPurpose = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Purpose { Id = 1, Value = "Regional miljöövervakning" },
            DatasetProgrammeArea = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProgrammeArea { Id = 4, Value = "Landskap" }
        },
        new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata
        {
            Id = 13,
            Identifier = "Artportalen - GDP Utter barmarksinventering",
            Metadatalanguage = "Swedish",
            Language = "Swedish",
            DataStewardship = "Datavärdskap Naturdata: Arter",
            StartDate = DateTime.Parse("2008-12-31T23:00:00.000Z"),
            Description = "Fynddata från barmarksinventering av utter inom regional miljöövervakning.",
            Title = "GDP Utter barmarksinventering",
            Spatial = "Sverige",
            Projects = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Project
                {
                    Id = 25,
                    ProjectId = "Artportalen ProjectId:5998",
                    ProjectCode = "Utter barmarksinventering RMÖ/GDP",
                    ProjectType = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProjectType { Id = 0, Value = "Artportalen projekt" },
                    ApProjectId = 5998
                }
            },
            Methodologies = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Methodology
                {
                    Id = 6,
                    Description = "Se: https://www.naturvardsverket.se/4a6631/globalassets/vagledning/miljoovervakning/handledning/undersokningstyper/utterforekomst-barmarksinventering-undersokningstyp-v1-2017-12-13.pdf",
                    Link = "https://www.naturvardsverket.se/4a6631/globalassets/vagledning/miljoovervakning/handledning/undersokningstyper/utterforekomst-barmarksinventering-undersokningstyp-v1-2017-12-13.pdf",
                    Name = "Metod för Utter barmarksinventering"
                }
            },
            Creators = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 5, Code = "Länsstyrelsen Norrbotten" }
            },
            Assigner = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 12, Code = "Länsstyrelsen" },
            OwnerInstitution = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 5, Code = "Länsstyrelsen Norrbotten" },
            Publisher = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" },
            DatasetAccessRights = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.AccessRights { Id = 0, Value = "Publik" },
            DatasetPurpose = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Purpose { Id = 1, Value = "Regional miljöövervakning" },
            DatasetProgrammeArea = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProgrammeArea { Id = 4, Value = "Landskap" }
        },
        new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata
        {
            Id = 5,
            Identifier = "Artportalen - Buskmus",
            Metadatalanguage = "Swedish",
            Language = "Swedish",
            DataStewardship = "Datavärdskap Naturdata: Arter",
            StartDate = DateTime.Parse("2012-12-31T23:00:00.000Z"),
            EndDate = DateTime.Parse("2021-12-30T23:00:00.000Z"),
            Title = "Buskmus",
            Spatial = "Sverige",
            Projects = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Project
                {
                    Id = 5,
                    ProjectId = "Artportalen ProjectId:3446",
                    ProjectCode = "Biogeografisk uppföljning av buskmus",
                    ProjectType = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProjectType { Id = 0, Value = "Artportalen projekt" },
                    ApProjectId = 3446
                },
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Project
                {
                    Id = 6,
                    ProjectId = "Artportalen ProjectId:3455",
                    ProjectCode = "Biogeografisk uppföljning av buskmus - bifångst",
                    ProjectType = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProjectType { Id = 0, Value = "Artportalen projekt" },
                    ApProjectId = 3455
                }
            },
            Methodologies = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Methodology
                {
                    Id = 2,
                    Description = "Metod ej publicerad",
                    Link = "",
                    Name = "Metod ej publicerad"
                }
            },
            Creators = new[]
            {
                new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" }
            },
            Assigner = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 1, Identifier = "2021001975", Code = "Naturvårdsverket" },
            OwnerInstitution = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" },
            Publisher = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Organisation { Id = 2, Code = "SLU Artdatabanken" },
            DatasetAccessRights = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.AccessRights { Id = 0, Value = "Publik" },
            DatasetPurpose = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.Purpose { Id = 2, Value = "Biogeografisk uppföljning" },
            DatasetProgrammeArea = new Lib.Models.Processed.Observation.ArtportalenDatasetMetadata.ProgrammeArea { Id = 0, Value = "Biogeografisk uppföljning av naturtyper och arter" }
        }
    };
}