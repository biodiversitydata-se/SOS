using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using Area = SOS.Lib.Models.Processed.Observation.Area;
using Location = SOS.Lib.Models.Processed.Observation.Location;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors.Artportalen
{
    public class ArtportalenChecklistFactory : ChecklistFactoryBase, IChecklistFactory<ArtportalenChecklistVerbatim>
    {
        /// <summary>
        /// Cast verbatim area to processed area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        private static Area? CastToArea(GeographicalArea area)
        {
            if (area == null)
            {
                return null;
            }

            return new Area
            {
                FeatureId = area.FeatureId,
                Name = area.Name
            };
        }

        private Event CreateEvent(ArtportalenChecklistVerbatim verbatim, string id)
        {
            var evnt = new Event(verbatim.StartDate, verbatim.EndDate)
            {
                EventId = id,
                SamplingProtocol = verbatim.Project?.SurveyMethod ?? verbatim.Project?.SurveyMethodUrl,
            };

            return evnt;
        }

        /// <summary>
        /// Create location object
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        private Location CreateLocation(ArtportalenChecklistVerbatim verbatim)
        {
            var location = new Location(LocationType.Point);

            if (verbatim.Site == null)
            {
                location.Locality = verbatim.Name;
                AddPositionData(location, verbatim.OccurrenceXCoord, verbatim.OccurrenceYCoord,
                    CoordinateSys.Rt90_25_gon_v, 0, 0);

                return location;
            }

            var point = (Point)verbatim.Site?.Point?.ToGeometry()!;

            var site = verbatim.Site;
            location.Attributes.CountyPartIdByCoordinate = site!.CountyPartIdByCoordinate;
            location.Attributes.ProvincePartIdByCoordinate = site!.ProvincePartIdByCoordinate;
            location.CountryRegion = CastToArea(site!.CountryRegion!);
            location.County = CastToArea(site!.County!);
            location.Locality = site!.Name.Trim();
            location.LocationId = $"urn:lsid:artportalen.se:site:{site!.Id}";
            location.Municipality = CastToArea(site!.Municipality!);
            location.Parish = CastToArea(site!.Parish!);
            location.Province = CastToArea(site!.Province!);
            location.Type = site!.HasGeometry ? LocationType.Polygon : LocationType.Point;

            AddPositionData(location, site.XCoord,
                site.YCoord,
                CoordinateSys.WebMercator,
                point,
                site.PointWithBuffer,
                site.Accuracy,
                0);

            return location;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        public ArtportalenChecklistFactory(
            DataProvider dataProvider,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
        }

        /// <summary>
        ///     Cast verbatim checklist to processed data model
        /// </summary>
        /// <param name="verbatimChecklist"></param>
        /// <returns></returns>
        public Checklist? CreateProcessedChecklist(ArtportalenChecklistVerbatim verbatimChecklist)
        {
            try
            {
                if (verbatimChecklist == null)
                {
                    return null;
                }

                var id = $"urn:lsid:artportalen.se:Checklist:{verbatimChecklist.Id}";
                return new Checklist
                {
                    ArtportalenInternal = new ApInternal()
                    {
                        ChecklistId = verbatimChecklist.Id,
                        ParentTaxonId = verbatimChecklist.ParentTaxonId,
                        UserId = verbatimChecklist.ControllingUserId
                    },
                    DataProviderId = DataProvider.Id,
                    Id = id,
                    Event = CreateEvent(verbatimChecklist, id),
                    Location = CreateLocation(verbatimChecklist),
                    Modified = verbatimChecklist.EditDate,
                    Name = verbatimChecklist.Name,
                    OccurrenceIds =
                        verbatimChecklist.SightingIds?.Select(sId => $"urn:lsid:artportalen.se:Sighting:{sId}"),
                    Project = ArtportalenFactoryHelper.CreateProcessedProject(verbatimChecklist.Project),
                    RecordedBy = verbatimChecklist.ControllingUser,
                    RegisterDate = verbatimChecklist.RegisterDate,
                    TaxonIds = verbatimChecklist.TaxonIds,
                    TaxonIdsFound = verbatimChecklist.TaxonIdsFound
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing Artportalen verbatim checklist with Id={verbatimChecklist.Id}", e);
            }
        }
    }
}