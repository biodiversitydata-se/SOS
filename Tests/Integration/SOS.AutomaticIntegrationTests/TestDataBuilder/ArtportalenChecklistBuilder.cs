using FizzWare.NBuilder;
using FizzWare.NBuilder.Implementation;
using NetTopologySuite.Geometries;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Enums.Artportalen;

namespace SOS.AutomaticIntegrationTests.TestDataBuilder
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
                    var assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var filePath = System.IO.Path.Combine(assemblyPath, @"Resources\ArtportalenVerbatimChecklists_1000.json");                    
                    string str = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                    var serializerSettings = new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { 
                            new TestHelpers.JsonConverters.ObjectIdConverter(),
                            new NewtonsoftGeoJsonGeometryConverter()                            
                        }
                    };

                    _verbatimArtportalenChecklistsFromJsonFile = JsonConvert.DeserializeObject<List<ArtportalenChecklistVerbatim>>(str, serializerSettings);
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
                checklist.Id = sourceChecklist.Id;
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