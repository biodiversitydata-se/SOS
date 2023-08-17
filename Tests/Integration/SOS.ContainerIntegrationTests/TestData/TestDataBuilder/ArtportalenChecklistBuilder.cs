using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using SOS.ContainerIntegrationTests.Extensions;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Verbatim.Artportalen;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SOS.ContainerIntegrationTests.TestData.TestDataBuilder
{
    public static class ArtportalenChecklistBuilder
    {
        private static Bogus.Faker _faker = new Bogus.Faker();
        private static Bogus.DataSets.Lorem _lorem = new Bogus.DataSets.Lorem("sv");

        public static List<ArtportalenChecklistVerbatim> VerbatimArtportalenChecklistsFromJsonFile
        {
            get
            {
                if (_verbatimArtportalenChecklistsFromJsonFile == null)
                {                    
                    string filePath = "Resources/TestDataBuilder/ArtportalenVerbatimChecklists_1000.json".GetAbsoluteFilePath();
                    string str = File.ReadAllText(filePath, Encoding.UTF8);

                    var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                    serializeOptions.Converters.Add(new ObjectIdConverter());
                    serializeOptions.Converters.Add(new GeoJsonConverter());
                    _verbatimArtportalenChecklistsFromJsonFile = JsonSerializer.Deserialize<List<ArtportalenChecklistVerbatim>>(str, serializeOptions);
                }

                return _verbatimArtportalenChecklistsFromJsonFile;
            }
        }
        private static List<ArtportalenChecklistVerbatim> _verbatimArtportalenChecklistsFromJsonFile;

        public static IOperable<ArtportalenChecklistVerbatim> HaveValuesFromPredefinedChecklists(this IOperable<ArtportalenChecklistVerbatim> operable)
        {
            var builder = ((IDeclaration<ArtportalenChecklistVerbatim>)operable).ObjectBuilder;
            builder.With((checklist, index) =>
            {
                var sourceChecklist = Pick<ArtportalenChecklistVerbatim>.RandomItemFrom(VerbatimArtportalenChecklistsFromJsonFile).Clone();
                checklist.Id = _faker.IndexVariable++;
                checklist.ControllingUser = sourceChecklist.ControllingUser;
                checklist.ControllingUserId = sourceChecklist.ControllingUserId;
                checklist.EditDate = sourceChecklist.EditDate;
                checklist.EndDate = sourceChecklist.EndDate;
                checklist.Name = sourceChecklist.Name;
                checklist.OccurrenceRange = sourceChecklist.OccurrenceRange;
                checklist.OccurrenceXCoord = sourceChecklist.OccurrenceXCoord;
                checklist.OccurrenceYCoord = sourceChecklist.OccurrenceYCoord;
                checklist.ParentTaxonId = sourceChecklist.ParentTaxonId;
                checklist.Project = sourceChecklist.Project;
                checklist.RegisterDate = sourceChecklist.RegisterDate;
                checklist.SightingIds = sourceChecklist.SightingIds;
                checklist.Site = sourceChecklist.Site;
                checklist.StartDate = sourceChecklist.StartDate;
                checklist.TaxonIds = sourceChecklist.TaxonIds;
                checklist.TaxonIdsFound = sourceChecklist.TaxonIdsFound;
            });

            return operable;
        }
    }
}