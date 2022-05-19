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

        public static List<ArtportalenCheckListVerbatim> VerbatimArtportalenChecklistsFromJsonFile
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

                    _verbatimArtportalenChecklistsFromJsonFile = JsonConvert.DeserializeObject<List<ArtportalenCheckListVerbatim>>(str, serializerSettings);
                }

                return _verbatimArtportalenChecklistsFromJsonFile;
            }
        }
        private static List<ArtportalenCheckListVerbatim> _verbatimArtportalenChecklistsFromJsonFile;        
       
        public static IOperable<ArtportalenCheckListVerbatim> HaveValuesFromPredefinedChecklists(this IOperable<ArtportalenCheckListVerbatim> operable)
        {
            var builder = ((IDeclaration<ArtportalenCheckListVerbatim>)operable).ObjectBuilder;
            builder.With((checklist, index) =>
            {
                var sourceChecklist = Pick<ArtportalenCheckListVerbatim>.RandomItemFrom(VerbatimArtportalenChecklistsFromJsonFile).Clone();
                checklist.Id = sourceChecklist.Id;
                checklist.Id = _faker.IndexVariable++;
                checklist.ControllingUser = sourceChecklist.ControllingUser;
                checklist.ControlingUserId = sourceChecklist.ControlingUserId;
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